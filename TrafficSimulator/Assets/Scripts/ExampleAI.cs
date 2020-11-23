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

        if (Speed() <= speedLimit)
        {
            Acceleration(MaxAcceleration());
        }
        else if (Speed() > speedLimit)
        {
            HitTheBrakes();
        }

        //List<float> distance = ObjectsAhead(Lane(),"Car");
        /*if ((distance.Count > 0) && (Speed() > 0))
        {
            // Ramp breaks from Visibility() to Visibility()/2
            float brake = MaxDeceleration() * Mathf.Log(Visibility() / distance[0]);
            Acceleration(brake);
        }*/
    }

}
