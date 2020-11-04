using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Route : MonoBehaviour
{
    public GameObject point;
    [Min(2)]
    public int numPointForGizmo = 25;
    public bool enableLines = true;

    public bool enablePoints = true;
    public float pointSize = 0.05f;

    public bool enableControlPoints = true;
    public float controlPointSize = 0.25f;

    public BezierCurve bezier { get; private set; }

    Vector3[] points;

    void Awake()
    {
        updatePoints();
    }

    void OnEnable()
    {
        if (!Application.isEditor)
        {
            Destroy(this);
        }
        SceneView.duringSceneGui += OnScene;
    }

    void OnScene(SceneView scene)
    {
        Event e = Event.current;

        if(e.type == EventType.MouseUp && e.button == 1 && e.control)
        {
            e.Use();
            Debug.Log("Hello");
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 50);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Hit");
                GameObject go = Instantiate(point, transform);
                go.name = "point" + points.Length;
                Vector3 temp = hit.point;
                temp.y = transform.position.y;
                go.transform.position = temp;

                updatePoints();
            }
        }
    }

    private void OnDrawGizmos()
    {
        updatePoints();
        if (bezier != null && points != null & points.Length > 1)
        {
            if (enableControlPoints)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    Gizmos.DrawSphere(points[i], controlPointSize);
                }
            }
            float step = 1f / numPointForGizmo;
            Vector3 prev = bezier.Formula(0);
            for (float t = 0 + step; t <= 1; t += step)
            {
                Vector3 current = bezier.Formula(t);
                if(enableLines) Gizmos.DrawLine(prev, current);
                if (enablePoints) Gizmos.DrawSphere(current, pointSize);
                prev = current;
            }
        }
    }

    void updatePoints()
    {
        Vector3[] childrenPos = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            childrenPos[i] = transform.GetChild(i).position;
        }
        points = childrenPos;
        bezier = new BezierCurve(points);
    }

    public Vector3[] getPoints()
    {
        return points;
    }

    public Vector3 getPoint(int i)
    {
        return points[i];
    }
}
