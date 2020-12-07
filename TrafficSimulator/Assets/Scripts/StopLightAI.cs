using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopLightAI : Vehicle
{
    private float speedLimit = 35f;

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (Speed() <= speedLimit)
        {
            Acceleration(MaxAcceleration());
        }
        else if (Speed() > speedLimit)
        {
            HitTheBrakes();
        }

        List<float> distance = VehicleDistancesAhead(Lane());
        if ((distance.Count > 0) && (Speed() > 0))
        {
            // Ramp breaks from Visibility() to Visibility()/2
            float brake = MaxDeceleration() * Mathf.Log(Visibility() / distance[0]);
            Acceleration(brake);
        }

        //If stop light is active ahead
        float d = ActiveStopLightDistance();
        if (d < 120)
        {
            Acceleration(-Mathf.Pow(Speed(),2)/(2*(d-10)));
            if(d <= 10 && d >= 0)
            {
                HitTheBrakes();
            }
        }
    }
}
