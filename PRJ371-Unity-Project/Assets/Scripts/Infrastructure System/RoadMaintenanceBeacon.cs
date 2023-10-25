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
    private int calledIndex;
    public void ColliderTriggered(GameObject current, bool resetCol)
    {
        _current = current;

        if (resetCol)
        {
            if (calledIndex == 0)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);
                inputController.ReceiveApiRequest(InputController.apiEvents.GO);
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
    public void ReceiveCarInfo(float distanceBeforeM)
    {
        _distanceBeforeM = distanceBeforeM;

        receivedCarInfo = true;
    }
    public string UpdateUI()
    {
        string result = "NA";
        result = _distanceBeforeM.ToString("f0");
        //if (_distanceBetweenObjs >= 0)
        //{
        //    result = _distanceBetweenObjs.ToString("f0");
        //}
        //else if (_distanceBetweenObjs < 0)
        //{
        //    result = _beaconDistanceLeft.ToString("f0");
        //}

        return result;
    }
    private void Start()
    {
        inputController = FindObjectOfType<InputController>();

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