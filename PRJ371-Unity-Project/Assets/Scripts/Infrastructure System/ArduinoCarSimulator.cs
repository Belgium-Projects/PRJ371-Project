using UnityEngine;

public class ArduinoCarSimulator : MonoBehaviour
{
    public float speed = 0.0f;
    public float acceleration = 2.0f;  // Acceleration/deceleration factor
    public GameObject trafficLight;  // Reference to the traffic light GameObject
    public GameObject car;  // Reference to the actual car GameObject
    public float beaconDetectionRadius = 50.0f;  // Radius within which the car detects the beacon
    private string apiRequest = "";  // Placeholder for the API request

    void Update()
    {
        // Check for the presence of a road maintenance beacon within the detection radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, beaconDetectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("RoadMaintenanceBeacon"))
            {
                ReceiveApiRequest("SlowDown");
            }
        }
        // Simulate receiving an API request from the traffic light
        apiRequest = trafficLight.GetComponent<TrafficLightController>().GetApiRequest();

        // Reacting to the API request
        switch (apiRequest)
        {
            case "Go":
                // Incrementally speed up the car to normal speed
                speed = Mathf.Min(speed + acceleration * Time.deltaTime, 10.0f);
                break;
            case "SlowDown":
                // Incrementally slow down the car
                speed = Mathf.Max(speed - acceleration * Time.deltaTime, 5.0f);
                break;
            case "Stop":
                // Incrementally bring the car to a stop
                speed = Mathf.Max(speed - acceleration * Time.deltaTime, 0.0f);
                break;
            default:
                // Do nothing (or maintain current speed)
                break;
        }
        // Move the car based on the updated speed
        car.transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    // This method allows external objects to send API requests to the car
    public void ReceiveApiRequest(string request)
    {
        apiRequest = request;
    }
}