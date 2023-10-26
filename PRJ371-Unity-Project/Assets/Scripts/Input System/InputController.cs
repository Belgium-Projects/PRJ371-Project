using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

public class InputController : MonoBehaviour
{
    //Editor Variables
    [SerializeField] private WheelCollider[] _wheelColliders;
    [SerializeField] private float torque;
    [SerializeField] private float _maxSteeringAngle;
    [SerializeField] private float _maxBrakingTorque;
    [SerializeField] private AudioClip _skidSoundEffect;
    [SerializeField] private float _skidThreshold;
    [SerializeField] private GameObject car;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxTorque;
    [SerializeField] private float torqueChange;

    //Global variables
    private PlayerInput playerInput;
    private AudioSource audioSource;
    private TrafficLight trafficLight;
    private RoadMaintenanceBeacon roadMaintenanceBeacon;
    private StopSign stopSign;
    private Transform carTrans;
    private GameObject _infrastructureObj;
    private CollisionDetection[] _allColliders;
    private CollisionDetection collisionDetection;
    private WheelCollider wheelColPlaceH;
    private Quaternion _wheelQuaternion;
    private Vector3 _wheelPosition;
    private Vector2 moveInput;
    private FaceDir _currentFaceDir;
    private FaceDir _currentRoadDir;
    private apiEvents _apiRequest;
    private bool col1Triggered;
    private bool _warning;
    private float _speed;
    private float _infrastructurDis;
    private float braking;
    private float carDistance;
    private float timeBetweenObjs;
    private float distanceBeforeM;
    private float acceleration;
    private float steering;
    private float thrustTorque;
    private int speedCalc;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    private Tuple<WheelCollider, WheelCollider> frontWheelColDic;
    private Tuple<WheelCollider, WheelCollider> backWheelColDic;
    private Dictionary<string, Tuple<WheelCollider, WheelCollider>> wheelColDic;
    public apiEvents ApiRequest { get { return _apiRequest; } set { _apiRequest = value; } }
    public bool Warning { get { return _warning; } set { _warning = value; } } //Set
    public float Speed { get { return _speed; } set { _speed = value; } }
    public float InfrastructureDis { get { return _infrastructurDis; } set { _infrastructurDis = value; } } //Set
    public Dictionary<string, Tuple<bool, bool>> dualColDic { get {return _dualColDic;} set {_dualColDic = value;} }
    public FaceDir currentFaceDir { get { return _currentFaceDir; } set { _currentFaceDir = value; } }
    public FaceDir currentRoadDir { get { return _currentRoadDir; } set { _currentRoadDir = value; } }
    public enum apiEvents
    {
        //Arduino events enum
        GO,
        SLOWDOWN,
        WARNING,
        STOP,
        SENDINFO
    }
    public enum FaceDir
    {
        //Car & Road facing direction enum
        North,
        South,
        East,
        West,
        Bend
    }
    public void ReceiveApiRequest(apiEvents request)
    {
        //Basic Arduino event request
        ApiRequest = request;
        Debug.LogError($"11111 ~ Received {request} Event");
    }
    public void ReceiveApiObjRequest(apiEvents request, GameObject infrastructureObj)
    {
        //Advanced Arduino event request
        ApiRequest = request;
        Debug.LogError($"22222 ~ Received {request} Event");
        _infrastructureObj = infrastructureObj;
    }
    private void PopulateInfColDic()
    {
        //Setting dual collider infrstructure objects
        _allColliders = collisionDetection.RetreiveAllColliders();
        dualColDic = new Dictionary<string, Tuple<bool, bool>>();
        wheelColDic = new Dictionary<string, Tuple<WheelCollider, WheelCollider>>();

        foreach (CollisionDetection collision in _allColliders)
        {
            if (!collision.tag.Contains("Road"))
            {
                if (!dualColDic.ContainsKey(collision.tag))
                {
                    dualColDic.Add(collision.tag, Tuple.Create(false, false));
                }
            }
        }

        //Setting wheel collider objects
        foreach (WheelCollider wheelCol in _wheelColliders)
        {
            if (!wheelCol.tag.Contains("Road"))
            {
                if (!wheelColDic.ContainsKey(wheelCol.tag))
                {
                    wheelColDic.Add(wheelCol.tag, Tuple.Create(wheelCol, wheelCol));
                    wheelColPlaceH = wheelCol;
                }
                else
                {
                    wheelColDic[wheelCol.tag] = Tuple.Create(wheelColPlaceH, wheelCol);
                }
            }
        }
    }
    public void CarDirectionCalc()
    {
        //Calculate which direction the car is facing
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
    }
    public void ColliderTriggered(bool resetCol)
    {
        //Car slows down in a bend in the road
        if (resetCol)
        {
            ReceiveApiRequest(apiEvents.GO);
        }
        else
        {
            ReceiveApiRequest(apiEvents.SLOWDOWN);
        }
    }
    private void Start()
    {
        //Initializing variables
        col1Triggered = false;
        carTrans = car.GetComponent<Transform>();

        ReceiveApiRequest(apiEvents.GO);
    }
    private void Awake()
    {
        //Getting other scripts in the scene
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
        trafficLight = FindObjectOfType<TrafficLight>();
        roadMaintenanceBeacon = FindObjectOfType<RoadMaintenanceBeacon>();
        collisionDetection = FindObjectOfType<CollisionDetection>();
        stopSign = FindObjectOfType<StopSign>();

        //Checking is scripts are not in the scene & logging error
        if (!playerInput) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(playerInput));
        if (!audioSource) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(audioSource));
        if (!trafficLight) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(trafficLight));
        if (!roadMaintenanceBeacon) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(roadMaintenanceBeacon));
        if (!collisionDetection) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(collisionDetection));
        if (!stopSign) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(stopSign));

        //Populating dictionaries
        PopulateInfColDic();
    }
    public void Braking(InputAction.CallbackContext context)
    {
        //Sets braking variable is input event is triggered
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
        //Logic for adding Skid particals and playing audio
        int skidCount = 0;

        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            WheelHit wheelHit;
            _wheelColliders[i].GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.forwardSlip) >= _skidThreshold ||
                Mathf.Abs(wheelHit.sidewaysSlip) >= _skidThreshold)
            {
                skidCount++;

                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(_skidSoundEffect);
                }
            }
        }

        //Checks if wheels is not skidding to turn the sound off
        if (skidCount == 0 & audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    void Update()
    {
        //Gets input value and setting the different axis
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        acceleration = moveInput.y;
        steering = moveInput.x;

        //Limiting variable values to a range
        acceleration = Mathf.Clamp(acceleration, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f) * _maxSteeringAngle;
        braking = Mathf.Clamp(braking, 0f, 1f) * _maxBrakingTorque;

        SkidCheck();

        //Update UI elements
        switch (ApiRequest)
        {
            case apiEvents.GO:
                //Call Update UI
                break;
            case apiEvents.SLOWDOWN:
                //Call Update UI
                break;
            case apiEvents.WARNING:
                //Call Update UI
                break;
            case apiEvents.STOP:
                //Call Update UI
                break;
            case apiEvents.SENDINFO:
                TimeDistanceCalc(_infrastructureObj);
                //Call Update UI
                break;
            default:
                break;
        }
    }
    private void LateUpdate()
    {
        //Capping speed value at max
        Speed = (speedCalc <= maxSpeed) ? speedCalc : maxSpeed;
    }
    private void FixedUpdate()
    {
        //Getting car speed value
        speedCalc = int.Parse(MathF.Floor((float)(car.GetComponent<Rigidbody>().velocity.magnitude * 3.6)).ToString("f0"));

        //Simulating gear change from drive ~ reverse
        if (acceleration > 0f)
        {
            if (Speed < 2)
            {
                thrustTorque = acceleration * torque;
            }
        }
        else if (acceleration < 0f)
        {
            if (Speed < 2)
            {
                thrustTorque = acceleration * torque;
            }
        }
        else if (acceleration == 0f)
        {
            thrustTorque = 0f;
        }

        CarMovement();
    }
    private void CarMovement()
    {
        SetWheelParms();

        //Car movement behaviour steering ~ front wheels & braking ~ back wheels
        if (braking > 0)
        {
            if (wheelColDic.TryGetValue("Front Wheels", out frontWheelColDic))
            {
                frontWheelColDic.Item1.motorTorque = 0f;
                frontWheelColDic.Item2.motorTorque = 0f;
            }
            if (wheelColDic.TryGetValue("Back Wheels", out backWheelColDic))
            {
                backWheelColDic.Item1.brakeTorque = braking;
                backWheelColDic.Item2.brakeTorque = braking;
            }
        }
        else
        {
            if (wheelColDic.TryGetValue("Front Wheels", out frontWheelColDic))
            {
                if (ApiRequest.Equals(apiEvents.GO))
                {
                    if (Speed <= maxSpeed)
                    {
                        frontWheelColDic.Item1.motorTorque = thrustTorque;
                        frontWheelColDic.Item2.motorTorque = thrustTorque;
                    }
                    else
                    {
                        frontWheelColDic.Item1.motorTorque = 0f;
                        frontWheelColDic.Item2.motorTorque = 0f;
                    }
                }
                else if (ApiRequest.Equals(apiEvents.SLOWDOWN))
                {
                    if (Speed >= minSpeed)
                    {
                        frontWheelColDic.Item1.motorTorque = 0f;
                        frontWheelColDic.Item2.motorTorque = 0f;
                    }
                    else
                    {
                        frontWheelColDic.Item1.motorTorque = thrustTorque;
                        frontWheelColDic.Item2.motorTorque = thrustTorque;
                    }
                }
                else if (ApiRequest.Equals(apiEvents.STOP))
                {
                    frontWheelColDic.Item1.motorTorque = 0f;
                    frontWheelColDic.Item2.motorTorque = 0f;
                }
            }
            if (wheelColDic.TryGetValue("Back Wheels", out backWheelColDic))
            {
                if (ApiRequest.Equals(apiEvents.GO))
                {
                    if (Speed <= maxSpeed)
                    {
                        backWheelColDic.Item1.brakeTorque = 0f;
                        backWheelColDic.Item2.brakeTorque = 0f;
                    }
                    else
                    {
                        backWheelColDic.Item1.brakeTorque = 0.6f * _maxBrakingTorque;
                        backWheelColDic.Item2.brakeTorque = 0.6f * _maxBrakingTorque;
                    }
                }
                else if (ApiRequest.Equals(apiEvents.SLOWDOWN))
                {
                    if (Speed >= minSpeed)
                    {
                        backWheelColDic.Item1.brakeTorque = 0.6f * _maxBrakingTorque;
                        backWheelColDic.Item2.brakeTorque = 0.6f * _maxBrakingTorque;
                    }
                    else
                    {
                        backWheelColDic.Item1.brakeTorque = 0f;
                        backWheelColDic.Item2.brakeTorque = 0f;
                    }
                }
                else if (ApiRequest.Equals(apiEvents.STOP))
                {
                    backWheelColDic.Item1.brakeTorque = 0.6f * _maxBrakingTorque;
                    backWheelColDic.Item2.brakeTorque = 0.6f * _maxBrakingTorque;
                }
            }
        }

        //Setting steering angle
        if (wheelColDic.TryGetValue("Front Wheels", out frontWheelColDic))
        {
            frontWheelColDic.Item1.steerAngle = steering;
            frontWheelColDic.Item2.steerAngle = steering;
        }
    }
    private void SetWheelParms()
    {
        //Gets world position & rotation of the wheels and sets it in mesh
        foreach (var wheel in _wheelColliders)
        {
            wheel.GetWorldPose(out _wheelPosition, out _wheelQuaternion);
            wheel.transform.GetChild(0).transform.position = _wheelPosition;
            wheel.transform.GetChild(0).transform.rotation = _wheelQuaternion;
        }
    }
    private void TimeDistanceCalc(GameObject infrastructureObj)
    {
        //Calculating distance between car & infrastructure element & time to reach
        carDistance = Vector3.Distance(car.transform.position, infrastructureObj.transform.position);
        timeBetweenObjs = (carDistance / speedCalc) * 3.6f;

        //Calculating distance to road maintenance
        if (infrastructureObj.tag.Contains("Beacon") && !col1Triggered)
        {
            var obj1 = infrastructureObj.GetComponents<SphereCollider>()[0];
            var obj2 = infrastructureObj.GetComponents<SphereCollider>()[1];

            distanceBeforeM = (obj2.radius - obj1.radius) / 3.6f;

            col1Triggered = true;
        }

        CarDirectionCalc();

        //Sends informatin to infrastructure elements
        switch (infrastructureObj.tag)
        {
            case "Beacon":
                roadMaintenanceBeacon.ReceiveCarInfo(distanceBeforeM);
                break;
            case "North StopS":
                stopSign.ReceiveCarInfo(_currentFaceDir, _currentRoadDir);
                break;
            case "North TrafficL":
                trafficLight.ReceiveCarInfo(timeBetweenObjs);
                break;
            case "East TrafficL":
                trafficLight.ReceiveCarInfo(timeBetweenObjs);
                break;
            case "West TrafficL":
                trafficLight.ReceiveCarInfo(timeBetweenObjs);
                break;
            default:
                break;
        }
    }
}