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
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            switch (this.tag)
            {
                case "North Road":
                    trafficLight.currentRoadDir = this.tag;
                    break;
                case "East Road":
                    trafficLight.currentRoadDir = this.tag;
                    break;
                case "South Road":
                    trafficLight.currentRoadDir = this.tag;
                    break;
                case "West Road":
                    trafficLight.currentRoadDir = this.tag;
                    break;
                case "Bend Road":
                    trafficLight.currentRoadDir = this.tag;
                    break;
                case "North TrafficL":
                    trafficLight.ColliderTriggered(other, this.gameObject);
                    break;
                case "East TrafficL":
                    trafficLight.ColliderTriggered(other, this.gameObject);
                    break;
                case "West TrafficL":
                    trafficLight.ColliderTriggered(other, this.gameObject);
                    break;
                case "West StopS":
                    trafficLight.ColliderTriggered(other, this.gameObject);
                    break;
                case "Beacon":
                    roadMaintenanceBeacon.ColliderTriggered(other, this.gameObject);
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
}
