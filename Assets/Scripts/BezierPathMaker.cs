using UnityEngine;

public class BezierPathMaker : MonoBehaviour
{
    // Public
    [HideInInspector] public BezierPath2D path;

    // Private
    [SerializeField] [Tooltip("The Space between each Path Point")] private float spacing = 0.2f;
    [SerializeField] [Tooltip("The Level of Details the Path Points will have (higher is better)")] private float precision = 10.0f;
    [SerializeField] [Tooltip("Add a Sphere Mesh to each Path Point(slow)")] private bool generateWithSphereMesh;
    private int pointsCount, pathCount = 0;
    private Vector2[] points;
    private BezierPathMaker bezierPathMaker;
    [HideInInspector] public GameObject pathGameObject, pointGameObject;

    public void CreatePath()
    {
        path = new BezierPath2D(transform.position);
    }

    public void GeneratePathPoints()
    {
        // Initialization
        pathCount++;
        pointsCount = 0;
        bezierPathMaker = FindObjectOfType<BezierPathMaker>();
        pathGameObject = new GameObject("Path " + pathCount.ToString());

        // Calculate Evenly Spaced Points of the Bezier Path
        if (bezierPathMaker.path != null)
            points = bezierPathMaker.path.CalculateEvenlySpacedPoints(spacing, precision);

        // Create a Sphere for each Point
        foreach (Vector2 point in points)
        {
            pointsCount++;

            if (generateWithSphereMesh)
            {
                pointGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DestroyImmediate(pointGameObject.GetComponent<Collider>());
                pointGameObject.transform.localScale = Vector3.one * 0.1f;
            }
            else
                pointGameObject = new GameObject();

            pointGameObject.name = "Point " + pointsCount.ToString();
            pointGameObject.transform.SetParent(pathGameObject.transform);
            pointGameObject.transform.position = point;
        }
    }

    public void DeletePath()
    {
        DestroyImmediate(pathGameObject);
    }
}