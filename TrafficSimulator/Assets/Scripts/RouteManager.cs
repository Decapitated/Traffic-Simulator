using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public int numPoints;
    [SerializeField]
    public GameObject[] routeObjects;

    Route[] routes;
    Vector3[] points;
    int totalPoints;

    void Awake()
    {
        routes = new Route[routeObjects.Length];
        totalPoints = 0;
        int pointsPerRoute = numPoints / routeObjects.Length;
        for(int i = 0; i < routeObjects.Length; i++)
        {
            routes[i] = routeObjects[i].GetComponent<Route>();

            totalPoints += routes[i].getPoints().Length;
        }
        GeneratePoints();
    }

    public void GeneratePoints()
    {
        points = new Vector3[numPoints];
        int index = 0;
        float step = 1f / numPoints;
        for (float t = 0; t <= 1 && index < numPoints; t += step, index++)
        {
            points[index] = GeneratePoint(t);
        }
    }

    Vector3 GeneratePoint(float t)
    {
        float tGlobal = Mathf.Abs(t - Mathf.Floor(t));
        int routeNum = 0;
        float aggPercent = 0;
        float percentWholeCurve = 0;

        // Find curve associated with global t value
        for (int i = 0; i < routes.Length; i++)
        {
            percentWholeCurve = routes[i].getPoints().Length / (float)totalPoints;
            aggPercent += percentWholeCurve;
            if (tGlobal <= aggPercent)
            {
                aggPercent -= percentWholeCurve;
                routeNum = i;
                break;
            }
        }

        float tLocal = (tGlobal - aggPercent) / percentWholeCurve;
        return routes[routeNum].bezier.Formula(tLocal);
    }

    public Vector3 GetPoint(int i)
    {
        return points[i];
    }

    public Vector3[] GetPoints()
    {
        return points;
    }
}
