using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;
using PathCreation;
using System;

[RequireComponent(typeof(PathFollower))]
public class Vehicle : MonoBehaviour
{
    public PathCreator[] lanes;
    public int weightKg = 1300;
    // average maximum m/s/s
    public float maxAcceleration = 5.25f;
    // average maximum breaking force, m/s/s
    public float maxDeceleration = 6f;
    public float lookAheadAngle = 170f;
    public float lookBehindAngle = 100f;
    public float viewDistance = 30f;
    public float viewResolution = 100f; 
    public float laneWidth = 3.5f;
    public int lane = 0;
    public float distanceTraveled { get; private set; }

    private float _acceleration;
    public float acceleration {
        get { return _acceleration; }
        set { _acceleration = Mathf.Clamp(value, maxDeceleration, maxAcceleration); }
    }
    [SerializeField]
    public float speed { get; private set; }


    void Start()
    {
        if ((lanes.Length > lane) && lanes[lane] != null)
        {
            distanceTraveled = lanes[lane].path.GetClosestDistanceAlongPath(transform.position);
            Vector3 tempP = lanes[lane].path.GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.position = tempP;
        }
    }

    void Update()
    {
        if ((lanes.Length > lane) && lanes[lane] != null)
        {
            speed += acceleration * Time.deltaTime;
            distanceTraveled += speed * Time.deltaTime;

            Vector3 tempP = lanes[lane].path.GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.position = tempP;

            Quaternion tempR = lanes[lane].path.GetRotationAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.rotation = tempR;
            transform.Rotate(new Vector3(0, 0, 90));
        }
    }

    public void SwitchLane(bool isLeft)
    {
        if (isLeft && lane > 0) lane--;
        else if (!isLeft && lane < lanes.Length - 1) lane++;
        distanceTraveled = lanes[lane].path.GetClosestDistanceAlongPath(transform.position);
    }

    public void setMaxAcceleration()
    {
        acceleration = maxAcceleration;
    }

    public void setMaxDeceleration()
    {
        acceleration = maxDeceleration;
    }

    public float GetVehicleDistance(Transform target)
    {
        float targetDist = lanes[lane].path.GetClosestDistanceAlongPath(target.position);
        float thisDist = lanes[lane].path.GetClosestDistanceAlongPath(transform.position);

        if (targetDist < thisDist) targetDist += lanes[lane].path.length;

        return targetDist - thisDist;
    }

    // Return array of lanes information
    // Lane information is distances to visible cars
    public List<float>[] LookAhead()
    {
        List<float>[] distances = new List<float>[lanes.Length];

        for (float i = -(lookAheadAngle / 2); i <= (lookAheadAngle / 2); i += (lookAheadAngle / viewResolution) + 1)
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 direction = rotation * transform.forward;
            RaycastHit hit;
            Vehicle vehicle;
            if (Physics.Raycast(transform.position, direction, out hit, viewDistance, LayerMask.GetMask("Car")) &&
                (vehicle = hit.collider.gameObject.GetComponent<Vehicle>()) != null
            )
            {
                float currentDistance = lanes[vehicle.lane].path.GetClosestDistanceAlongPath(transform.position);
                float vehicleDistance = lanes[vehicle.lane].path.GetClosestDistanceAlongPath(hit.transform.position);
                if (vehicleDistance >= currentDistance)
                {
                    float distance = vehicleDistance - currentDistance;
                    distances[vehicle.lane].Add(distance);
                }
                else
                {
                    float distance = lanes[lane].path.length - currentDistance + vehicleDistance;
                    distances[vehicle.lane].Add(distance);
                }
            }
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            distances[i].Sort();
        }

