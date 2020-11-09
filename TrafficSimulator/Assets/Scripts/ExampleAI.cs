using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class ExampleCar : MonoBehaviour
{
    public int fieldOfView = 45;
    public float criticalDistance = 5f;
    public float viewDistance = 20f;
    public float speedLimit = 20f;
    public float maxBreak = 20f;
    public bool userControl = false;

    Vehicle vehicle;

    void Awake()
    {
        vehicle = GetComponent<Vehicle>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if car is in front
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, viewDistance / 2, transform.forward, out hit, viewDistance / 2))
        {
            //find angle between my agent and the hit is it in my field of view
            float angle = Vector3.Dot(transform.forward, hit.point.normalized);
            float degree = Mathf.Acos(angle) * Mathf.Rad2Deg;

            // We can see them
            if (hit.transform.GetComponent<Vehicle>().lane == vehicle.lane && (degree >= -fieldOfView || degree <= fieldOfView))
            {
                float brake = maxBreak * Mathf.Log(viewDistance / vehicle.GetVehicleDistance(hit.transform));
                //vehicle.Brake(brake);
                if ((hit.transform.position - transform.position).magnitude > vehicle.GetVehicleDistance(hit.transform) + 3f)
                {
                    Debug.Log("Weird distance...");
                    Time.timeScale = 0;
                }
                Debug.Log($"Brake: {brake} Distance: {(hit.transform.position - transform.position).magnitude} Path Distance: {vehicle.GetVehicleDistance(hit.transform)}");
            }
        }

        // Accelerate if below speed limit else cap at speed limit
        if (vehicle.pf.speed > speedLimit)
        {
            vehicle.pf.speed = speedLimit;
        }
        else if (vehicle.pf.speed < speedLimit)
        {
            vehicle.setMaxAcceleration();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && userControl)
        {
            vehicle.SwitchLane(true);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && userControl)
        {
            vehicle.SwitchLane(false);
        }
    }

}
