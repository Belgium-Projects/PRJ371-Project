using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.FilterWindow;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI changedStateText;
}
public class InfoGUIController : MonoBehaviour
{
    //Editor variables
    [SerializeField] private CarUIelements carUIelements;
    [SerializeField] private MenuUIelements menuUIelements;

    //Global variables
    private InputController inputController;
    private string _currentApiEvent;
    private string _infrastructureDis;
    private bool _warning;
    private bool _infrastructureDisActive;
    private bool _gamepaused;
    private float _speed;
    private float _speedFiller;
    private float _maxSpeed;
    void Start()
    {
        //Finds scripts necessary
        inputController = FindObjectOfType<InputController>();

        //Initialize variables
        _currentApiEvent = "Go";
        _warning = false;
        _speedFiller = 0f;
        _maxSpeed = 0f;
        _speed = 0f;
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

        _gamepaused = true;
        menuUIelements.changedStateText.color = new Color32(255, 246, 0, 255);
        Time.timeScale = 0f;
    }
    void Update()
    {
        //Method call
        CallUIinfo();
    }
    private void CallUIinfo()
    {
        //Gets the values from the InputController
        _currentApiEvent = inputController.ApiRequest.ToString().ToLower();
        _warning = inputController.Warning;
        _speed = inputController.Speed;
        _maxSpeed = inputController.MaxSpeed;
        _infrastructureDisActive = inputController.InfrastructureDisActive;
        _infrastructureDis = inputController.InfrastructureDis.ToString("f0");

        //Method call with retreived variables
        CalculateUIvalues(_currentApiEvent, _warning, _speedFiller, _speed, _infrastructureDisActive, _infrastructureDis);
    }
    private void CalculateUIvalues(string currentApiEvent, bool warning, float speedFiller, float speed, bool infrastructureDisActive, string infrastructureDis)
    {
        //Change the values for display
        currentApiEvent = $"Event: {currentApiEvent.FirstCharacterToUpper()}";

        //Clamping value between 0~1 & increasing it by 0.001f
        speedFiller = Mathf.Lerp(speed / _maxSpeed, 1, 0.001f);

        //Checks if infrastructureDis is true then set the distance
        if (infrastructureDisActive)
        {
            infrastructureDis = $"{infrastructureDis}m";
        }

        //Method call with changed variables
        SetUIvalues(currentApiEvent, warning, speedFiller, speed, infrastructureDisActive, infrastructureDis);
    }
    private void SetUIvalues(string currentApiEvent, bool warning, float speedFiller, float speed, bool infrastructureDisActive, string infrastructureDis)
    {
        //Setting the variable values to UI elements
        carUIelements.currentEvent.text = currentApiEvent;
        carUIelements.warningSign.SetActive(warning);
        carUIelements.speedBar.fillAmount = speedFiller;
        carUIelements.speedReadout.text = speed.ToString("f0");
        carUIelements.distanceReadout.transform.parent.gameObject.SetActive(infrastructureDisActive);

        //Checks if infrastructureDis is Active
        if (infrastructureDisActive)
        {
            carUIelements.distanceReadout.text = infrastructureDis;
        }
    }
    public void ChangeGameState()
    {
        //Change game state if clicked
        _gamepaused = _gamepaused ? false : true;

        //Freezes game if paused & unfreeze if play
        if (_gamepaused)
        {
            Time.timeScale = 0f;
            menuUIelements.changedStateText.color = new Color32(255, 246, 0, 255);
        }
        else
        {
            Time.timeScale = 1f;
            menuUIelements.changedStateText.color = new Color32(255, 255, 255, 200);
        }
    }
    public void RestartGame()
    {
        //Reloads main scene
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        //Quits simulation
        Application.Quit();
    }
}
