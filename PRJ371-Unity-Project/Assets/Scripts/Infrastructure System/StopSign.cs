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
    private InputController.FaceDir _currentFaceDir;
    private InputController.FaceDir _currentRoadDir;
    private Dictionary<string, Tuple<bool, bool>> _dualColDic;
    private Tuple<bool, bool> currentColDic;
    private bool receivedCarInfo;
    private bool coIsRunning;
    private int calledIndex;
    public void ColliderTriggered(GameObject current, bool resetCol)
    {
        //Currents is the infrastructure GameObject
        _current = current;

        //Called when the Collider is entered
        if (resetCol)
        {
            if (calledIndex == 0)
            {
                _dualColDic[current.tag] = new Tuple<bool, bool>(false, false);
            }
        }
        else
        {
            //Request info from the Input Controller
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
        //Variables received by Input Controller
        _currentFaceDir = currentFaceDir;
        _currentRoadDir = currentRoadDir;

        receivedCarInfo = true;
    }
    void Start()
    {
        //Gets the Input Controller script
        inputController = FindObjectOfType<InputController>();

        //Iniliazizes variables
        calledIndex = 0;
        coIsRunning = false;
        _dualColDic = inputController.dualColDic;
    }
    private void LateUpdate()
    {
        //Sends final request to Input Controller when info is received
        if (receivedCarInfo)
        {
            SendApiRequest();
            receivedCarInfo = false;
        }
    }
    private void SendApiRequest()
    {
        //Condition checks for the stop sign & sending appropriate event
        if (_dualColDic.TryGetValue(_current.tag, out currentColDic))
        {
            if (_currentRoadDir == InputController.FaceDir.North)
            {
                if (_currentFaceDir == InputController.FaceDir.North)
                {
                    if (currentColDic.Equals(new Tuple<bool, bool>(true, false)))
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    }
                    else if (currentColDic.Equals(new Tuple<bool, bool>(true, true)))
                    {
                        inputController.ReceiveApiRequest(InputController.apiEvents.STOP);
                        StartCoroutine(WaitForThreeSec());
                    }
                }
            }
        }
    }
    IEnumerator WaitForThreeSec()
    {
        //Co-routine to wait 3 seconds before event is set to Go
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
