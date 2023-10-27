using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMaintenanceBeacon : MonoBehaviour
{
    //Editor variables
    [SerializeField] private GameObject beaconObj;

    //Global variables
    private GameObject _current;
    private InputController inputController;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    private Tuple<bool, bool> currentColDic;
    private bool receivedCarInfo;
    private float _distanceBeforeM;
    private int enterIndex;
    private int exitIndex;
    public void ColliderTriggered(GameObject current, bool resetCol)
    {
        _current = current;

        if (resetCol)
        {
            exitIndex++;

            if (exitIndex == 1)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, true);
            }
            else if (exitIndex == 2)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);
                inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                exitIndex = 0;
            }
        }
        else
        {
            inputController.ReceiveApiObjRequest(InputController.apiEvents.SENDINFO, current);
            enterIndex++;

            if (enterIndex == 1)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(true, false);
            }
            else if (enterIndex == 2)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(true, true);
                enterIndex = 0;
            }
        }
    }
    public void ReceiveCarInfo(float distanceBeforeM)
    {
        //Start slowing down if this distance = 100m for example
        _distanceBeforeM = distanceBeforeM;

        receivedCarInfo = true;
    }
    private void Start()
    {
        inputController = FindObjectOfType<InputController>();

        enterIndex = 0;
        exitIndex = 0;
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
        if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
        {
            if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
            {
                inputController.ReceiveApiRequest(InputController.apiEvents.WARNING);
            }
            else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
            {
                inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
            }
        }
    }
}