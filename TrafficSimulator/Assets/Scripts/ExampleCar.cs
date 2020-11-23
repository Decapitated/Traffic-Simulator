using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCar : Vehicle
{
    public int fieldOfView = 45;
    public float criticalDistance = 5f;
    public float viewDistance = 20f;
    public float speedLimit = 20f;
    public float maxBreak = 20f;
    public bool userControl = false;

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Check if car is in front
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, viewDistance / 2, transform.forward, out hit, viewDistance / 2))
        {
            //find angle between my agent and the hit is it in my field of view
            float angle = Vector3.Dot(transform.forward, hit.point.normalized);
            float degree = Mathf.Acos(angle) * Mathf.Rad2Deg;

            // We can see them
            bool val = hit.transform.GetComponent<Vehicle>().Lane() == Lane();

            if (hit.transform.GetComponent<Vehicle>().Lane() == Lane() && (degree >= -fieldOfView || degree <= fieldOfView))
            {
                float brake = MaxDeceleration() * Mathf.Log(viewDistance / GetVehicleDistance(hit.transform));
                Acceleration(brake);
                if ((hit.transform.position - transform.position).magnitude > GetVehicleDistance(hit.transform) + 3f)
                {
                    Debug.Log("Weird distance...");
                    Time.timeScale = 0;
                }
                Debug.Log($"Brake: {brake} Distance: {(hit.transform.position - transform.position).magnitude} Path Distance: {GetVehicleDistance(hit.transform)}");
            }
        }

        // Accelerate if below speed limit else cap at speed limit
        if (Speed() > speedLimit)
        {
            HitTheBrakes();
        }
        else if (Speed() < speedLimit)
        {
            Acceleration(MaxAcceleration());
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && userControl)
        {
            SwitchLane(true);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && userControl)
        {
            SwitchLane(false);
        }
    }

    void OnDrawGizmos()
    {
        float radius = viewDistance / 2;

        // Draw SphereCast 
        Debug.DrawLine(transform.position, transform.position + transform.forward * radius, Color.blue);
        Gizmos.DrawWireSphere(transform.position + transform.forward * radius, radius);

        // Draw FOV
        for (int i = -fieldOfView; i <= fieldOfView; i += 5)
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 ray = rotation * transform.forward;
            Debug.DrawLine(transform.position, transform.position + ray * viewDistance, Color.red);
        }
    }

}