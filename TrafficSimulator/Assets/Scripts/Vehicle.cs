using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;
using PathCreation;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Vehicle : MonoBehaviour
{
    [SerializeField]
    private PathCreator[] lanePathCreators = new PathCreator[0];
    // approximate real life maximum acceleration, m/s/s
    [SerializeField]
    [Range(0, 12.75f)]
    private float maxAcceleration = 5.25f;
    // approximate real life maximum breaking force, m/s/s
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
    private float viewResolution = 25f;
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

    protected PathCreator[] LanePathCreators() { return lanePathCreators; }
    protected float MaxAcceleration() { return maxAcceleration; }
    protected float MaxDeceleration() { return maxDeceleration; }
    protected float LookAheadAngle() { return lookAheadAngle; }
    protected float LookBehindAngle() { return lookBehindAngle; }
    protected float Visibility() { return visibility; }
    public int Lane() { return lane; }
    protected float DistanceTraveled() { return distanceTraveled; }
    protected float Acceleration() { return acceleration; }
    protected void Acceleration(float newAcceleration) { acceleration = Mathf.Clamp(newAcceleration, maxDeceleration, maxAcceleration); }
    protected float Speed() { return speed; }

    protected void Start()
    {
        if (!LaneNullCheck(lane))
        {
            distanceTraveled = CurrentPath().GetClosestDistanceAlongPath(transform.position);
            UpdateTransform();
        }
    }

    protected void Update()
    {
        speed += acceleration * Time.deltaTime;
        distanceTraveled += speed * Time.deltaTime;
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (!LaneNullCheck(lane))
        {
            Vector3 tempP = CurrentPath().GetPointAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.position = tempP;

            Quaternion tempR = CurrentPath().GetRotationAtDistance(distanceTraveled, EndOfPathInstruction.Loop);
            transform.rotation = tempR;
            // Rotation aligns "Up" with normals of road
            // normals are within the plane of the road
            transform.Rotate(new Vector3(0, 0, 90));
        }
    }

    protected void SwitchLane(bool isLeft)
    {
        if (isLeft && lane > 0) lane--;
        else if (!isLeft && lane < lanePathCreators.Length - 1) lane++;
        distanceTraveled = CurrentPath().GetClosestDistanceAlongPath(transform.position);
    }

    private bool LaneNullCheck(int lane)
    {
        return (lane >= lanePathCreators.Length) || (lanePathCreators[lane] == null);
    }

    private VertexPath CurrentPath()
    {
        if (LaneNullCheck(lane))
        {
            return null;
        }
        else
        {
            return lanePathCreators[lane].path;
        }
    }

    protected void HitTheBrakes()
    {
        if (speed < 0)
        {
            acceleration = maxAcceleration;
        }
        else if (speed > 0)
        {
            acceleration = maxDeceleration;
        }
        else
        {
            acceleration = 0;
        }
    }

    protected float GetVehicleDistance(Transform target)
    {
        float targetDist = CurrentPath().GetClosestDistanceAlongPath(target.position);
        float thisDist = CurrentPath().GetClosestDistanceAlongPath(transform.position);

        if (targetDist < thisDist) targetDist += CurrentPath().length;

        return targetDist - thisDist;
    }

    protected List<float> VehicleDistancesAhead(int lane)
    {
        if (LaneNullCheck(lane))
        {
            return null;
        }

        List<float> distances = new List<float>();

        for (float i = -(lookAheadAngle / 2); i <= (lookAheadAngle / 2); i += lookAheadAngle / (viewResolution + 1))
        {
            Quaternion rotation = Quaternion.AngleAxis(i, transform.up);
            Vector3 direction = rotation * transform.forward;
            RaycastHit hit;
            Vehicle hitVehicle;
            if (Physics.Raycast(transform.position, direction, out hit, visibility, LayerMask.GetMask("Car")) &&
                ((hitVehicle = hit.collider.gameObject.GetComponentInParent<Vehicle>()) != null) &&
                (hitVehicle.lane == lane)
            )
            {
                float currentDistance = lanePathCreators[lane].path.GetClosestDistanceAlongPath(transform.position);
                float hitVehicleDistance = lanePathCreators[lane].path.GetClosestDistanceAlongPath(hit.transform.position);
                if (hitVehicleDistance >= currentDistance)
                {
                    float distance = hitVehicleDistance - currentDistance;
                    distances.Add(distance);
                }
                else
                {
                    float distance = CurrentPath().length - currentDistance + hitVehicleDistance;
                    distances.Add(distance);
                }
            }
        }

        distances.Sort();
        return distances;
    }

    protected List<float> VehicleDistancesBehind(int lane)
    {
        if (LaneNullCheck(lane))
        {
            return null;
        }

        List<float> distances = new List<float>();

        for (float i = -(lookBehindAngle / 2); i <= (lookBehindAngle / 2); i += lookBehindAngle / (viewResolution + 1))
        {
            Quaternion rotation = Quaternion.AngleAxis(i + 180, transform.up);
            Vector3 direction = rotation * transform.forward;
            RaycastHit hit;
            Vehicle hitVehicle;
            if (Physics.Raycast(transform.position, direction, out hit, visibility, LayerMask.GetMask("Car")) && 
                ((hitVehicle = hit.collider.gameObject.GetComponentInParent<Vehicle>()) != null) &&
                (hitVehicle.lane == lane)
            )
            {
                float currentDistance = lanePathCreators[lane].path.GetClosestDistanceAlongPath(transform.position);
                float hitVehicleDistance = lanePathCreators[lane].path.GetClosestDistanceAlongPath(hit.transform.position);
                if (currentDistance >= hitVehicleDistance)
                {
                    float distance = currentDistance - hitVehicleDistance;
                    distances.Add(distance);
                }
                else
                {
                    float distance = CurrentPath().length - hitVehicleDistance + currentDistance;
                    distances.Add(distance);
                }
            }
        }

        distances.Sort();
        return distances;
    }

    protected bool CanTurnLeft()
    {
        BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
        Quaternion aboutFace = Quaternion.AngleAxis(180, transform.up);

        Vector3 front = boxCollider.bounds.ClosestPoint(transform.position + transform.forward);
        Vector3 back = boxCollider.bounds.ClosestPoint(transform.position + (aboutFace * transform.forward));
        Vector3 middle = transform.position;
        Vector3 left = Vector3.Cross(front - transform.position, Vector3.up);
        if (
            Physics.Raycast(front, left, laneWidth, LayerMask.GetMask("Car")) ||
            Physics.Raycast(back, left, laneWidth, LayerMask.GetMask("Car")) ||
            Physics.Raycast(middle, left, laneWidth, LayerMask.GetMask("Car"))
            )
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected bool CanTurnRight()
    {
        BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
        Quaternion aboutFace = Quaternion.AngleAxis(180, transform.up);

        Vector3 front = boxCollider.bounds.ClosestPoint(transform.position + transform.forward);
        Vector3 back = boxCollider.bounds.ClosestPoint(transform.position + (aboutFace * transform.forward));
        Vector3 middle = transform.position;
        Vector3 right = Vector3.Cross(-(front - transform.position), Vector3.up);
        if (
            Physics.Raycast(front, right, laneWidth, LayerMask.GetMask("Car")) ||
            Physics.Raycast(back, right, laneWidth, LayerMask.GetMask("Car")) ||
            Physics.Raycast(middle, right, laneWidth, LayerMask.GetMask("Car"))
            )
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void OnDrawGizmosSelected()
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

            BoxCollider boxCollider = GetComponentInChildren<BoxCollider>();
            Quaternion aboutFace = Quaternion.AngleAxis(180, transform.up);

            Vector3 front = boxCollider.bounds.ClosestPoint(transform.position + transform.forward);
            Vector3 back = boxCollider.bounds.ClosestPoint(transform.position + (aboutFace * transform.forward));
            Vector3 middle = transform.position;

            Vector3 left = Vector3.Cross(front - transform.position, Vector3.up);
            Vector3 right = Vector3.Cross(-(front - transform.position), Vector3.up);

            // Right lane vision
            Debug.DrawLine(front, front + (laneWidth * right), Color.yellow);
            Debug.DrawLine(back, back + (laneWidth * right), Color.yellow);
            Debug.DrawLine(middle, middle + (laneWidth * right), Color.yellow);

            // Left lane vision
            Debug.DrawLine(front, front + laneWidth * left, Color.yellow);
            Debug.DrawLine(back, back + laneWidth * left, Color.yellow);
            Debug.DrawLine(middle, middle + laneWidth * left, Color.yellow);
        }
    }
}
