using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleAI : Vehicle
{
    public float speedLimit = 35f;

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
            PedalToTheMetal();
        }

        // Check if car is in front
        List<float>[] carsAhead =  LookAhead();
        if (carsAhead[Lane()].Count > 0)
        {
            float brake = MaxDeceleration() * Mathf.Log(Visibility() / carsAhead[Lane()][0]);
            Acceleration(brake);
        }
    }

}
