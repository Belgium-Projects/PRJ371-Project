using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.FilterWindow;

[System.Serializable]
public class CarUIelements
{
    //Creates custom parameters for Car GUI elements
    public TextMeshProUGUI currentEvent;
    public GameObject warningSign;
    public UnityEngine.UI.Image speedBar;
    public TextMeshProUGUI speedReadout;
    public TextMeshProUGUI distanceReadout;
}
[System.Serializable]
public class MenuUIelements
{
    //Creates custom parameters Menu GUI elements
    public GameObject playPauseBut;
    public GameObject restartBut;
    public GameObject exitBut;
}
public class InfoGUIController : MonoBehaviour
{
    //Editor variables
    [SerializeField] private CarUIelements carUIelements;
    [SerializeField] private MenuUIelements menuUIelements;

    //Global variables
    private InputController inputController;
    private string _currentApiEvent;
    private string _speed;
    private string _infrastructureDis;
    private bool _warning;
    private bool _infrastructureDisActive;
    private float _speedFiller;
    void Start()
    {
        //Make bool to send through with trafficls & Beacon & StopS to calculatedistance and send to UI in Inputcontroller
        //Set camera starting point at car on angle
        //Change InfrastructureDis to * 36 in Inputcontroller

        //Finds scripts necessary
        inputController = FindObjectOfType<InputController>();

        //Initialize variables
        _currentApiEvent = "Go";
        _warning = false;
        _speedFiller = 0f;
        _speed = "0";
        _infrastructureDisActive = false;
        _infrastructureDis = "0m";
    }
    private void Awake()
    {
        //Checks if objects is set in the Editor
        if (carUIelements.currentEvent.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(carUIelements.currentEvent));
        if (carUIelements.warningSign.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(carUIelements.warningSign));
        if (carUIelements.speedBar.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(carUIelements.speedBar));
        if (carUIelements.speedReadout.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(carUIelements.speedReadout));
        if (carUIelements.distanceReadout.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(carUIelements.distanceReadout));

        if (menuUIelements.playPauseBut.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(menuUIelements.playPauseBut));
        if (menuUIelements.restartBut.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(menuUIelements.restartBut));
        if (menuUIelements.exitBut.Equals(null)) Debug.LogErrorFormat(this, "Member \"{0}\" is required.", nameof(menuUIelements.exitBut));
    }
    void Update()
    {
        //Method call
        CallUIinfo();
    }
    private void CallUIinfo()
    {
        //Gets the values from the InputController
        _currentApiEvent = inputController.ApiRequest.ToString();
        _warning = inputController.Warning;
        _speed = inputController.Speed.ToString("fo");
        _infrastructureDis = inputController.InfrastructureDis.ToString("fo");

        //Method call with retreived variables
        CalculateUIvalues(_currentApiEvent, _warning, _speedFiller, _speed, _infrastructureDisActive, _infrastructureDis);
    }
    private void CalculateUIvalues(string currentApiEvent, bool warning, float speedFiller, string speed, bool infrastructureDisActive, string infrastructureDis)
    {
        //Change the values for display
        currentApiEvent = $"Event: {currentApiEvent}";
        speedFiller = Mathf.Clamp(speedFiller, 0f, 1f);

        //Checks if infrastructureDis is outputting a 0 value
        if (infrastructureDis.Equals("0"))
        {
            infrastructureDisActive = false;
        }
        else
        {
            infrastructureDis = $"{infrastructureDis}m";
            infrastructureDisActive = true;
        }

        //Method call with changed variables
        SetUIvalues(currentApiEvent, warning, speedFiller, speed, infrastructureDisActive, infrastructureDis);
    }
    private void SetUIvalues(string currentApiEvent, bool warning, float speedFiller, string speed, bool infrastructureDisActive, string infrastructureDis)
    {
        //Setting the variable values to UI elements
        carUIelements.currentEvent.text = currentApiEvent;
        carUIelements.warningSign.SetActive(warning);
        carUIelements.speedBar.fillAmount = speedFiller;
        carUIelements.speedReadout.text = speed;
        carUIelements.distanceReadout.GetComponentInParent<GameObject>().SetActive(infrastructureDisActive);

        //Checks if infrastructureDis is Active
        if (infrastructureDisActive)
        {
            carUIelements.distanceReadout.text = infrastructureDis;
        }
    }
}
