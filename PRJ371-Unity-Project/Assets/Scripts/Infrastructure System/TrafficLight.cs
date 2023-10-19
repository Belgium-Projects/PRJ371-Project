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
    private string _currentRoadDir;
    public string currentRoadDir { get {return _currentRoadDir;} set { _currentRoadDir = value;}}
    public void ColliderTriggered(Collider other, GameObject current)
    {
        Debug.Log(current.tag);

    }
    void Start()
    {
        inputController = FindObjectOfType<InputController>();

        if (inputController == null)
        {
            Debug.LogError("No inputController script in the scene");
        }
        //Initialize all the traffic lights
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