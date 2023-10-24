using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    //Global variables
    private InputController inputController;
    private RoadMaintenanceBeacon roadMaintenanceBeacon;
    private TrafficLight trafficLight;
    private StopSign stopSign;
    private Dictionary<string, Tuple<bool ,bool>> dualColDic;
    CollisionDetection[] _allColliders;
    //private InputController InputController;
    void Start()
    {
        inputController = FindObjectOfType<InputController>();
        roadMaintenanceBeacon = FindObjectOfType<RoadMaintenanceBeacon>();
        trafficLight = FindObjectOfType<TrafficLight>();
        stopSign = FindObjectOfType<StopSign>();

        RetreiveAllColliders();



        //_allColliders = RetreiveAllColliders();
        //dualColDic = new Dictionary<string, Tuple<bool, bool>>();
        //PopulateDic();
        //foreach (var collider in dualColDic)
        //{
        //    Debug.LogError($"Key: {collider.Key} Col1: {collider.Value.Item1} Col2: {collider.Value.Item2}");
        //}
    }
    //private void PopulateDic()
    //{
    //    //_allColliders = RetreiveAllColliders();
    //    //dualColDic = new Dictionary<string, Tuple<bool, bool>>();
    //    //foreach (var collider in _allColliders)
    //    //{
    //    //    Debug.LogError($"Key: {collider.tag}");
    //    //}
    //    ////foreach (CollisionDetection collision in _allColliders)
    //    ////{
    //    ////    if (!collision.tag.Contains("Road") && !collision.tag.Contains("Beacon"))
    //    ////    {
    //    ////        Debug.LogError(dualColDic);
    //    ////        if (!dualColDic.ContainsKey(collision.tag))
    //    ////        {
    //    ////            Debug.LogError("Entered Dic Col");
    //    ////            dualColDic.Add(collision.tag, Tuple.Create(false, false));
    //    ////        }
    //    ////        else
    //    ////        {
    //    ////            Debug.LogError("Exited Dic Col");
    //    ////            //return;
    //    ////        }
    //    ////    }
    //    ////}
    //    //foreach (var collider in dualColDic)
    //    //{
    //    //    Debug.LogError($"Key: {collider.Key} Col1: {collider.Value.Item1} Col2: {collider.Value.Item2}");
    //    //}
    //}
    public CollisionDetection[] RetreiveAllColliders()
    {
        CollisionDetection[] allColliders = FindObjectsOfType(typeof(CollisionDetection)) as CollisionDetection[];
        //Debug.Log("Found " + myItems.Length + " instances with this script attached");
        //foreach (CollisionDetection item in myItems)
        //{
        //    Debug.Log(item.gameObject.name);
        //}
        return allColliders.Distinct().ToArray();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            switch (this.tag)
            {
                case "North Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.North;
                    stopSign.ColliderTriggered(this.gameObject, true);
                    //inputController.ReceiveApiRequest(InputController.apiEvents.GO);
                    Debug.LogError("North Collision");
                    break;
                case "East Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.East;
                    trafficLight.ColliderTriggered(this.gameObject, true);
                    Debug.LogError("East Collision");
                    break;
                case "South Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.South;
                    trafficLight.ColliderTriggered(this.gameObject, true);
                    Debug.LogError("South Collision");
                    break;
                case "West Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.West;
                    trafficLight.ColliderTriggered(this.gameObject, true);
                    Debug.LogError("West Collision");
                    break;
                case "Bend Road":
                    inputController.currentRoadDir = InputController.FaceDir.Bend;
                    //inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
                    inputController.ColliderTriggered(this.gameObject, true);
                    Debug.LogError("Bend Collision");
                    break;
                case "North TrafficL":
                    trafficLight.ColliderTriggered(this.gameObject, false);
                    break;
                case "East TrafficL":
                    trafficLight.ColliderTriggered(this.gameObject, false);
                    break;
                case "West TrafficL":
                    trafficLight.ColliderTriggered(this.gameObject, false);
                    break;
                case "North StopS":
                    //trafficLight.ColliderTriggered(other, this.gameObject);
                    stopSign.ColliderTriggered(this.gameObject, false);
                    break;
                case "Beacon":
                    roadMaintenanceBeacon.ColliderTriggered(other, this.gameObject, true);
                    break;
                default: 
                    break;
            }
            //Send a warning to the car to slow down  
            //inputController.ReceiveApiRequest(InputController.apiEvents.SLOWDOWN);
            //Debug.Log("Slow Down");
            //carSimulator.ReceiveApiRequest("SlowDown");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (this.tag.Contains("Beacon"))
            {
                roadMaintenanceBeacon.ColliderTriggered(other, this.gameObject, false);
            }
            else if (this.tag.Contains("Bend"))
            {
                inputController.ColliderTriggered(this.gameObject, false);
            }
        }
    }
}
