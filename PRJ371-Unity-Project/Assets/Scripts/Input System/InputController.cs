using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class InputController : MonoBehaviour
{
    //Editor Variables
    [SerializeField] private WheelCollider[] _wheelColliders;
    [SerializeField] private float torque;
    [SerializeField] private float _maxSteeringAngle;
    [SerializeField] private float _maxBrakingTorque;
    [SerializeField] private AudioClip _skidSoundEffect;
    [SerializeField] private float _skidThreshold;

    //Global variables
    private PlayerInput playerInput;
    private float braking;
    private AudioSource _audioSource;
    private TrafficLight trafficLight;
    private RoadMaintenanceBeacon roadMaintenanceBeacon;

    //Arduino car variables
    [SerializeField] private GameObject car;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxTorque;
    [SerializeField] private float torqueChange;
    private apiEvents _apiRequest;
    public apiEvents apiRequest { get { return _apiRequest; } set { _apiRequest = value; } }
    private GameObject _infrastructureObj;
    private float speed;
    private int speedCalc;
    private float torqueCalc;
    private float torqueNow;
    private Transform carTrans;
    private FaceDir _currentFaceDir;
    private FaceDir _currentRoadDir;
    private float carDistance;
    private float initialCarDistance;
    private float beaconDistanceLeft;
    private float timeBetweenObjs;
    private float distanceBeforeM;
    private bool col1Triggered;
    private bool initialDistance;
    private bool _pastBeacon;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    public Dictionary<string, Tuple<bool, bool>> dualColDic { get {return _dualColDic;} set {_dualColDic = value;} }
    private CollisionDetection[] _allColliders;
    private CollisionDetection collisionDetection;
    private StopSign stopSign;
    public FaceDir currentFaceDir { get {return _currentFaceDir;} set {_currentFaceDir = value;}}
    public FaceDir currentRoadDir { get { return _currentRoadDir; } set { _currentRoadDir = value; } }
    //Arduino events enum
    public enum apiEvents
    {
        GO,
        SLOWDOWN,
        WARNING,
        STOP,
        SENDINFO,
        UPDATEDIR
    }
    public enum FaceDir
    {
        North,
        South,
        East,
        West,
        Bend
    }
    //Arduino event setting
    public void ReceiveApiRequest(apiEvents request)
    {
        apiRequest = request;
        Debug.Log("1 ~ Triggered " + apiRequest + " Event!");
    }
    public void ReceiveApiObjRequest(apiEvents request, GameObject infrastructureObj)
    {
        initialDistance = true;
        apiRequest = request;
        _infrastructureObj = infrastructureObj;
        Debug.Log("2 ~ Triggered " + apiRequest + " Event!");
    }
    private void PopulateInfColDic()
    {
        _allColliders = collisionDetection.RetreiveAllColliders();
        dualColDic = new Dictionary<string, Tuple<bool, bool>>();
        foreach (CollisionDetection collision in _allColliders)
        {
            if (!collision.tag.Contains("Road"))// && !collision.tag.Contains("Beacon"))
            {
                Debug.LogError(dualColDic);
                if (!dualColDic.ContainsKey(collision.tag))
                {
                    Debug.LogError("Entered Dic Col");
                    dualColDic.Add(collision.tag, Tuple.Create(false, false));
                }
                else
                {
                    Debug.LogError("Exited Dic Col");
                    //return;
                }
            }
        }
        foreach (var collider in dualColDic)
        {
            Debug.LogError($"Key: {collider.Key} Col1: {collider.Value.Item1} Col2: {collider.Value.Item2}");
        }
    }
    public void CarDirectionCalc()
    {
        if (carTrans.localEulerAngles.y > 45f && carTrans.localEulerAngles.y <= 135f)
        {
            currentFaceDir = FaceDir.North;
        }
        else if (carTrans.localEulerAngles.y > 135f && carTrans.localEulerAngles.y <= 225f)
        {
            currentFaceDir = FaceDir.East;
        }
        else if (carTrans.localEulerAngles.y > 225f && carTrans.localEulerAngles.y <= 315f)
        {
            currentFaceDir = FaceDir.South;
        }
        else
        {
            currentFaceDir = FaceDir.West;
        }
        Debug.Log(currentFaceDir);
        //Debug.Log(carTrans.localEulerAngles.y);
    }
    public void ColliderTriggered(GameObject current, bool enteredCol)
    {
        if (current.tag.Contains("Bend"))
        {
            if (enteredCol)
            {
                ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
            }
            else
            {
                ReceiveApiRequest(InputController.apiEvents.GO);
            }
        }
    }
    private void Start()
    {
        col1Triggered = false;
    }
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("Player input component is missing");
        }
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("Car missing audio source");
        }
        trafficLight = FindObjectOfType<TrafficLight>();
        if (trafficLight == null)
        {
            Debug.LogError("No trafficLight script in the scene");
        }
        roadMaintenanceBeacon = FindObjectOfType<RoadMaintenanceBeacon>();
        if (roadMaintenanceBeacon == null)
        {
            Debug.LogError("No roadMaintenanceBeacon script in the scene");
        }
        collisionDetection = FindObjectOfType<CollisionDetection>();
        if (collisionDetection == null)
        {
            Debug.LogError("No collisionDetection script in the scene");
        }
        stopSign = FindObjectOfType<StopSign>();
        if (stopSign == null)
        {
            Debug.LogError("No stopSign script in the scene");
        }
        PopulateInfColDic();
        ReceiveApiRequest(apiEvents.GO);
        carTrans = car.GetComponent<Transform>();
    }
    public void Braking(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            braking = 1f;
        }
        else if (context.canceled)
        {
            braking = 0f;
        }
    }
    private void SkidCheck()
    {
        int skidCount = 0;
        //Logic for adding Skid particals and playing audio
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            WheelHit wheelHit;
            _wheelColliders[i].GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.forwardSlip) >= _skidThreshold ||
                Mathf.Abs(wheelHit.sidewaysSlip) >= _skidThreshold)
            {
                skidCount++;

                if (!_audioSource.isPlaying)
                {
                    _audioSource.PlayOneShot(_skidSoundEffect);
                }
            }
        }

        //Checks if wheels is not skidding to turn the sound off
        if (skidCount == 0 & _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
    void Update()
    {
        //float acceleration = Input.GetAxis("Vertical");
        //float steering = Input.GetAxis("Horizontal");
        Vector2 moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        float acceleration = moveInput.y;
        float steering = moveInput.x;

        //Move(acceleration, steering, braking);
        SkidCheck();
        //if (Keyboard.current.spaceKey.wasPressedThisFrame)
        //{
        //    Debug.Log("Spacebar key was pressed");
        //}
        //speed = ((car.GetComponent<Rigidbody>().velocity.magnitude * 3.6) <= maxSpeed) ? 
            //MathF.Floor((float)(car.GetComponent<Rigidbody>().velocity.magnitude * 3.6)) : maxSpeed;
        //Debug.Log(speed.ToString("f0"));
        switch (apiRequest)
        {
            case apiEvents.GO:
                MoveAccelerate(acceleration, steering, braking);
                // Incrementally speed up the car to normal speed
                //speed = Mathf.Min(speed + speedChange * Time.deltaTime, 10.0f);
                break;
            case apiEvents.SLOWDOWN:
                MoveDecelerate(acceleration, steering, braking);
                // Incrementally slow down the car
                //speed = Mathf.Max(speed - speedChange * Time.deltaTime, 5.0f);
                break;
            case apiEvents.WARNING:
                MoveDecelerate(acceleration, steering, braking);
                // Incrementally slow down the car
                //speed = Mathf.Max(speed - speedChange * Time.deltaTime, 5.0f);
                break;
            case apiEvents.STOP:
                MoveStop(acceleration, steering, braking);
                // Incrementally bring the car to a stop
                //speed = Mathf.Max(speed - speedChange * Time.deltaTime, 0.0f);
                break;
            case apiEvents.SENDINFO:
                MoveAccelerate(acceleration, steering, braking);
                TimeDistanceCalc(_infrastructureObj);
                // Incrementally bring the car to a stop
                //speed = Mathf.Max(speed - speedChange * Time.deltaTime, 0.0f);
                break;
            case apiEvents.UPDATEDIR:
                Debug.LogError(_infrastructureObj);
                if (_infrastructureObj.tag.Contains("Road"))
                {
                    MoveAccelerate(acceleration, steering, braking);
                    //trafficLight.UpdateCarInfo(_currentFaceDir, _currentRoadDir);
                    ReceiveApiRequest(apiEvents.GO);
                }
                else if (_infrastructureObj.tag.Contains("Beacon"))
                {
                    MoveDecelerate(acceleration, steering, braking);
                    var result = UpdateDistanceCalc(_infrastructureObj);
                    //roadMaintenanceBeacon.UpdateCarInfo(result.carDistance, result.beaconDistanceLeft, result.pastBeacon);
                    ReceiveApiRequest(apiEvents.WARNING);
                }
                //else if (_infrastructureObj.tag.Contains("StopS"))
                //{
                //    MoveAccelerate(acceleration, steering, braking);
                //    stopSign.UpdateCarInfo(_currentFaceDir, _currentRoadDir);
                //    ReceiveApiRequest(apiEvents.GO);
                //}
                // Incrementally bring the car to a stop
                //speed = Mathf.Max(speed - speedChange * Time.deltaTime, 0.0f);
                break;
            default:
                // Do nothing (or maintain current speed)
                break;
        }
    }
    private void LateUpdate()
    {
        speed = (speedCalc <= maxSpeed) ? speedCalc : maxSpeed;
        //Debug.Log(speed);
        if (speed <= minSpeed)
        {
            Debug.Log("Min Speed Reached");
        }
    }
    private void FixedUpdate()
    {
        speedCalc = int.Parse(MathF.Floor((float)(car.GetComponent<Rigidbody>().velocity.magnitude * 3.6)).ToString("f0"));
    }
    private (float beaconDistanceLeft, float carDistance, bool pastBeacon) UpdateDistanceCalc(GameObject infrastructureObj)
    {
        carDistance = Vector3.Distance(car.transform.position, infrastructureObj.transform.position);

        if (carDistance < 0)
        {
            beaconDistanceLeft = initialCarDistance - carDistance;
            _pastBeacon = true;
        }
        else
        {
            beaconDistanceLeft = 0f;
            _pastBeacon = false;
        }

        return (beaconDistanceLeft, carDistance, _pastBeacon);
    }
    private void DistanceCalc(GameObject infrastructureObj)
    {
        //Call method to update Beacon UI
        //if (infrastructureObj.tag.Contains("Beacon"))
        //{
        //    var obj1 = infrastructureObj.GetComponents<SphereCollider>()[0];
        //    var obj2 = infrastructureObj.GetComponents<SphereCollider>()[1];
        //    var calc = (obj2.radius - obj1.radius) / 3.6f;
        //    //Debug.LogError(calc);
        //}



        //if (initialDistance)
        //{
        //    initialCarDistance = Vector3.Distance(car.transform.position, infrastructureObj.transform.position);
        //    initialDistance = false;
        //}
        //carDistance = Vector3.Distance(car.transform.position, infrastructureObj.transform.position);

        //if (carDistance >= 0)
        //{
        //    roadMaintenanceBeacon.ReceiveCarInfo(carDistance, currentFaceDir, currentRoadDir, true);
        //}
        //else if (carDistance < 0)
        //{
        //    beaconDistanceLeft = initialCarDistance - carDistance;
        //    roadMaintenanceBeacon.ReceiveCarInfo(beaconDistanceLeft, currentFaceDir, currentRoadDir, false);
        //}
    }
    private void TimeDistanceCalc(GameObject infrastructureObj)
    {
        carDistance = Vector3.Distance(car.transform.position, infrastructureObj.transform.position);
        timeBetweenObjs = (carDistance / speedCalc) * 3.6f;

        if (infrastructureObj.tag.Contains("Beacon") && !col1Triggered)
        {
            var obj1 = infrastructureObj.GetComponents<SphereCollider>()[0];
            var obj2 = infrastructureObj.GetComponents<SphereCollider>()[1];
            distanceBeforeM = (obj2.radius - obj1.radius) / 3.6f;
            col1Triggered = true;
            //Debug.LogError(calc);
        }

        //Debug.LogError(timeBetweenObjs);
        //Debug.Log(carDistance);
        //Debug.Log(speedCalc);
        //Debug.Log(timeBetweenObjs);
        CarDirectionCalc();

        switch (infrastructureObj.tag)
        {
            case "Beacon":
                roadMaintenanceBeacon.ReceiveCarInfo(distanceBeforeM);
                break;
            case "North StopS":
                //trafficLight.ReceiveCarInfo(timeBetweenObjs, currentFaceDir);
                stopSign.ReceiveCarInfo(_currentFaceDir, _currentRoadDir);
                break;
            case "North TrafficL":
                Debug.LogError("North TrafficL");
                Debug.LogError("Input RoadDir " + currentRoadDir);
                Debug.LogError("Input CarDir " + currentFaceDir);
                trafficLight.ReceiveCarInfo(timeBetweenObjs);
                break;
            case "East TrafficL":
                Debug.LogError("East TrafficL");
                Debug.LogError("Input RoadDir " + currentRoadDir);
                Debug.LogError("Input CarDir " + currentFaceDir);
                trafficLight.ReceiveCarInfo(timeBetweenObjs);
                break;
            case "West TrafficL":
                Debug.LogError("West TrafficL");
                Debug.LogError("Input RoadDir " + currentRoadDir);
                Debug.LogError("Input CarDir " + currentFaceDir);
                trafficLight.ReceiveCarInfo(timeBetweenObjs);
                break;
            default:
                break;
        }
    }
    private void MoveAccelerate(float acceleration, float steering, float braking)
    {
        //Wheel variable declaration
        Quaternion quaternion;
        Vector3 position;

        //Clamed values to -1,1 range
        acceleration = Mathf.Clamp(acceleration, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f) * _maxSteeringAngle;
        braking = Mathf.Clamp(braking, 0f, 1f) * _maxBrakingTorque;

        //float thrustTorque = acceleration * torque;
        Quaternion newRotation = Quaternion.AngleAxis(90, Vector3.up);

        if (acceleration > 0.1)
        {
            if (speed < 2)
            {
                if (torqueCalc < maxTorque)
                {
                    torqueCalc += torqueChange;
                }
                torqueNow = acceleration * torqueCalc;
            }
        }
        else if (acceleration < -0.1)
        {
            if (speed < 2)
            {
                if (torqueCalc < maxTorque)
                {
                    torqueCalc += torqueChange;
                }
                torqueNow = acceleration * torqueCalc;
                //Debug.Log("Check "+speed);
            }
        }
        else if (acceleration < 0.1 || acceleration > -0.1)
        {
            torqueNow = 0f;
            torqueCalc = 0f;
        }

        foreach (var wheel in _wheelColliders)
        {
            //wheel.motorTorque = thrustTorque;

            //Gets world position & rotation of the wheels and sets it in mesh
            wheel.GetWorldPose(out position, out quaternion);
            wheel.transform.GetChild(0).transform.position = position;
            wheel.transform.GetChild(0).transform.rotation = quaternion;
            //Debug.Log("Accelerating Speed");
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,0.0005f);
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,Time.deltaTime*0.0000005f);
        }
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            //if (speed < maxSpeed)
            //{
            //    _wheelColliders[i].motorTorque = thrustTorque;
            //}
            //else
            //{
            //    _wheelColliders[i].motorTorque = 0;
            //}
            if (speed < maxSpeed)
            {
                _wheelColliders[i].motorTorque = torqueNow;
                _wheelColliders[i].brakeTorque = 0f;
                Debug.Log("Under Min Speed & Not Braking");
            }
            else if ((torqueNow > 0.1 || torqueNow < -0.1) && braking <= 0)
            {
                _wheelColliders[i].motorTorque = 0;
                _wheelColliders[i].brakeTorque = 0.6f * _maxBrakingTorque;
                //_wheelColliders[i].motorTorque = thrustTorque;
                Debug.Log("Slowing Down & Switch over to Stop");
                //ReceiveApiRequest(apiEvents.STOP);
            }
            if (i < 2)
            {
                _wheelColliders[i].steerAngle = steering;
            }
            else
            {
                _wheelColliders[i].brakeTorque = braking;
                if (braking > 0)
                {
                    Debug.Log("Braking");
                    _wheelColliders[i].motorTorque = 0;
                }
            }
        }
    }
    private void MoveDecelerate(float acceleration, float steering, float braking)
    {
        //Wheel variable declaration
        Quaternion quaternion;
        Vector3 position;

        //Clamed values to -1,1 range
        acceleration = Mathf.Clamp(acceleration, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f) * _maxSteeringAngle;
        braking = Mathf.Clamp(braking, 0f, 1f) * _maxBrakingTorque;
        //speedChange = Mathf.Clamp(speedChange, 0f, 1f);

        //float thrustTorque = acceleration * torque;
        //float torqueNow = (acceleration > 0.1) ? (torqueCalc < maxSpeed)  torqueCalc++ : 0f;

        if (acceleration > 0.1)
        {
            if (speed < 2)
            {
                if (torqueCalc < maxTorque)
                {
                    torqueCalc += torqueChange;
                }
                torqueNow = acceleration * torqueCalc;
            }
        }
        else if (acceleration < -0.1)
        {
            if (speed < 2)
            {
                if (torqueCalc < maxTorque)
                {
                    torqueCalc += torqueChange;
                }
                torqueNow = acceleration * torqueCalc;
                //Debug.Log("Check "+speed);
            }
        }
        else if (acceleration < 0.1 || acceleration > -0.1)
        {
            torqueNow = 0f;
            torqueCalc = 0f;
        }

        //Debug.Log("accelerate"+acceleration);
        //Debug.Log("Speed"+torqueNow);
        Quaternion newRotation = Quaternion.AngleAxis(90, Vector3.up);

        foreach (var wheel in _wheelColliders)
        {
            //wheel.motorTorque = thrustTorque;

            //Gets world position & rotation of the wheels and sets it in mesh
            wheel.GetWorldPose(out position, out quaternion);
            wheel.transform.GetChild(0).transform.position = position;
            wheel.transform.GetChild(0).transform.rotation = quaternion;
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,0.0005f);
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,Time.deltaTime*0.0000005f);
        }
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            if (speed >= minSpeed)
            {
                _wheelColliders[i].motorTorque = 0;
                _wheelColliders[i].brakeTorque = 0.6f * _maxBrakingTorque;
                //_wheelColliders[i].motorTorque = thrustTorque;
                Debug.Log("Slowing Down");
            }
            else if ((torqueNow > 0.1 || torqueNow < -0.1) && braking <= 0)
            {
                _wheelColliders[i].motorTorque = torqueNow;
                _wheelColliders[i].brakeTorque = 0f;
                Debug.Log("Under Min Speed & Not Braking");
            }
            //Debug.Log(torqueNow);
            if (i < 2)
            {
                _wheelColliders[i].steerAngle = steering;
            }
            else
            {
                _wheelColliders[i].brakeTorque = braking;
                if (braking > 0)
                {
                    Debug.Log("Braking");
                    _wheelColliders[i].motorTorque = 0;
                }
            }
        }
    }
    private void MoveStop(float acceleration, float steering, float braking)
    {
        //Wheel variable declaration
        Quaternion quaternion;
        Vector3 position;

        //Clamed values to -1,1 range
        acceleration = Mathf.Clamp(acceleration, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f) * _maxSteeringAngle;
        braking = Mathf.Clamp(braking, 0f, 1f) * _maxBrakingTorque;

        //float thrustTorque = acceleration * torque;

        if (acceleration > 0.1)
        {
            if (speed < 2)
            {
                if (torqueCalc < maxTorque)
                {
                    torqueCalc += torqueChange;
                }
                torqueNow = acceleration * torqueCalc;
            }
        }
        else if (acceleration < -0.1)
        {
            if (speed < 2)
            {
                if (torqueCalc < maxTorque)
                {
                    torqueCalc += torqueChange;
                }
                torqueNow = acceleration * torqueCalc;
                //Debug.Log("Check "+speed);
            }
        }
        else if (acceleration < 0.1 || acceleration > -0.1)
        {
            torqueNow = 0f;
            torqueCalc = 0f;
        }

        Quaternion newRotation = Quaternion.AngleAxis(90, Vector3.up);

        foreach (var wheel in _wheelColliders)
        {
            //wheel.motorTorque = thrustTorque;

            //Gets world position & rotation of the wheels and sets it in mesh
            wheel.GetWorldPose(out position, out quaternion);
            wheel.transform.GetChild(0).transform.position = position;
            wheel.transform.GetChild(0).transform.rotation = quaternion;
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,0.0005f);
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,Time.deltaTime*0.0000005f);
        }
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            //if (speed < maxSpeed)
            //{
            //    _wheelColliders[i].motorTorque = thrustTorque;
            //}
            //else
            //{
            //    _wheelColliders[i].motorTorque = 0;
            //}
            if (speed >= 0)
            {
                _wheelColliders[i].motorTorque = 0;
                _wheelColliders[i].brakeTorque = 1f * _maxBrakingTorque;
                //_wheelColliders[i].motorTorque = thrustTorque;
                Debug.Log("Slowing Down");
            }
            //else if ((torqueNow > 0.1 || torqueNow < -0.1) && braking <= 0)
            //{
            //    _wheelColliders[i].motorTorque = torqueNow;
            //    _wheelColliders[i].brakeTorque = 0f;
            //    Debug.Log("Under Min Speed & Not Braking");
            //}

            if (i < 2)
            {
                _wheelColliders[i].steerAngle = steering;
            }
            else
            {
                _wheelColliders[i].brakeTorque = braking;
                if (braking > 0)
                {
                    Debug.Log("Braking");
                    _wheelColliders[i].motorTorque = 0;
                }
            }
        }
    }
}
