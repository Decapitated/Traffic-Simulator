using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleAI : Vehicle
{
    [SerializeField]
    private float speedLimit = 35f;

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Accelerate if below speed limit else cap at speed limit
        if (Speed() > speedLimit)
        {
            HitTheBrakes();
        }
        else if (Speed() < speedLimit)
        {
            Acceleration(MaxAcceleration());
        }

        // Check if car is in front
        List<float> distance = VehicleDistancesBehind(Lane());
        if (distance.Count > 0)
        {
            // Ramp breaks from Visibility() to Visibility()/2
            float brake = MaxDeceleration() * Mathf.Log(Visibility() / distance[0]);
            Acceleration(brake);
        }
    }

}
