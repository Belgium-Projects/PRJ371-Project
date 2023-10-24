using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InfoGUIController : MonoBehaviour
{
    //Editor variables
    [SerializeField] private UnityEngine.UI.Image speedBarHolder;
    [SerializeField] private TextMeshProUGUI speedHolder;
    [SerializeField] private TextMeshProUGUI eventHolder;

    //Global variables
    private InputController inputController;
    private float _speedFiller = 0f;
    private int _speed = 0;
    private string _event = "Stop";
    private string _currentApiEvent;
    void Start()
    {
        inputController = FindObjectOfType<InputController>();
        if (speedHolder == null || eventHolder == null)
        {
            Debug.LogError("The speed &| event holders is not added");
        }

        speedBarHolder.fillAmount = _speedFiller;
        speedHolder.text = _speed.ToString();
        eventHolder.text = _event;





        //if (inputController == null)
        //{
        //    Debug.LogError("No inputController script in the scene");
        //}
    }
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        CallUIinfo();
    }
    private void CallUIinfo()
    {
        _currentApiEvent = inputController.apiRequest.ToString();
    }
}
