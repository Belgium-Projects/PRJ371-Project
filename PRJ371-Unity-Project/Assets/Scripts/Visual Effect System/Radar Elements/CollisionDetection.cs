using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    //Global variables
    private InputController inputController;
    private RoadMaintenanceBeacon roadMaintenanceBeacon;
    private TrafficLight trafficLight;
    private InputController InputController;
    void Start()
    {
        inputController = FindObjectOfType<InputController>();
        roadMaintenanceBeacon = FindObjectOfType<RoadMaintenanceBeacon>();
        trafficLight = FindObjectOfType<TrafficLight>();
        InputController = FindObjectOfType<InputController>();

        if (inputController == null)
        {
            Debug.LogError("No inputController script in the scene");
        }
        else if (roadMaintenanceBeacon == null)
        {
            Debug.LogError("No roadMaintenanceBeacon script in the scene");
        }
        else if (trafficLight == null)
        {
            Debug.LogError("No trafficLight script in the scene");
        }
        RetreiveAllColliders();
    }
    public CollisionDetection[] RetreiveAllColliders()
    {
        CollisionDetection[] allColliders = FindObjectsOfType(typeof(CollisionDetection)) as CollisionDetection[];
        //Debug.Log("Found " + myItems.Length + " instances with this script attached");
        //foreach (CollisionDetection item in myItems)
        //{
        //    Debug.Log(item.gameObject.name);
        //}
        return allColliders;
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
                    Debug.LogError("North Collision");
                    break;
                case "East Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.East;
                    trafficLight.ColliderTriggered(other, this.gameObject, false);
                    Debug.LogError("East Collision");
                    break;
                case "South Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.South;
                    trafficLight.ColliderTriggered(other, this.gameObject, false);
                    Debug.LogError("South Collision");
                    break;
                case "West Road":
                    //trafficLight.roadDirChanged = true;
                    inputController.currentRoadDir = InputController.FaceDir.West;
                    trafficLight.ColliderTriggered(other, this.gameObject, false);
                    Debug.LogError("West Collision");
                    break;
                case "Bend Road":
                    inputController.currentRoadDir = InputController.FaceDir.Bend;
                    Debug.LogError("Bend Collision");
                    break;
                case "North TrafficL":
                    trafficLight.ColliderTriggered(other, this.gameObject, true);
                    break;
                case "East TrafficL":
                    trafficLight.ColliderTriggered(other, this.gameObject, true);
                    break;
                case "West TrafficL":
                    trafficLight.ColliderTriggered(other, this.gameObject, true);
                    break;
                case "North StopS":
                    //trafficLight.ColliderTriggered(other, this.gameObject);
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
            if (this.tag == "Beacon")
            {
                roadMaintenanceBeacon.ColliderTriggered(other, this.gameObject, false);
            }
        }
    }
}
