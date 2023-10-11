using System;
using System.Collections;
using System.Collections.Generic;
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

        Move(acceleration, steering, braking);
        SkidCheck();
        //if (Keyboard.current.spaceKey.wasPressedThisFrame)
        //{
        //    Debug.Log("Spacebar key was pressed");
        //}
    }
    private void Move(float acceleration, float steering, float braking)
    {
        //Wheel variable declaration
        Quaternion quaternion;
        Vector3 position;

        //Clamed values to -1,1 range
        acceleration = Mathf.Clamp(acceleration, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f) * _maxSteeringAngle;
        braking = Mathf.Clamp(braking, 0f, 1f) * _maxBrakingTorque;

        float thrustTorque = acceleration * torque;
        Quaternion newRotation = Quaternion.AngleAxis(90, Vector3.up);

        foreach (var wheel in _wheelColliders)
        {
            wheel.motorTorque = thrustTorque;

            //Gets world position & rotation of the wheels and sets it in mesh
            wheel.GetWorldPose(out position, out quaternion);
            wheel.transform.GetChild(0).transform.position = position;
            wheel.transform.GetChild(0).transform.rotation = quaternion;
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,0.0005f);
            //wheel.transform.GetChild(0).transform.rotation = Quaternion.Slerp(quaternion,newRotation,Time.deltaTime*0.0000005f);
        }
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].motorTorque = thrustTorque;

            if (i < 2)
            {
                _wheelColliders[i].steerAngle = steering;
            }
            else
            {
                _wheelColliders[i].brakeTorque = braking;
            }
        }
    }
}
