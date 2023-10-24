using System.Collections;
using UnityEngine;

public class RoadMaintenanceBeacon : MonoBehaviour
{
    //Editor variables
    //[SerializeField] private float warningRadius = 50.0f;
    [SerializeField] private GameObject beaconObj;
    //[SerializeField] private GameObject carObj;

    //Global variables
    private SphereCollider beaconCollider;
    //private ArduinoCarSimulator carSimulator;
    private InputController inputController;
    //private CollisionDetection collisionDetection;
    private InputController.FaceDir _currentFaceDir;
    private InputController.FaceDir _currentRoadDir;
    private float _distanceBetweenObjs;
    private bool receivedCarInfo;
    private GameObject _current;
    //CollisionDetection[] _allColliders;
    private bool updateDist;
    private float _beaconDistanceLeft;
    private bool _enteredCol;
    private bool _pastBeacon;
    public void ColliderTriggered(Collider other, GameObject current, bool enteredCol)
    {
        _current = current;
        _enteredCol = enteredCol;

        if (enteredCol)
        {
            inputController.ReceiveApiObjRequest(InputController.apiEvents.SENDINFO, current);
            updateDist = true;
        }
        else
        {
            ///inputController.ReceiveApiRequest(InputController.apiEvents.GO);
            updateDist = false;
            SendApiRequest();
        }
        //Debug.Log(current.tag);
    }
    public void ReceiveCarInfo(float distanceBetweenObjs, InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir, bool pastBeacon)
    {
        if (pastBeacon)
        {
            _distanceBetweenObjs = distanceBetweenObjs;
        }
        else
        {
            _beaconDistanceLeft = distanceBetweenObjs;
        }
        _pastBeacon = pastBeacon;
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;

        receivedCarInfo = true;
    }
    public void UpdateCarInfo(float carDistance, float distanceBetweenObjs, bool pastBeacon)
    {
        _distanceBetweenObjs = carDistance;
        _beaconDistanceLeft = distanceBetweenObjs;
        _pastBeacon = pastBeacon;
    }
    public string UpdateUI()
    {
        string result = "NA";

        if (_distanceBetweenObjs >= 0)
        {
            result = _distanceBetweenObjs.ToString("f0");
        }
        else if (_distanceBetweenObjs < 0)
        {
            result = _beaconDistanceLeft.ToString("f0");
        }

        return result;
    }
    private void Start()
    {
        inputController = FindObjectOfType<InputController>();




        //beaconCollider = beaconObj.GetComponent<SphereCollider>();
        //if (beaconCollider == null)
        //{
        //    Debug.LogError("No sphere collider added to the game object");
        //}

        //Get the beacon sphere collider and Arduino script
        //carSimulator = gameObject.GetComponent<ArduinoCarSimulator>();
        //inputController = carObj.GetComponent<InputController>();

        //Debug.Log(inputController);
        //collisionDetection = FindObjectOfType<CollisionDetection>();

        //if (collisionDetection == null)
        //{
        //    Debug.LogError("No collisionDetection script in the scene");
        //}
        //Check if objects exist in the scene

        //_allColliders = collisionDetection.RetreiveAllColliders();
    }
    private void LateUpdate()
    {
        if (receivedCarInfo)
        {
            SendApiRequest();
            //CarBeaconLogic();
            receivedCarInfo = false;
        }
        if (updateDist)
        {
            inputController.ReceiveApiObjRequest(InputController.apiEvents.UPDATEDIR, _current);
        }
    }
    //private void CarBeaconLogic()
    //{
    //    if (_currentRoadDir == InputController.FaceDir.West)
    //    {
    //        if (_currentFaceDir == InputController.FaceDir.West)
    //        {
    //            _current.GetComponent<Collider>().enabled = true;

    //            foreach (CollisionDetection collider in _allColliders)
    //            {
    //                if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
    //                {
    //                    collider.GetComponent<Collider>().enabled = false;
    //                }
    //            }
    //            receivedCarInfo = false;
    //            SendApiRequest();
    //        }
    //    }
    //    else if (_currentRoadDir == InputController.FaceDir.West)
    //    {
    //        if (_currentFaceDir == InputController.FaceDir.East)
    //        {
    //            _current.GetComponent<Collider>().enabled = true;

    //            foreach (CollisionDetection collider in _allColliders)
    //            {
    //                if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
    //                {
    //                    collider.GetComponent<Collider>().enabled = false;
    //                }
    //            }
    //            receivedCarInfo = false;
    //            SendApiRequest();
    //        }
    //    }
    //}
    private void SendApiRequest()
    {
        if (_enteredCol)
        {
            inputController.ReceiveApiObjRequest(InputController.apiEvents.WARNING, _current);
            Debug.LogError("Collider Entered");
        }
        else
        {
            inputController.ReceiveApiRequest(InputController.apiEvents.GO);
            Debug.LogError("Collider Existed");
        }
    }
}