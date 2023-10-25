using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private GameObject _current;
    private InputController inputController;
    private Dictionary<string, TrafficLights> trafficLDic;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    private Tuple<bool, bool> currentColDic;
    private TrafficLights selectTrafficL;
    private TrafficLights sendTrafficLReq;
    private bool receivedCarInfo;
    private bool coIsRunning;
    private float _timer = 0f;
    private float _timeBetweenObjs;
    private int _colorIndex = 0;
    private int calledIndex;
    
    //Get|Set variables
    public bool roadDirChanged { get; set; }
    public void ColliderTriggered(GameObject current, bool resetCol)
    {
        _current = current;

        if (resetCol)
        {
            if (calledIndex == 0)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);

                if (current.tag.Contains("South"))
                {
                    if (inputController.currentFaceDir.Equals(InputController.FaceDir.North))
                    {
                        if (trafficLDic.TryGetValue("North TrafficL", out selectTrafficL))
                        {
                            ChangeTrafficLCol(selectTrafficL);
                        }
                    }
                }
                else if (current.tag.Contains("East"))
                {
                    if (inputController.currentFaceDir.Equals(InputController.FaceDir.West))
                    {
                        if (trafficLDic.TryGetValue("West TrafficL", out selectTrafficL))
                        {
                            ChangeTrafficLCol(selectTrafficL);
                        }
                    }
                }
                else if (current.tag.Contains("West"))
                {
                    if (inputController.currentFaceDir.Equals(InputController.FaceDir.East))
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
    }
    public void ReceiveCarInfo(float timeBetweenObjs)
    {
        _timeBetweenObjs = timeBetweenObjs;

        receivedCarInfo = true;
    }
    void Start()
    {
        inputController = FindObjectOfType<InputController>();

        calledIndex = 0;
        coIsRunning = false;
        _dualColDic = inputController.dualColDic;

        trafficLDic = trafficLights.ToDictionary(keySelector: m => m.parent.tag, elementSelector: m => m);

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
            SendApiRequest();
            receivedCarInfo = false;
        }
    }
    private void SendApiRequest()
    {
        if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
        {
            if (trafficLDic.TryGetValue(_current.tag, out sendTrafficLReq))
            {
                if (sendTrafficLReq.currentSignal.Equals(currentColor.Green))
                {
                    if (_timeBetweenObjs < (13 - _timer) && (13 - _timer) >= 0)
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                    }
                    else
                    {
                        if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                        {
                            inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                        }
                        else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                        {
                            inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                        }
                    }
                }
                else if (sendTrafficLReq.currentSignal.Equals(currentColor.Yellow))
                {
                    if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    }
                    else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                    }
                }
                else if (sendTrafficLReq.currentSignal.Equals(currentColor.Red))
                {
                    if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    }
                    else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                    }
                }
            }
        }

        StartCoroutine(WaitForGreenL());
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
}