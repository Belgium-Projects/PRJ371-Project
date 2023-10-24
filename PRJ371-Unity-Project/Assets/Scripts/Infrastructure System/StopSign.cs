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
    private bool coIsRunning;
    private int stopWaitCounter;
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

        calledIndex = 0;
        stopWaitCounter = 0;
        coIsRunning = false;
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
        //Add Car Go Event Logic(Get car speed = 0 maybe?)
        Debug.LogError("11111");
        if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
        {
            Debug.LogError("22222");
            if (_currentRoadDir == InputController.FaceDir.North)
            {
                Debug.LogError("33333");
                if (_currentFaceDir == InputController.FaceDir.North)
                {
                    Debug.LogError("444444");
                    if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                    {
                        Debug.LogError("55555");
                        inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    }
                    else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                    {
                        Debug.LogError("66666");
                        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                        StartCoroutine(WaitForThreeSec());
                    }
                }
            }
        }



        //if (_currentRoadDir == InputController.FaceDir.North)
        //{
        //    if (_currentFaceDir == InputController.FaceDir.North)
        //    {
        //        if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
        //        {
        //            switch (currentColDic)
        //            {
        //                case Tuple<bool, bool>(false, false):
        //                    Debug.LogError("Tuple Reset");
        //                    break;
        //                case Tuple<bool, bool>(true, false):
        //                    inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
        //                    break;
        //                case Tuple<bool, bool>(true, true):
        //                    inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //}
    }
    IEnumerator WaitForThreeSec()
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
            yield return new WaitForSeconds(3);
            inputController.ReceiveApiRequest(InputController.apiEvents.GO);
            coIsRunning = false;
        }
    }
}
