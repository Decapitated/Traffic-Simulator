using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;
using PathCreation;

[RequireComponent(typeof(PathFollower))]
public class Vehicle : MonoBehaviour
{
    public PathCreator[] lanes;

    public int lane { get; private set; } = 0;
    public PathFollower pf { get; private set; }

    void Awake()
    {
        pf = GetComponent<PathFollower>();
        pf.pathCreator = lanes[lane];
    }

    public void switchLane(bool isLeft)
    {
        if (isLeft && lane > 0) lane--;
        else if (!isLeft && lane < lanes.Length - 1) lane++;
        pf.pathCreator = lanes[lane];
        pf.distanceTravelled = pf.pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    public void Accelerate(float increase)
    {
        pf.speed += increase * Time.deltaTime;
    }

    public void Brake(float decrease)
    {
        if (pf.speed > 0)
        {
            pf.speed -= decrease * Time.deltaTime;
        }else if (pf.speed < 0)
        {
            pf.speed = 0;
        }
    }

    public float GetVehicleDistance(Transform target)
    {
        float targetDist = pf.pathCreator.path.GetClosestDistanceAlongPath(target.position);
        float thisDist = pf.pathCreator.path.GetClosestDistanceAlongPath(transform.position);

        if (targetDist < thisDist) targetDist += pf.pathCreator.path.length;

        return targetDist - thisDist;
    }
}
