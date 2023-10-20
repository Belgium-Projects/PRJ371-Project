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
    private CollisionDetection collisionDetection;
    private InputController.FaceDir _currentFaceDir;
    private InputController.FaceDir _currentRoadDir;
    private float _timeBetweenObjs;
    private bool receivedCarInfo;
    private GameObject _current;
    CollisionDetection[] _allColliders;
    public void ColliderTriggered(Collider other, GameObject current)
    {
        _current = current;
        inputController.ReceiveApiObjRequest(InputController.apiEvents.WARNING, current);
        Debug.Log(current.tag);
    }
    public void ReceiveCarInfo(float timeBetweenObjs, InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    {
        _timeBetweenObjs = timeBetweenObjs;
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;

        receivedCarInfo = true;
    }
    private void Start()
    {
        //Get the beacon sphere collider and Arduino script
        beaconCollider = beaconObj.GetComponent<SphereCollider>();
        //carSimulator = gameObject.GetComponent<ArduinoCarSimulator>();
        //inputController = carObj.GetComponent<InputController>();
        inputController = FindObjectOfType<InputController>();
        //Debug.Log(inputController);
        collisionDetection = FindObjectOfType<CollisionDetection>();

        if (collisionDetection == null)
        {
            Debug.LogError("No collisionDetection script in the scene");
        }
        //Check if objects exist in the scene
        if (beaconCollider == null )
        {
            Debug.LogError("No sphere collider added to the game object");
        }
        if (inputController == null)
        {
            Debug.LogError("No inputController script in the scene");
        }

        _allColliders = collisionDetection.RetreiveAllColliders();
    }
    private void LateUpdate()
    {
        if (receivedCarInfo)
        {
            CarBeaconLogic();
        }
    }
    private void CarBeaconLogic()
    {
        if (_currentRoadDir == InputController.FaceDir.West)
        {
            if (_currentFaceDir == InputController.FaceDir.West)
            {
                _current.GetComponent<Collider>().enabled = true;

                foreach (CollisionDetection collider in _allColliders)
                {
                    if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
                    {
                        collider.GetComponent<Collider>().enabled = false;
                    }
                }
                receivedCarInfo = false;
                SendApiRequest();
            }
        }
        else if (_currentRoadDir == InputController.FaceDir.West)
        {
            if (_currentFaceDir == InputController.FaceDir.East)
            {
                _current.GetComponent<Collider>().enabled = true;

                foreach (CollisionDetection collider in _allColliders)
                {
                    if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
                    {
                        collider.GetComponent<Collider>().enabled = false;
                    }
                }
                receivedCarInfo = false;
                SendApiRequest();
            }
        }
    }
    private void SendApiRequest()
    {

    }
}