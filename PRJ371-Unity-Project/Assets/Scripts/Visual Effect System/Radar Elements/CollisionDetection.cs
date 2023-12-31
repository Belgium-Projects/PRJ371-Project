using System.Linq;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    //Global variables
    private InputController inputController;
    private RoadMaintenanceBeacon roadMaintenanceBeacon;
    private TrafficLight trafficLight;
    private StopSign stopSign;
    void Start()
    {
        //Finds the required scripts
        inputController = FindObjectOfType<InputController>();
        roadMaintenanceBeacon = FindObjectOfType<RoadMaintenanceBeacon>();
        trafficLight = FindObjectOfType<TrafficLight>();
        stopSign = FindObjectOfType<StopSign>();

        //Method call to get all componenets with this script on it
        RetreiveAllColliders();
    }
    public CollisionDetection[] RetreiveAllColliders()
    {
        //Creates an array with all the colliders
        CollisionDetection[] allColliders = FindObjectsOfType(typeof(CollisionDetection)) as CollisionDetection[];

        return allColliders.Distinct().ToArray();
    }
    private void OnTriggerEnter(Collider other)
    {
        //Tag checks to get what infrastructure colliders where entered
        //Sends the necessary info to the scripts accordingly
        if (other.CompareTag("Car"))
        {
            switch (this.tag)
            {
                case "North Road":
                    inputController.currentRoadDir = InputController.FaceDir.North;
                    stopSign.ColliderTriggered(this.gameObject, true);
                    break;
                case "East Road":
                    inputController.currentRoadDir = InputController.FaceDir.East;
                    inputController.CarDirectionCalc();
                    trafficLight.ColliderTriggered(this.gameObject, true);
                    break;
                case "South Road":
                    inputController.currentRoadDir = InputController.FaceDir.South;
                    inputController.CarDirectionCalc();
                    trafficLight.ColliderTriggered(this.gameObject, true);
                    break;
                case "West Road":
                    inputController.currentRoadDir = InputController.FaceDir.West;
                    inputController.CarDirectionCalc();
                    trafficLight.ColliderTriggered(this.gameObject, true);
                    break;
                case "Bend Road":
                    inputController.currentRoadDir = InputController.FaceDir.Bend;
                    inputController.ColliderTriggered(false, false);
                    break;
                case "Intersection Road":
                    inputController.currentRoadDir = InputController.FaceDir.Bend;
                    inputController.ColliderTriggered(false, true);
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
                    stopSign.ColliderTriggered(this.gameObject, false);
                    break;
                case "Beacon":
                    roadMaintenanceBeacon.ColliderTriggered(this.gameObject, false);
                    break;
                default: 
                    break;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Tag checks to get what infrastructure colliders where exited
        //Sends the necessary info to the scripts accordingly
        if (other.CompareTag("Car"))
        {
            if (this.tag.Contains("Beacon"))
            {
                roadMaintenanceBeacon.ColliderTriggered(this.gameObject, true);
            }
            else if (this.tag.Contains("Bend"))
            {
                inputController.ColliderTriggered(true, false);
            }
            else if (this.tag.Contains("Intersection"))
            {
                inputController.ColliderTriggered(true, true);
            }
        }
    }
}
