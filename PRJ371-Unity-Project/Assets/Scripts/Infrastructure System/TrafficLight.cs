using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TrafficLights
{
    //Creates custom parameters for a List var
    public currentColor currentSignal;
    public GameObject parent;
    public GameObject red;
    public GameObject green;
    public GameObject yellow;
}
public enum currentColor
{
    Red, Green, Yellow
}
public class TrafficLight : MonoBehaviour
{
    //Editor variables
    [SerializeField] private List<TrafficLights> trafficLights;

    //Global variables
    private float _timer = 0f;
    private int _colorIndex = 0;
    private InputController inputController;
    private CollisionDetection collisionDetection;
    //private string _currentRoadDir;
    private InputController.FaceDir _currentFaceDir;
    private InputController.FaceDir _currentRoadDir;
    private float _timeBetweenObjs;
    private bool receivedCarInfo;
    private GameObject _current;
    CollisionDetection[] _allColliders;
    public bool roadDirChanged { get; set; }
    //public string currentRoadDir { get {return _currentRoadDir;} set { _currentRoadDir = value;}}
    public void ColliderTriggered(Collider other, GameObject current)
    {
        Debug.Log(current.tag);
        //_currentFaceDir = inputController.currentFaceDir;
        _current = current;
        inputController.ReceiveApiObjRequest(InputController.apiEvents.SENDINFO, current);
    }
    public void ReceiveCarInfo(float timeBetweenObjs, InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    {
        _timeBetweenObjs = timeBetweenObjs;
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;

        receivedCarInfo = true;
    }
    public void UpdateCarInfo(InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    {
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;
    }
    void Start()
    {
        collisionDetection = FindObjectOfType<CollisionDetection>();
        if (collisionDetection == null)
        {
            Debug.LogError("No collisionDetection script in the scene");
        }

        inputController = FindObjectOfType<InputController>();
        if (inputController == null)
        {
            Debug.LogError("No inputController script in the scene");
        }
        //Initialize all the traffic lights

        _allColliders = collisionDetection.RetreiveAllColliders();
        UpdateLights(_colorIndex);
    }

    void Update()
    {
        //Timer used for changing the traffic lights
        _timer += Time.deltaTime;

        if ((_colorIndex == 1 || _colorIndex == 3) && (_timer >= 3))
        {
            TimerLogic();
        }
        else if (_timer >= 10f)
        {
            TimerLogic();
        }
    }
    private void LateUpdate()
    {
        if (receivedCarInfo)
        {
            CarTrafficLLogic();
        }

        if (roadDirChanged)
        {
            Debug.LogError("Trigerreddddddddddddddddddddddd");
            Debug.LogError(roadDirChanged);
            inputController.ReceiveApiRequest(InputController.apiEvents.UPDATEDIR);
            foreach (TrafficLights light in trafficLights)
            {
                if (_currentRoadDir == InputController.FaceDir.South)
                {
                    
                    Debug.LogError("11111111111111");
                    if (_currentFaceDir == InputController.FaceDir.North)
                    {
                        Debug.LogError("22222222222222");
                        if (light.parent.tag == "North TrafficL")
                        {
                            Debug.LogError("33333333333333");
                            light.parent.GetComponent<Collider>().enabled = true;
                            roadDirChanged = false;
                        }
                    }
                }
                else if (_currentRoadDir == InputController.FaceDir.East)
                {
                    if (_currentFaceDir == InputController.FaceDir.West)
                    {
                        if (light.parent.tag == "West TrafficL")
                        {
                            light.parent.GetComponent<Collider>().enabled = true;
                            roadDirChanged = false;
                        }
                    }
                }
                else if (_currentRoadDir == InputController.FaceDir.West)
                {
                    if (_currentFaceDir == InputController.FaceDir.East)
                    {
                        if (light.parent.tag == "East TrafficL")
                        {
                            light.parent.GetComponent<Collider>().enabled = true;
                            roadDirChanged = false;
                        }
                    }
                }
            }
        }
    }
    private void CarTrafficLLogic()
    {
        Debug.LogError("Triggered CarTrafficLLogic");
        Debug.LogError(_currentRoadDir);
        if (_currentRoadDir == InputController.FaceDir.South)
        {
            Debug.Log("Triggered _currentRoadDir");
            if (_currentFaceDir == InputController.FaceDir.North)
            {
                Debug.Log("Triggered _currentFaceDir");
                _current.GetComponent<Collider>().enabled = true;

                foreach (CollisionDetection collider in _allColliders)
                {
                    if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
                    {
                        Debug.Log("Triggered collider.tag");
                        collider.GetComponent<Collider>().enabled = false;
                    }
                }
                receivedCarInfo = false;
                SendApiRequest();
            }
        }
        else if (_currentRoadDir == InputController.FaceDir.East)
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
        else
        {
            //North do nothing no traffic light
        }
    }
    private void SendApiRequest()
    {
        Debug.Log("Triggered SendApiRequest");
        foreach (TrafficLights light in trafficLights)
        {
            Debug.LogError("Run For Loop");
            if (_current.tag == light.parent.tag)
            {
                Debug.LogError(_current.tag);
                Debug.LogError(light.parent.tag);
                if (light.currentSignal == currentColor.Green && _timeBetweenObjs < (13 - _timer))
                {
                    inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                }
                else if (light.currentSignal == currentColor.Yellow && _timeBetweenObjs < (3 - _timer))
                {
                    inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                }
                else if (light.currentSignal == currentColor.Red && _timeBetweenObjs > (10 - _timer))
                {
                    inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                }
                else
                {
                    inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                }
            }
        }
    }
    private void TimerLogic()
    {
        //Timer logic and method calling
        _timer = 0f;

        if (_colorIndex >= 3)
        {
            _colorIndex = 0;
        }
        else
        {
            _colorIndex++;
        }

        UpdateLights(_colorIndex);
    }
    private void UpdateLights(int color)
    {
        //Logic for changing the traffic lights when North is Green / Yellow
        //East & West whould be Red and vise versa
        foreach (TrafficLights light in trafficLights)
        {
            switch (color)
            {
                case 0:
                    if (light.parent.tag == "North")
                    {
                        SetLightGreen(light);
                    }
                    else
                    {
                        SetLightRed(light);
                    }
                    break;
                case 1:
                    if (light.parent.tag == "North")
                    {
                        SetLightYellow(light);
                    }
                    else
                    {
                        SetLightRed(light);
                    }
                    break;
                case 2:
                    if (light.parent.tag == "North")
                    {
                        SetLightRed(light);
                    }
                    else
                    {
                        SetLightGreen(light);
                    }
                    break;
                case 3:
                    if (light.parent.tag == "North")
                    {
                        SetLightRed(light);
                    }
                    else
                    {
                        SetLightYellow(light);
                    }
                    break;
                default:
                    SetLightRed(light);
                    break;
            }
            if (light.currentSignal == currentColor.Green && inputController.apiRequest == InputController.apiEvents.STOP && !receivedCarInfo)
            {
                inputController.ReceiveApiRequest(InputController.apiEvents.GO);
            }
        }
    }
    private void SetLightRed(TrafficLights light)
    {
        //Used to set traffic light Red
        light.currentSignal = currentColor.Red;
        light.red.SetActive(true);
        light.green.SetActive(false);
        light.yellow.SetActive(false);
    }
    private void SetLightGreen(TrafficLights light)
    {
        //Used to set traffic light Green
        light.currentSignal = currentColor.Green;
        light.green.SetActive(true);
        light.red.SetActive(false);
        light.yellow.SetActive(false);
    }
    private void SetLightYellow(TrafficLights light)
    {
        //Used to set traffic light Yellow
        light.currentSignal = currentColor.Yellow;
        light.yellow.SetActive(true);
        light.red.SetActive(false);
        light.green.SetActive(false);
    }
    public string GetApiRequest()
    {
        // Signal can be set based on your actual implementation
        // For demonstration, it's set to "Red", "Orange", or "Green"
        //switch (currentSignal)
        //{
        //    case "Green":
        //        return "Go";
        //    case "Yellow":
        //        return "SlowDown";
        //    case "Red":
        //        return "Stop";
        //    default:
        //        return "";
        //}
        return "None";
    }
}