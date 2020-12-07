using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopLight : MonoBehaviour
{
    const float STOP_TIME = 10f;

    float timeTaken = 0;

    [SerializeField]
    bool isEnabled = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isEnabled) Debug.Log("Stop Light: ON");
        else Debug.Log("Stop Light: OFF");
        timeTaken += Time.deltaTime;
        if (timeTaken >= STOP_TIME)
        {
            //Get remainder and reset timeTaken
            timeTaken -= STOP_TIME;
            
            isEnabled = !isEnabled;
        }
    }

    public bool IsEnabled()
    {
        return isEnabled;
    }
}
