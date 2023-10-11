using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //Editor variables
    [Header("Configurable Properties")]
    public float LookOffset;
    public float CameraAngle;
    public float DefaultZoom;
    public float ZoomMax;
    public float ZoomMin;
    public float CameraSpeed;
    public float RotationSpeed;

    //Global variables
    private Camera _actualCamera;
    private Vector3 _cameraPositionTarget;
    //private const float InternalMoveTargetSpeed = 8;
    private const float InternalMoveSpeed = 4;
    private Vector3 _moveTarget;
    private Vector3 _moveDirection;
    private float _currentZoomAmount;
    private float _internalZoomSpeed = 4;
    private bool _rightMouseDown = false;
    private const float InternalRotationSpeed = 4;
    private Quaternion _rotationTarget;
    private Vector2 _mouseDelta;
    //private float _eventCounter;
    public float CurrentZoom
    {
        get => _currentZoomAmount;
        private set
        {
            _currentZoomAmount = value;
            UpdateCameraTarget();
        }
    }
    void Start()
    {
        _actualCamera = GetComponentInChildren<Camera>();

        _actualCamera.transform.rotation = Quaternion.AngleAxis(CameraAngle, Vector3.right);

        CurrentZoom = DefaultZoom;
        _actualCamera.transform.position = _cameraPositionTarget;

        _rotationTarget = transform.rotation;
        //_actualCamera = GetComponentInChildren<Camera>();

        ////Set editor property equal to the cameras
        //_actualCamera.transform.rotation = Quaternion.AngleAxis(CameraAngle, Vector3.right);

        //_cameraPositionTarget = (Vector3.up * LookOffset) + (Quaternion.AngleAxis(CameraAngle,
        //    Vector3.right) * Vector3.back) * DefaultZoom;
        //_actualCamera.transform.position = _cameraPositionTarget;
    }
    private void LateUpdate()
    {
        //Debug.Log(_eventCounter);
        //_eventCounter = 0;
        //Lerp  the camera to a new move target position
        transform.position = Vector3.Lerp(transform.position, _moveTarget, Time.deltaTime * InternalMoveSpeed);
        //Move the _actualCamera's local position based on the new zoom factor
        _actualCamera.transform.localPosition = Vector3.Lerp(_actualCamera.transform.localPosition,
            _cameraPositionTarget, Time.deltaTime * _internalZoomSpeed);

        //Set the target rotation based on the mouse delta position and our rotation speed
        _rotationTarget *= Quaternion.AngleAxis(_mouseDelta.x * Time.deltaTime * RotationSpeed, Vector3.up);

        //Slerp the camera rig's rotation based on the new target
        transform.rotation = Quaternion.Slerp(transform.rotation, _rotationTarget, Time.deltaTime * InternalRotationSpeed);
    }
    private void FixedUpdate()
    {
        _moveTarget += (transform.forward * _moveDirection.z + transform.right *
            _moveDirection.x) * Time.fixedDeltaTime * CameraSpeed;
    }
    private void UpdateCameraTarget()
    {
        _cameraPositionTarget = (Vector3.up * LookOffset) +
            (Quaternion.AngleAxis(CameraAngle, Vector3.right) * Vector3.back) * _currentZoomAmount;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        //Read the input values and setting values accordingly
        Vector2 value = context.ReadValue<Vector2>();

        _moveDirection = new Vector3(value.x, 0, value.y);
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        _mouseDelta = _rightMouseDown ? context.ReadValue<Vector2>() : Vector2.zero;
        //_eventCounter += _rightMouseDown ? 1 : 0;
    }
    public void OnRotateToggle(InputAction.CallbackContext context)
    {
        _rightMouseDown = context.ReadValue<float>() == 1;
    }
    public void OnZoom(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed)
        {
            return;
        }
        // Adjust the current zoom value based on the direction of the scroll - this is clamped to our zoom min/max. 
        CurrentZoom = Mathf.Clamp(_currentZoomAmount - context.ReadValue<Vector2>().y, ZoomMax, ZoomMin);
    }
    public void onLockToggle(InputAction.CallbackContext context)
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        //if (context.phase != InputActionPhase.Performed)
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //}
        //else
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //}
    }
}
