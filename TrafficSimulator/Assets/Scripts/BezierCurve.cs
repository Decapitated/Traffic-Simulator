using UnityEngine;

public class BezierCurve
{
    private Vector3[] points;

    public BezierCurve(Vector3[] points)
    {
        this.points = points;
    }
    
    public Vector3 Formula(float t)
    {
        Vector3 temp = new Vector3();
        for(int i = 0; i < points.Length; i++)
        {
            temp += BinomialCoefficient(i) * Mathf.Pow(1 - t, points.Length - i - 1) * Mathf.Pow(t, i) * points[i];
        }

        return temp;
    }

    private int BinomialCoefficient(int point)
    {
        return Factorial(points.Length - 1) / ( Factorial(point) * Factorial(points.Length - point - 1) );
    }

    private int Factorial(int n)
    {
        int temp = n;
        for(int i = n-1; i > 0; i--)
        {
            temp *= i;
        }
        if (temp < 1) return 1;
        return temp;
    }
}
