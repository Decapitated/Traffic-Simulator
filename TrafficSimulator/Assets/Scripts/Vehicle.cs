using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PathCreation.Examples;
using PathCreation;
using static PathCreation.VertexPath;

[RequireComponent(typeof(PathFollower))]
public class Vehicle : MonoBehaviour
{
    public int fieldOfView = 45;
    public float criticalDistance = 5f;
    public float viewDistance = 20f;
    public float speedLimit = 20f;
    public float maxBreak = 20f;

    public bool userControl = false;

    PathFollower pf;
    public PathCreator[] lanes;
    public int lane { get; private set; } = 0;

    void Awake()
    {
        pf = GetComponent<PathFollower>();
        pf.pathCreator = lanes[lane];
    }

    // Start is called before the first frame update
    void Start()
    {
        //pf = GetComponent<PathFollower>();
    }

    // Update is called once per frame
    void Update()
    {

        // Check if car is in front
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, viewDistance / 2, transform.forward, out hit, viewDistance / 2))
        {
            //Debug.Log($"Hit");
            //find angle between my agent and the hit is it in my field of view
            float angle = Vector3.Dot(transform.forward, hit.point.normalized);
            float degree = Mathf.Acos(angle) * Mathf.Rad2Deg;

            // We can see them
            if(hit.transform.GetComponent<Vehicle>().lane == lane && (degree >= -fieldOfView || degree <= fieldOfView))
            {

                float brakingDist;
                //float brake = maxBreak * ((viewDistance / hit.distance) / 100);
                float brake = maxBreak * Mathf.Log(viewDistance / GetVehicleDistance(hit.transform));
                Brake(brake); 
                Debug.Log($"Brake: {brake} Distance: {(hit.transform.position - transform.position).magnitude} Path Distance: {GetVehicleDistance(hit.transform)}");
                //if (hit.distance <= 0) Debug.Log($"Arc Distance: {GetVehicleDistance(hit.collider.transform)}");
            }
        }

        // Accelerate if below speed limit else cap at speed limit
        if (pf.speed > speedLimit)
        {
            pf.speed = speedLimit;
        }
        else if (pf.speed < speedLimit)
        {
            Accelerate(2);
        }
    
        if(Input.GetKeyDown(KeyCode.LeftArrow) && userControl)
        {
            switchLane(true);
        } 
        else if (Input.GetKeyDown(KeyCode.RightArrow) && userControl)
        {
            switchLane(false);
        }

    }

    void switchLane(bool isLeft)
    {
        if (isLeft && lane > 0) lane--;
        else if (!isLeft && lane < lanes.Length - 1) lane++;
        pf.pathCreator = lanes[lane];
        pf.distanceTravelled = pf.pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    void OnDrawGizmos()
    {
        float radius = viewDistance / 2;
        float dist = viewDistance / 2;
        Vector3 spherePos = transform.position + transform.forward * dist;

        // Draw SphereCast 
        Debug.DrawLine(transform.position, transform.position + transform.forward * dist, Color.blue);
        Gizmos.DrawWireSphere(transform.position + transform.forward * dist, radius);

        // Draw FOV
        for (int i = -fieldOfView; i <= fieldOfView; i += 5)
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 ray = rotation * transform.forward;
            Debug.DrawLine(transform.position, transform.position + ray * viewDistance, Color.red);
        }
    }

    void Accelerate(float increase)
    {
        pf.speed += increase * Time.deltaTime;
    }

    void Brake(float decrease)
    {
        if (pf.speed > 0)
        {
            pf.speed -= decrease * Time.deltaTime;
        }else if (pf.speed < 0)
        {
            pf.speed = 0;
        }
    }

    float GetVehicleDistance(Transform target)
    {
        float targetDist = pf.pathCreator.path.GetClosestDistanceAlongPath(target.position);
        float thisDist = pf.pathCreator.path.GetClosestDistanceAlongPath(transform.position);

        if (targetDist < thisDist) targetDist += pf.pathCreator.path.length;

        return targetDist - thisDist;
    }
}
