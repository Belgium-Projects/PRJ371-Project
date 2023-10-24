using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

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
    //private CollisionDetection collisionDetection;
    //private string _currentRoadDir;
    private InputController.FaceDir _currentFaceDir;
    //private InputController.FaceDir _currentRoadDir;
    private float _timeBetweenObjs;
    private bool receivedCarInfo;
    private GameObject _current;
    //CollisionDetection[] _allColliders;
    private Dictionary<string, TrafficLights> trafficLDic;
    private TrafficLights selectTrafficL;
    private TrafficLights sendTrafficLReq;
    //private bool updateDir;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    private int calledIndex;
    private bool coIsRunning;
    private Tuple<bool, bool> currentColDic;
    //private Tuple<bool, bool> currentColDic;
    //private bool _sendReq;
    public bool roadDirChanged { get; set; }
    //public string currentRoadDir { get {return _currentRoadDir;} set { _currentRoadDir = value;}}
    public void ColliderTriggered(GameObject current, bool resetCol) //bool sendReq)
    {
        _current = current;

        if (resetCol)
        {
            if (calledIndex == 0)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);

                if (current.tag.Contains("South"))
                {
                    if (_currentFaceDir == InputController.FaceDir.North)
                    {
                        if (trafficLDic.TryGetValue("North TrafficL", out selectTrafficL))
                        {
                            ChangeTrafficLCol(selectTrafficL);
                        }
                    }
                }
                else if (current.tag.Contains("East"))
                {
                    if (_currentFaceDir == InputController.FaceDir.West)
                    {
                        if (trafficLDic.TryGetValue("West TrafficL", out selectTrafficL))
                        {
                            ChangeTrafficLCol(selectTrafficL);
                        }
                    }
                }
                else if (current.tag.Contains("West"))
                {
                    if (_currentFaceDir == InputController.FaceDir.East)
                    {
                        if (trafficLDic.TryGetValue("East TrafficL", out selectTrafficL))
                        {
                            ChangeTrafficLCol(selectTrafficL);
                        }
                    }
                }
            }
        }
        else
        {
            inputController.ReceiveApiObjRequest(InputController.apiEvents.SENDINFO, current);
            calledIndex++;

            if (calledIndex == 1)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(true, false);
            }
            else if (calledIndex == 2)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(true, true);
                calledIndex = 0;
            }
        }




        //Debug.Log(current.tag);
        ////_currentFaceDir = inputController.currentFaceDir;
        //_current = current;
        ////_sendReq = sendReq;
        //if (sendReq)
        //{
        //    inputController.ReceiveApiObjRequest(InputController.apiEvents.SENDINFO, current);
        //    //_sendReq = false;
        //}
        //else
        //{
        //    //updateDir = true;
        //    inputController.ReceiveApiObjRequest(InputController.apiEvents.UPDATEDIR, current);
        //    if (current.tag.Contains("South"))
        //    {
        //        if (_currentFaceDir == InputController.FaceDir.North)
        //        {
        //            if (trafficLDic.TryGetValue("North TrafficL", out selectTrafficL))
        //            {
        //                ChangeTrafficLCol(selectTrafficL);
        //            }
        //        }
        //    }
        //    else if (current.tag.Contains("East"))
        //    {
        //        if (_currentFaceDir == InputController.FaceDir.West)
        //        {
        //            if (trafficLDic.TryGetValue("West TrafficL", out selectTrafficL))
        //            {
        //                ChangeTrafficLCol(selectTrafficL);
        //            }
        //        }
        //    }
        //    else if (current.tag.Contains("West"))
        //    {
        //        if (_currentFaceDir == InputController.FaceDir.East)
        //        {
        //            if (trafficLDic.TryGetValue("East TrafficL", out selectTrafficL))
        //            {
        //                ChangeTrafficLCol(selectTrafficL);
        //            }
        //        }
        //    }
        //}
    }
    private void ChangeTrafficLCol(TrafficLights selectTrafficL)
    {
        selectTrafficL.parent.GetComponents<Collider>()[0].enabled = true;
        selectTrafficL.parent.GetComponents<Collider>()[1].enabled = true;

        foreach (var light in trafficLDic)
        {
            if (!selectTrafficL.parent.CompareTag(light.Key))
            {
                light.Value.parent.GetComponents<Collider>()[0].enabled = false;
                light.Value.parent.GetComponents<Collider>()[1].enabled = false;
            }
        }




        //foreach (var light in trafficLDic)
        //{
        //    if (light.Key == selectTrafficL.parent.tag)
        //    {
        //        light.Value.parent.GetComponent<Collider>().enabled = true;
        //        Debug.LogError($"Enabled = {light.Key}");
        //    }
        //    else
        //    {
        //        light.Value.parent.GetComponent<Collider>().enabled = false;
        //    }
        //}
    }
    public void ReceiveCarInfo(float timeBetweenObjs, InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    {
        _timeBetweenObjs = timeBetweenObjs;
        _currentFaceDir = currentFaceDir;

        receivedCarInfo = true;




        //_currentRoadDir = currentRoadDir;
    }
    //public void UpdateCarInfo(InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    //{
    //    _currentFaceDir = currentFaceDir;




    //    //_currentRoadDir = currentRoadDir;
    //}
    public string UpdateUI()
    {
        string result = "N/A";
        TrafficLights currentTrafficL;

        if (receivedCarInfo)
        {
            if (trafficLDic.TryGetValue(_current.tag, out currentTrafficL))
            {
                result = inputController.apiRequest.ToString() + "," + currentTrafficL.currentSignal.ToString();
            }
        }

        return result;
    }
    void Start()
    {
        inputController = FindObjectOfType<InputController>();

        calledIndex = 0;
        coIsRunning = false;
        _dualColDic = inputController.dualColDic;

        trafficLDic = trafficLights.ToDictionary(keySelector: m => m.parent.tag, elementSelector: m => m);

        UpdateLights(_colorIndex);






        //foreach (var trafficL in trafficLDic)
        //{
        //    Debug.LogError($"{trafficL.Key}={trafficL.Value.parent}={trafficL.Value.currentSignal}");
        //}
        //.ToDictionary(i => i.parent.tag, i => i);
        //collisionDetection = FindObjectOfType<CollisionDetection>();
        //if (collisionDetection == null)
        //{
        //    Debug.LogError("No collisionDetection script in the scene");
        //}


        //if (inputController == null)
        //{
        //    Debug.LogError("No inputController script in the scene");
        //}
        //Initialize all the traffic lights

        //_allColliders = collisionDetection.RetreiveAllColliders();
        //foreach (var collider in _allColliders)
        //{
        //    Debug.LogError($"Key: {collider.tag}");
        //}
        //dualColDic = new Dictionary<string, Tuple<bool, bool>>();
        //foreach (CollisionDetection collision in _allColliders)
        //{
        //    if (!collision.tag.Contains("Road") && !collision.tag.Contains("Beacon"))
        //    {
        //        Debug.LogError(dualColDic);
        //        if (!dualColDic.ContainsKey(collision.tag))
        //        {
        //            Debug.LogError("Entered Dic Col");
        //            dualColDic.Add(collision.tag, Tuple.Create(false, false));
        //        }
        //        else
        //        {
        //            Debug.LogError("Exited Dic Col");
        //            //return;
        //        }
        //    }
        //}
        //foreach (var collider in dualColDic)
        //{
        //    Debug.LogError($"Key: {collider.Key} Col1: {collider.Value.Item1} Col2: {collider.Value.Item2}");
        //}
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
            SendApiRequest();
            receivedCarInfo = false;
        }

        //if (updateDir)
        //{
        //    inputController.ReceiveApiObjRequest(InputController.apiEvents.UPDATEDIR, _current);
        //    updateDir = false;
        //}

        //if (roadDirChanged)
        //{
        //    Debug.LogError("Trigerreddddddddddddddddddddddd");
        //    Debug.LogError(roadDirChanged);
        //    inputController.ReceiveApiRequest(InputController.apiEvents.UPDATEDIR);
        //    foreach (TrafficLights light in trafficLights)
        //    {
        //        if (_currentRoadDir == InputController.FaceDir.South)
        //        {
                    
        //            Debug.LogError("11111111111111");
        //            if (_currentFaceDir == InputController.FaceDir.North)
        //            {
        //                Debug.LogError("22222222222222");
        //                if (light.parent.tag == "North TrafficL")
        //                {
        //                    Debug.LogError("33333333333333");
        //                    light.parent.GetComponent<Collider>().enabled = true;
        //                    //roadDirChanged = false;
        //                }
        //            }
        //        }
        //        else if (_currentRoadDir == InputController.FaceDir.East)
        //        {
        //            if (_currentFaceDir == InputController.FaceDir.West)
        //            {
        //                if (light.parent.tag == "West TrafficL")
        //                {
        //                    light.parent.GetComponent<Collider>().enabled = true;
        //                    //roadDirChanged = false;
        //                }
        //            }
        //        }
        //        else if (_currentRoadDir == InputController.FaceDir.West)
        //        {
        //            if (_currentFaceDir == InputController.FaceDir.East)
        //            {
        //                if (light.parent.tag == "East TrafficL")
        //                {
        //                    light.parent.GetComponent<Collider>().enabled = true;
        //                    //roadDirChanged = false;
        //                }
        //            }
        //        }
        //    }
        //    roadDirChanged = false;
        //}
    }
    //private void CarTrafficLLogic()
    //{
    //    Debug.LogError("Triggered CarTrafficLLogic");
    //    Debug.LogError(_currentRoadDir);
    //    if (_currentRoadDir == InputController.FaceDir.South)
    //    {
    //        Debug.Log("Triggered _currentRoadDir");
    //        if (_currentFaceDir == InputController.FaceDir.North)
    //        {
    //            Debug.Log("Triggered _currentFaceDir");
    //            //_current.GetComponent<Collider>().enabled = true;

    //            foreach (CollisionDetection collider in _allColliders)
    //            {
    //                if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
    //                {
    //                    Debug.Log("Triggered collider.tag");
    //                    collider.GetComponent<Collider>().enabled = false;
    //                }
    //            }
    //            //receivedCarInfo = false;
    //            SendApiRequest();
    //        }
    //    }
    //    else if (_currentRoadDir == InputController.FaceDir.East)
    //    {
    //        if (_currentFaceDir == InputController.FaceDir.West)
    //        {
    //            //_current.GetComponent<Collider>().enabled = true;

    //            foreach (CollisionDetection collider in _allColliders)
    //            {
    //                if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
    //                {
    //                    collider.GetComponent<Collider>().enabled = false;
    //                }
    //            }
    //            //receivedCarInfo = false;
    //            SendApiRequest();
    //        }
    //    }
    //    else if (_currentRoadDir == InputController.FaceDir.West)
    //    {
    //        if (_currentFaceDir == InputController.FaceDir.East)
    //        {
    //            //_current.GetComponent<Collider>().enabled = true;

    //            foreach (CollisionDetection collider in _allColliders)
    //            {
    //                if (collider.tag != _current.tag && !collider.tag.Contains("Road"))
    //                {
    //                    collider.GetComponent<Collider>().enabled = false;
    //                }
    //            }
    //            //receivedCarInfo = false;
    //            SendApiRequest();
    //        }
    //    }
    //    else
    //    {
    //        //North do nothing no traffic light
    //    }
    //}
    private void SendApiRequest()
    {
        Debug.LogError("11111");
        if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
        {
            if (trafficLDic.TryGetValue(_current.tag, out sendTrafficLReq))
            {
                Debug.LogError("22222");
                if (sendTrafficLReq.currentSignal.Equals(currentColor.Green))
                {
                    Debug.LogError("33333");
                    if (_timeBetweenObjs < (13 - _timer) && (13 - _timer) >= 0)
                    {
                        Debug.LogError("44444");
                        inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                    }
                    else
                    {
                        Debug.LogError("55555");
                        if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                        {
                            Debug.LogError("66666");
                            inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                        }
                        else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                        {
                            Debug.LogError("77777");
                            inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                        }
                    }
                }
                else if (sendTrafficLReq.currentSignal.Equals(currentColor.Yellow))
                {
                    Debug.LogError("33333");
                    if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                    {
                        Debug.LogError("44444");
                        inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    }
                    else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                    {
                        Debug.LogError("55555");
                        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                    }
                }
                else if (sendTrafficLReq.currentSignal.Equals(currentColor.Red))
                {
                    Debug.LogError("33333");
                    if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                    {
                        Debug.LogError("44444");
                        inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    }
                    else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                    {
                        Debug.LogError("55555");
                        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                    }
                }
            }
        }

        StartCoroutine(WaitForGreenL());



        //if (trafficLDic.TryGetValue(_current.tag, out sendTrafficLReq))
        //{
        //    if (sendTrafficLReq.currentSignal == currentColor.Green && _timeBetweenObjs < (13 - _timer))
        //    {
        //        inputController.ReceiveApiRequest(InputController.apiEvents.GO);
        //    }
        //    else if (sendTrafficLReq.currentSignal == currentColor.Yellow && _timeBetweenObjs < (3 - _timer))
        //    {
        //        inputController.ReceiveApiRequest(InputController.apiEvents.GO);
        //    }
        //    else if (sendTrafficLReq.currentSignal == currentColor.Red && _timeBetweenObjs > (10 - _timer))
        //    {
        //        inputController.ReceiveApiRequest(InputController.apiEvents.GO);
        //    }
        //    else
        //    {
        //        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
        //    }
        //}




        //foreach (TrafficLights light in trafficLights)
        //{
        //    Debug.LogError("Run For Loop");
        //    if (_current.tag == light.parent.tag)
        //    {
        //        Debug.LogError(_current.tag);
        //        Debug.LogError(light.parent.tag);
        //        if (light.currentSignal == currentColor.Green && _timeBetweenObjs < (13 - _timer))
        //        {
        //            inputController.ReceiveApiRequest(InputController.apiEvents.GO);
        //        }
        //        else if (light.currentSignal == currentColor.Yellow && _timeBetweenObjs < (3 - _timer))
        //        {
        //            inputController.ReceiveApiRequest(InputController.apiEvents.GO);
        //        }
        //        else if (light.currentSignal == currentColor.Red && _timeBetweenObjs > (10 - _timer))
        //        {
        //            inputController.ReceiveApiRequest(InputController.apiEvents.GO);
        //        }
        //        else
        //        {
        //            inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
        //        }
        //    }
        //}
    }
    IEnumerator WaitForGreenL()
    {
        if (coIsRunning)
        {
            yield break;
        }
        else
        {
            coIsRunning = true;
        }

        while (coIsRunning)
        {
            if (sendTrafficLReq.currentSignal.Equals(currentColor.Green))
            {
                inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                coIsRunning = false;
                yield break;
            }
            yield return null;
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
                    if (light.parent.tag == "North TrafficL")
                    {
                        SetLightGreen(light);
                    }
                    else
                    {
                        SetLightRed(light);
                    }
                    break;
                case 1:
                    if (light.parent.tag == "North TrafficL")
                    {
                        SetLightYellow(light);
                    }
                    else
                    {
                        SetLightRed(light);
                    }
                    break;
                case 2:
                    if (light.parent.tag == "North TrafficL")
                    {
                        SetLightRed(light);
                    }
                    else
                    {
                        SetLightGreen(light);
                    }
                    break;
                case 3:
                    if (light.parent.tag == "North TrafficL")
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





            //if (light.currentSignal == currentColor.Green && inputController.apiRequest == InputController.apiEvents.STOP && !receivedCarInfo)
            //{
            //    inputController.ReceiveApiRequest(InputController.apiEvents.GO);
            //}
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




    //public string GetApiRequest()
    //{
    //    // Signal can be set based on your actual implementation
    //    // For demonstration, it's set to "Red", "Orange", or "Green"
    //    //switch (currentSignal)
    //    //{
    //    //    case "Green":
    //    //        return "Go";
    //    //    case "Yellow":
    //    //        return "SlowDown";
    //    //    case "Red":
    //    //        return "Stop";
    //    //    default:
    //    //        return "";
    //    //}
    //    return "None";
    //}
}