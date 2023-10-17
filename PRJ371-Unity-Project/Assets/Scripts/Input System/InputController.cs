using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
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

    //Arduino car variables
    [SerializeField] private GameObject car;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxTorque;
    [SerializeField] private float torqueChange;
    private apiEvents apiRequest;
    private float speed;
    private int speedCalc;
    private float torqueCalc;
    private float torqueNow;

    //Arduino events enum
    public enum apiEvents
    {
        GO,
        SLOWDOWN,
        WARNING,
        STOP
    }
    //Arduino event setting
    public void ReceiveApiRequest(apiEvents request)
    {
        apiRequest = request;
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

        ReceiveApiRequest(apiEvents.GO);
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
                MoveAccelerate(acceleration, steering, braking);
                // Incrementally slow down the car
                //speed = Mathf.Max(speed - speedChange * Time.deltaTime, 5.0f);
                break;
            case apiEvents.STOP:
                MoveStop(acceleration, steering, braking);
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
                _wheelColliders[i].brakeTorque = 0.3f * _maxBrakingTorque;
                //_wheelColliders[i].motorTorque = thrustTorque;
                Debug.Log("Slowing Down & Switch over to Stop");
                ReceiveApiRequest(apiEvents.STOP);
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
                _wheelColliders[i].brakeTorque = 0.3f * _maxBrakingTorque;
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
