using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;
using PathCreation;
using System;

public class Vehicle : MonoBehaviour
{
    [SerializeField]
    private PathCreator[] lanePaths = null;
    // kg
    [SerializeField]
    private int weight = 1300;
    // average maximum m/s/s
    [SerializeField]
    [Range(0, 12.75f)]
    private float maxAcceleration = 5.25f;
    // average maximum breaking force, m/s/s
    [SerializeField]
    [Range(-37.75f, 0)]
    private float maxDeceleration = -6f;
    [SerializeField]
    private float lookAheadAngle = 170f;
    [SerializeField]
    private float lookBehindAngle = 100f;
    [SerializeField]
    private float visibility = 50f;
    [SerializeField]
    private float viewResolution = 100f;
    [SerializeField]
    private float laneWidth = 3.5f;
    [SerializeField]
    private int lane = 0;
    [SerializeField]
    public float distanceTraveled = 0;
    [SerializeField]
    private float acceleration = 0;
    [SerializeField]
    private float speed = 0;
    [SerializeField]
    private bool visualizeVision = true;


    protected void Start()
    {
        if ((lanePaths.Length > lane) && CurrentPath() != null)
        {
            distanceTraveled = CurrentPath().GetClosestDistanceAlongPath(transform.position);
            Vector3 tempP = CurrentPath().GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.position = tempP;
        }
    }

    protected void Update()
    {
        if ((lanePaths.Length > lane) && CurrentPath() != null)
        {
            speed += acceleration * Time.deltaTime;
            distanceTraveled += speed * Time.deltaTime;

            Vector3 tempP = CurrentPath().GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.position = tempP;

            Quaternion tempR = CurrentPath().GetRotationAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.rotation = tempR;
            transform.Rotate(new Vector3(0, 0, 90));
        }
    }

    protected void SwitchLane(bool isLeft)
    {
        if (isLeft && lane > 0) lane--;
        else if (!isLeft && lane < lanePaths.Length - 1) lane++;
        distanceTraveled = CurrentPath().GetClosestDistanceAlongPath(transform.position);
    }

    protected VertexPath CurrentPath()
    {
        if (lanePaths[lane] == null)
        {
            return null;
        }
        else
        {
            return lanePaths[lane].path;
        }
    }
    protected PathCreator[] LanePaths() { return lanePaths; }
    protected int Weight() { return weight; }
    protected float MaxAcceleration() { return maxAcceleration; }
    protected float MaxDeceleration() { return maxDeceleration; }
    protected float LookAheadAngle() { return lookAheadAngle; }
    protected float LookBehindAngle() { return lookBehindAngle; }
    protected float Visibility() { return visibility; }
    public int Lane() { return lane; }
    protected float DistanceTraveled() { return distanceTraveled; }
    protected float Acceleration() { return acceleration; }
    protected void Acceleration(float newAcceleration) { acceleration = Mathf.Clamp(newAcceleration, maxDeceleration, maxAcceleration); }
    protected void PedalToTheMetal() { acceleration = maxAcceleration; }
    protected void HitTheBrakes() { acceleration = maxDeceleration; }
    protected float Speed() { return speed; }

    protected float GetVehicleDistance(Transform target)
    {
        float targetDist = CurrentPath().GetClosestDistanceAlongPath(target.position);
        float thisDist = CurrentPath().GetClosestDistanceAlongPath(transform.position);

        if (targetDist < thisDist) targetDist += CurrentPath().length;

        return targetDist - thisDist;
    }

    // Return array of lanePaths information
    // Lane information is distances to visible cars
    protected List<float>[] LookAhead()
    {
        List<float>[] distances = new List<float>[lanePaths.Length];
        for (int i = 0; i < lanePaths.Length; i++)
        {
            distances[i] = new List<float>();
        }

        for (float i = -(lookAheadAngle / 2); i <= (lookAheadAngle / 2); i += lookAheadAngle / (viewResolution + 1))
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 direction = rotation * transform.forward;
            RaycastHit hit;
            Vehicle vehicle;
            if (Physics.Raycast(transform.position, direction, out hit, visibility, LayerMask.GetMask("Car")) &&
                (vehicle = hit.collider.gameObject.GetComponentInParent<Vehicle>()) != null
            )
            {
                float currentDistance = lanePaths[vehicle.lane].path.GetClosestDistanceAlongPath(transform.position);
                float vehicleDistance = lanePaths[vehicle.lane].path.GetClosestDistanceAlongPath(hit.transform.position);
                if (vehicleDistance >= currentDistance)
                {
                    float distance = vehicleDistance - currentDistance;
                    distances[vehicle.lane].Add(distance);
                }
                else
                {
                    float distance = CurrentPath().length - currentDistance + vehicleDistance;
                    distances[vehicle.lane].Add(distance);
                }
            }
        }

        for (int i = 0; i < lanePaths.Length; i++)
        {
            distances[i].Sort();
        }

        return distances;
    }

    // Return array of lanePaths information
    // Lane information is distances to visible cars
    protected List<float>[] LookBehind()
    {
        List<float>[] distances = new List<float>[lanePaths.Length];
        for (int i = 0; i < lanePaths.Length; i++)
        {
            distances[i] = new List<float>();
        }

        for (float i = -(lookBehindAngle / 2); i <= (lookBehindAngle / 2); i += lookBehindAngle / (viewResolution + 1))
        {
            Quaternion rotation = Quaternion.AngleAxis(i + 180, transform.up);
            Vector3 direction = rotation * transform.forward;
            RaycastHit hit;
            Vehicle vehicle;
            if (Physics.Raycast(transform.position, direction, out hit, visibility, LayerMask.GetMask("Car")) && 
                (vehicle = hit.collider.gameObject.GetComponentInParent<Vehicle>()) != null
            )
            {
                float currentDistance = lanePaths[vehicle.lane].path.GetClosestDistanceAlongPath(transform.position);
                float vehicleDistance = lanePaths[vehicle.lane].path.GetClosestDistanceAlongPath(hit.transform.position);
                if (currentDistance >= vehicleDistance)
                {
                    float distance = currentDistance - vehicleDistance;
                    distances[vehicle.lane].Add(distance);
                }
                else
                {
                    float distance = CurrentPath().length - vehicleDistance + currentDistance;
                    distances[vehicle.lane].Add(distance);
                }
            }
        }

        for (int i = 0; i < lanePaths.Length; i++)
        {
            distances[i].Sort();
        }

        return distances;
    }

    protected bool CanTurnLeft()
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

    protected bool CanTurnRight()
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
        if (visualizeVision)
        {
            // Front vision
            for (float i = -(lookAheadAngle / 2); i <= (lookAheadAngle / 2); i += (lookAheadAngle / viewResolution) + 1)
            {
                Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
                Vector3 ray = rotation * transform.forward;
                Debug.DrawLine(transform.position, transform.position + ray * visibility, Color.red);
            }

            // Back vision
            for (float i = -(lookBehindAngle / 2); i <= (lookBehindAngle / 2); i += (lookBehindAngle / viewResolution) + 1)
            {
                Quaternion rotation = Quaternion.AngleAxis(i + 180, transform.up);
                Vector3 ray = rotation * transform.forward;
                Debug.DrawLine(transform.position, transform.position + ray * visibility, Color.blue);
            }

            // lanePaths vision
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
}
