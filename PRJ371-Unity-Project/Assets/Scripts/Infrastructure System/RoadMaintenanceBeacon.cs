using UnityEngine;

public class RoadMaintenanceBeacon : MonoBehaviour
{
    // Radius within which cars should be warned
    public float warningRadius = 50.0f;
    private SphereCollider beaconCollider;
    private void Start()
    {
        // Set up the beacon's trigger zone
        beaconCollider = gameObject.AddComponent<SphereCollider>();
        beaconCollider.isTrigger = true;
        beaconCollider.radius = warningRadius;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger zone is a car
        if (other.CompareTag("Car"))
        {
            // Send a warning to the car to slow down
            ArduinoCarSimulator carSimulator = other.GetComponent<ArduinoCarSimulator>();
            if (carSimulator != null)
            {
                carSimulator.ReceiveApiRequest("SlowDown");
            }
        }
    }
}