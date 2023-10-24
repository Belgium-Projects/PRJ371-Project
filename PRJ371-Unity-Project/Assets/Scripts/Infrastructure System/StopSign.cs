using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopSign : MonoBehaviour
{
    //Editor variables
    [SerializeField] private GameObject stopSign;

    //Global variables
    private GameObject _current;
    private InputController inputController;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    private bool receivedCarInfo;
    private InputController.FaceDir _currentFaceDir;
    private InputController.FaceDir _currentRoadDir;
    private int calledIndex;
    public void ColliderTriggered(GameObject current)
    {
        _current = current;

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
        else
        {
            _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);
        }
    }
    public void ReceiveCarInfo(InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    {
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;

        receivedCarInfo = true;
    }
    void Start()
    {
        inputController = FindObjectOfType<InputController>();
        if (inputController == null)
        {
            Debug.LogError("No inputController script in the scene");
        }
        calledIndex = 0;
        _dualColDic = inputController.dualColDic;
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

    }
}