        return distances;
    }

    // Return array of lanes information
    // Lane information is distances to visible cars
    public List<float>[] LookBehind()
    {
        List<float>[] distances = new List<float>[lanes.Length];

        for (float i = -(lookBehindAngle / 2); i <= (lookBehindAngle / 2); i += (lookBehindAngle / viewResolution) + 1)
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 direction = rotation * transform.forward;
            RaycastHit hit;
            Vehicle vehicle;
            if (Physics.Raycast(transform.position, direction, out hit, viewDistance, LayerMask.GetMask("Car")) &&
                (vehicle = hit.collider.gameObject.GetComponent<Vehicle>()) != null
            )
            {
                float currentDistance = lanes[vehicle.lane].path.GetClosestDistanceAlongPath(transform.position);
                float vehicleDistance = lanes[vehicle.lane].path.GetClosestDistanceAlongPath(hit.transform.position);
                if (currentDistance >= vehicleDistance)
                {
                    float distance = currentDistance - vehicleDistance;
                    distances[vehicle.lane].Add(distance);
                }
                else
                {
                    float distance = lanes[lane].path.length - vehicleDistance + currentDistance;
                    distances[vehicle.lane].Add(distance);
                }
            }
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            distances[i].Sort();
        }

        return distances;
    }

    bool CanTurnLeft()
    {
        BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("Cannot Lane check, box collider not found");
            return false;
        }
        else
        {
            Vector3 front = boxCollider.bounds.ClosestPoint(transform.position + transform.forward);
            Quaternion aboutFace = Quaternion.AngleAxis(180, transform.up);
            Vector3 back = boxCollider.bounds.ClosestPoint(transform.position + (aboutFace * transform.forward));
            Vector3 direction = Vector3.Cross(front - transform.position, Vector3.up);
            if (
                Physics.Raycast(front, direction, laneWidth, LayerMask.GetMask("Car")) ||
                Physics.Raycast(back, direction, laneWidth, LayerMask.GetMask("Car")) ||
                Physics.Raycast(transform.position, direction, laneWidth, LayerMask.GetMask("Car"))
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    bool CanTurnRight()
    {
        BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("Cannot Lane check, box collider not found");
            return false;
        }
        else
        {
            Vector3 front = boxCollider.bounds.ClosestPoint(transform.position + transform.forward);
            Quaternion aboutFace = Quaternion.AngleAxis(180, transform.up);
            Vector3 back = boxCollider.bounds.ClosestPoint(transform.position + (aboutFace * transform.forward));
            Vector3 direction = Vector3.Cross(-(front - transform.position), Vector3.up);
            if (
                Physics.Raycast(front, direction, laneWidth, LayerMask.GetMask("Car")) ||
                Physics.Raycast(back, direction, laneWidth, LayerMask.GetMask("Car")) ||
                Physics.Raycast(transform.position, direction, laneWidth, LayerMask.GetMask("Car"))
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Front vision
        for (float i = -(lookAheadAngle / 2); i <= (lookAheadAngle / 2); i += (lookAheadAngle / viewResolution) + 1)
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 ray = rotation * transform.forward;
            Debug.DrawLine(transform.position, transform.position + ray * viewDistance, Color.red);
        }

        // Back vision
        for (float i = -(lookBehindAngle / 2); i <= (lookBehindAngle / 2); i += (lookBehindAngle / viewResolution) + 1)
        {
            Quaternion rotation = Quaternion.AngleAxis(i + 180, transform.up);
            Vector3 ray = rotation * transform.forward;
            Debug.DrawLine(transform.position, transform.position + ray * viewDistance, Color.blue);
        }

        // Lanes vision
        BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
        if (boxCollider != null)
        {
            Vector3 front = boxCollider.bounds.ClosestPoint(transform.position + transform.forward);
            Quaternion aboutFace = Quaternion.AngleAxis(180, transform.up);
            Vector3 back = boxCollider.bounds.ClosestPoint(transform.position + (aboutFace * transform.forward));
            Vector3 leftDirection = Vector3.Cross(front - transform.position, Vector3.up);
            Vector3 rightDirection = Vector3.Cross(-(front - transform.position), Vector3.up);

            // Right lane
            Debug.DrawLine(front, front + laneWidth * rightDirection, Color.yellow);
            Debug.DrawLine(back, back + laneWidth * rightDirection, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + laneWidth * rightDirection, Color.yellow);

            // Left lane
            Debug.DrawLine(front, front + laneWidth * leftDirection, Color.yellow);
            Debug.DrawLine(back, back + laneWidth * leftDirection, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + laneWidth * leftDirection, Color.yellow);
        }

    }
}
