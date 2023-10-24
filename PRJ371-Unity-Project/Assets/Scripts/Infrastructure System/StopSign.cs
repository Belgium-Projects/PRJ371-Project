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
    private Tuple<bool, bool> currentColDic;
    public void ColliderTriggered(GameObject current, bool resetCol)
    {
        _current = current;

        if (resetCol)
        {
            if (calledIndex == 0)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);
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
        //inputController.ReceiveApiObjRequest(InputController.apiEvents.UPDATEDIR, current);
        //if (current.tag.Contains("North"))
        //{
        //    if (_currentFaceDir == InputController.FaceDir.North)
        //    {

        //    }
        //}
    }
    public void ReceiveCarInfo(InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    {
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;

        receivedCarInfo = true;
    }
    //public void UpdateCarInfo(InputController.FaceDir currentFaceDir, InputController.FaceDir currentRoadDir)
    //{
    //    _currentFaceDir = currentFaceDir;
    //    _currentRoadDir = currentRoadDir;
    //}
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
        if (_current.tag.Contains("North"))
        {
            if (_currentFaceDir == InputController.FaceDir.North)
            {
                if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
                {
                    switch (currentColDic)
                    {
                        case Tuple<bool, bool>(false, false):
                            Debug.LogError("Tuple Reset");
                            break;
                        case Tuple<bool, bool>(true, false):
                            inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                            break;
                        case Tuple<bool, bool>(true, true):
                            inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
