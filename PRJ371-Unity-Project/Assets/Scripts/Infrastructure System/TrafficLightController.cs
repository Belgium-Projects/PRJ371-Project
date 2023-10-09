using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TrafficLightRespond : MonoBehaviour
{
    public float speed = 0.0f;
    public float acceleration = 2.0f;  // Acceleration/deceleration factor
    public GameObject trafficLight;  // Reference to the traffic light GameObject
    public GameObject car;  // Reference to the actual car GameObject
    private string apiRequest = "";  // Placeholder for the API request

    void Update()
    {
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
}
public class TrafficLightController : MonoBehaviour
{
    private string currentSignal = "Red";  // Placeholder for the current traffic light signal
    public string GetApiRequest()
    {
        // Signal can be set based on your actual implementation
        // For demonstration, it's set to "Red", "Orange", or "Green"
        switch (currentSignal)
        {
            case "Green":
                return "Go";
            case "Orange":
                return "SlowDown";
            case "Red":
                return "Stop";
            default:
                return "";
        }
    }
}