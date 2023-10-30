using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //Editor variables
    [Header("Fixed-Cam Properties")]
    [SerializeField] private Transform targetCar;
    [SerializeField] private float distance;
    [SerializeField] private float height;
    [SerializeField] private float damping;
    [SerializeField] private float rotationDamping;
    [SerializeField] private bool smoothRotation;
    [SerializeField] private bool followBehind;

    [Header("Free-Cam Properties")]
    [SerializeField] private float LookOffset;
    [SerializeField] private float CameraAngle;
    [SerializeField] private float DefaultZoom;
    [SerializeField] private float ZoomMax;
    [SerializeField] private float ZoomMin;
    [SerializeField] private float CameraSpeed;
    [SerializeField] private float RotationSpeed;

    //Global variables
    private InputController inputController;
    private Camera _actualCamera;
    private Vector3 _cameraPositionTarget;
    private Vector3 _moveTarget;
    private Vector3 _moveDirection;
    private Vector3 wantedPosition;
    private Vector2 _mouseDelta;
    private Quaternion _rotationTarget;
    private const float InternalMoveSpeed = 4;
    private const float InternalRotationSpeed = 4;
    private bool _rightMouseDown;
    private bool toggleFreeCam;
    private float _currentZoomAmount;
    private float _internalZoomSpeed;

    //Get|Set variables
    public float CurrentZoom
    {
        get => _currentZoomAmount;
        private set
        {
            _currentZoomAmount = value;
            UpdateCameraTarget();
        }
    }
    private void Awake()
    {
        //Sets toggle value
        toggleFreeCam = false;
    }
    void Start()
    {
        //Gets the Input Controller script
        inputController = FindObjectOfType<InputController>();
        //Gets the camera component in the parent
        _actualCamera = GetComponentInChildren<Camera>();

        //Sets a few camera properties
        _actualCamera.transform.rotation = Quaternion.AngleAxis(CameraAngle, Vector3.right);

        CurrentZoom = DefaultZoom;
        _actualCamera.transform.position = _cameraPositionTarget;

        _rotationTarget = transform.rotation;
    }
    private void LateUpdate()
    {
        if (toggleFreeCam)
        {
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
        else
        {
            //Flips camera between infront or back of car
            if (followBehind && inputController.CurrentAcceleration < 0)
            {
                wantedPosition = targetCar.TransformPoint(0, height, distance);
            }
            else if (followBehind && inputController.CurrentAcceleration >= 0)
            {
                wantedPosition = targetCar.TransformPoint(0, height, -distance);
            }
            else
            {
                wantedPosition = targetCar.TransformPoint(0, height, distance);
            }

            //Lerp the camera rig's rotation based on the new target
            transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

            //Smooths the camera rotation & prosition
            if (smoothRotation)
            {
                Quaternion wantedRotation = Quaternion.LookRotation(targetCar.position - transform.position, targetCar.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
            }
            else transform.LookAt(targetCar, targetCar.up);
        }
    }
    private void FixedUpdate()
    {
        //Calculating free camera movements
        _moveTarget += (transform.forward * _moveDirection.z + transform.right *
            _moveDirection.x) * Time.fixedDeltaTime * CameraSpeed;
    }
    private void UpdateCameraTarget()
    {
        //Called to update free camera target
        _cameraPositionTarget = (Vector3.up * LookOffset) +
            (Quaternion.AngleAxis(CameraAngle, Vector3.right) * Vector3.back) * _currentZoomAmount;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        //Read the input values and setting values accordingly in free camera
        Vector2 value = context.ReadValue<Vector2>();

        _moveDirection = new Vector3(value.x, 0, value.y);
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        //Rotate free camera
        _mouseDelta = _rightMouseDown ? context.ReadValue<Vector2>() : Vector2.zero;
    }
    public void OnRotateToggle(InputAction.CallbackContext context)
    {
        //Check if rotate key is hold to zoom in free camera
        _rightMouseDown = context.ReadValue<float>() == 1;
    }
    public void OnZoom(InputAction.CallbackContext context)
    {
        //Checks is zoom key changed and update free camera zoom
        if (context.phase != InputActionPhase.Performed)
        {
            return;
        }
        //Adjust the current zoom value based on the direction of the scroll
        CurrentZoom = Mathf.Clamp(_currentZoomAmount - context.ReadValue<Vector2>().y, ZoomMax, ZoomMin);
    }
    public void OnLockToggle(InputAction.CallbackContext context)
    {
        //Toggles between cursor locked in screen
        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void OnCamToggle(InputAction.CallbackContext context)
    {
        //Toggle between the two types of cameras
        toggleFreeCam = toggleFreeCam ? false : true;
    }
}
