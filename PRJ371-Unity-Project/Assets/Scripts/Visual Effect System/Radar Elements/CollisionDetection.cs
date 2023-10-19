using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    private InputController inputController;
    private RoadMaintenanceBeacon roadMaintenanceBeacon;
    private TrafficLight trafficLight;
    void Start()
    {
        inputController = FindObjectOfType<InputController>();
        roadMaintenanceBeacon = FindObjectOfType<RoadMaintenanceBeacon>();
        trafficLight = FindObjectOfType<TrafficLight>();

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
                case "Beacon":
                    roadMaintenanceBeacon.ColliderTriggered(true, other, this.gameObject);
                    break;
                case "North":
                    trafficLight.ColliderTriggered(true, other, this.gameObject);
                    break;
                case "East":
                    trafficLight.ColliderTriggered(true, other, this.gameObject);
                    break;
                case "West":
                    trafficLight.ColliderTriggered(true, other, this.gameObject);
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
