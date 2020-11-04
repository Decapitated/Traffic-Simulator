using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

[ExecuteAlways]
[RequireComponent(typeof(PathCreator), typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 1;
    public float roadWidth = 1;
    public bool autoUpdate;
    public bool connectEnds = true;

    public void UpdateRoad()
    {
        GetComponent<RouteManager>().GeneratePoints();
        Vector3[] routePoints = GetComponent<RouteManager>().GetPoints();
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(routePoints);
        for(int i = 0; i < routePoints.Length; i++)
        {   
            if(i < routePoints.Length - 1)
            {
                Gizmos.DrawLine(routePoints[i], routePoints[i + 1]);
            } else if(connectEnds)
            {
                Gizmos.DrawLine(routePoints[i], routePoints[0]);
            }
        }
    }

    Mesh CreateRoadMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0;
        int trIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            Vector3 temp = Vector3.zero;
            if (i < points.Length - 1)
            {
                temp = points[i + 1] - points[i];
                forward = new Vector2(temp.x, temp.z);
            }
            if (i > 0)
            {
                temp = points[i] - points[i - 1];
                forward += new Vector2(temp.x, temp.z);
            }
            forward.Normalize();
            Vector3 left = new Vector3(forward.x, temp.y, -forward.y);

            verts[vertIndex] = points[i] - (left * roadWidth * 0.5f);
            verts[vertIndex + 1] = points[i] + (left * roadWidth * 0.5f);

            if (i < points.Length - 1)
            {
                tris[trIndex] = vertIndex;
                tris[trIndex + 1] = vertIndex + 2;
                tris[trIndex + 2] = vertIndex + 1;

                tris[trIndex + 3] = vertIndex + 1;
                tris[trIndex + 4] = vertIndex + 2;
                tris[trIndex + 5] = vertIndex + 3;
            }
            vertIndex += 2;
            trIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;

        return mesh;
    }
}
