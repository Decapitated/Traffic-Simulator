using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopLight : MonoBehaviour
{
    float STOP_TIME = 10f;

    float timeTaken = 0;

    bool isEnabled = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeTaken += Time.deltaTime / STOP_TIME;
        if(timeTaken >= 1)
        {
            timeTaken = 0;
            isEnabled = !isEnabled;
        }
    }

    public bool IsEnabled()
    {
        return isEnabled;
    }
}
