using System.Collections;
using UnityEngine;

public class RoadMaintenanceBeacon : MonoBehaviour
{
    //Editor variables
    //[SerializeField] private float warningRadius = 50.0f;
    [SerializeField] private GameObject beaconObj;

    //Global variables
    private SphereCollider beaconCollider;
    private ArduinoCarSimulator carSimulator;
    private void Start()
    {
        //Get the beacon sphere collider and Arduino script
        beaconCollider = beaconObj.GetComponent<SphereCollider>();
        carSimulator = gameObject.GetComponent<ArduinoCarSimulator>();

        //Check if objects exist in the scene
        if (beaconCollider == null )
        {
            Debug.LogError("No sphere collider added to the game object");
        }
        if (carSimulator == null)
        {
            Debug.LogError("No ArduinoCarSimilator script in the scene");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //Check if the collider entered is the car
        if (other.CompareTag("Car"))
        {
            //Send a warning to the car to slow down  
            Debug.Log("Slow Down");
            //carSimulator.ReceiveApiRequest("SlowDown");
        }
    }
}