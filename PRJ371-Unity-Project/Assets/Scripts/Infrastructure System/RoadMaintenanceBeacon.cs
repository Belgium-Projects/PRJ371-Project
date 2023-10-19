using System.Collections;
using UnityEngine;

public class RoadMaintenanceBeacon : MonoBehaviour
{
    //Editor variables
    //[SerializeField] private float warningRadius = 50.0f;
    [SerializeField] private GameObject beaconObj;
    //[SerializeField] private GameObject carObj;

    //Global variables
    private SphereCollider beaconCollider;
    //private ArduinoCarSimulator carSimulator;
    private InputController inputController;
    public void ColliderTriggered(Collider other, GameObject current)
    {
        inputController.ReceiveApiRequest(InputController.apiEvents.WARNING);
        Debug.Log(current.tag);
    }
    private void Start()
    {
        //Get the beacon sphere collider and Arduino script
        beaconCollider = beaconObj.GetComponent<SphereCollider>();
        //carSimulator = gameObject.GetComponent<ArduinoCarSimulator>();
        //inputController = carObj.GetComponent<InputController>();
        inputController = FindObjectOfType<InputController>();
        //Debug.Log(inputController);

        //Check if objects exist in the scene
        if (beaconCollider == null )
        {
            Debug.LogError("No sphere collider added to the game object");
        }
        if (inputController == null)
        {
            Debug.LogError("No inputController script in the scene");
        }
    }
}