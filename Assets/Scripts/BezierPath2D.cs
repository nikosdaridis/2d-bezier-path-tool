using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPath2D
{
    // Public 
    public bool pathIsClosed;

    // Private
    [SerializeField, HideInInspector] private List<Vector2> anchors, handles;

    private float distance;
    private Vector2 difference, deltaMove;

    // Initial Path Constructor
    public BezierPath2D(Vector2 segmentCenter)
    {
        anchors = new List<Vector2>
        {
            segmentCenter + Vector2.left,
            segmentCenter + Vector2.right
        };

        handles = new List<Vector2>
        {
            segmentCenter + (Vector2.left + Vector2.up) *0.5f,
            segmentCenter + (Vector2.right + Vector2.down) * 0.5f
        };
    }

    public int AnchorsCount()
    {
        return anchors.Count;
    }

    public int HandlesCount()
    {
        return handles.Count;
    }

    public int SegmentsCount()
    {
        if (pathIsClosed) // Closed Path + 1 Segment
            return AnchorsCount();
        else
            return AnchorsCount() - 1;
    }

    public Vector2 GetAnchorPosition(int index)
    {
        return anchors[index];
    }

    public Vector2 GetHandlesPosition(int index)
    {
        return handles[index];
    }

    public void AddSegment(Vector2 anchorPosition)
    {
        // If it's Closed, Open it, add new Segment and then Close it
        if (pathIsClosed)
        {
            ToggleCloseOpenPath();

            handles.Add(anchors[anchors.Count - 1] * 2 - handles[handles.Count - 1]);
            handles.Add((handles[handles.Count - 1] + anchorPosition) * 0.5f);
            anchors.Add(anchorPosition);

            ToggleCloseOpenPath();
        }
        else
        {
            handles.Add(anchors[anchors.Count - 1] * 2 - handles[handles.Count - 1]);
            handles.Add((handles[handles.Count - 1] + anchorPosition) * 0.5f);
            anchors.Add(anchorPosition);
        }
    }

    public Vector2[] GetAnchorsInSegment(int index)
    {
        if (index == SegmentsCount() && pathIsClosed) // Closed Path Segment Anchors
            return new Vector2[] { anchors[index - 1], anchors[0] };
        else
            return new Vector2[] { anchors[index - 1], anchors[index] };
    }

    public Vector2[] GetHandlesInSegment(int index)
    {
        if (pathIsClosed)
        {
            if(index == SegmentsCount()) // Closed Path Segment Handles
            {
                return new Vector2[] { handles[(index - 1) * 2 - 1], handles[(index - 1) * 2], handles[(index - 1) * 2 + 1], handles[0] };
            }

            if (index == 1) // First Segment
                return new Vector2[] { handles[0], handles[1], handles[2], handles[HandlesCount() - 1] };
            else if (index == SegmentsCount()) // Last Segment
                return new Vector2[] { handles[index * 2 - 3],
                handles[index * 2 - 2],
                handles[index * 2 - 1],
                handles[index * 2]};
            if (index >= 2) // Middle Segments
                return new Vector2[] { handles[index * 2 - 3],
                handles[index * 2 - 2],
                handles[index * 2 - 1],
                handles[index * 2] };
            else
                return new Vector2[] { };
        }
        else
        {
            if (index == 1) // First Segment
                return new Vector2[] { handles[0], handles[1] };
            else if (index == SegmentsCount()) // Last Segment
                return new Vector2[] { handles[index * 2 - 3],
                handles[index * 2 - 2],
                handles[index * 2 - 1] };
            if (index >= 2) // Middle Segments
                return new Vector2[] { handles[index * 2 - 3],
                handles[index * 2 - 2],
                handles[index * 2 - 1],
                handles[index * 2] };
            else
                return new Vector2[] { };
        }
    }

    public void MoveAnchorPoint(int index, Vector2 newPosition)
    {
        // Move the Anchor Point
        deltaMove = newPosition - anchors[index];
        anchors[index] = newPosition;

        // Move Handles together with Anchor
        if (pathIsClosed)
        {
            if (index == 0) // First Anchor
            {
                handles[0] += deltaMove;
                handles[HandlesCount() - 1] += deltaMove;
            }
            else if (index == AnchorsCount() - 1) // Last Anchor
            {
                handles[HandlesCount() - 2] += deltaMove;
                handles[HandlesCount() - 3] += deltaMove;
            }
            else // Middle Anchors
            {
                handles[index * 2] += deltaMove;
                handles[index * 2 - 1] += deltaMove;
            }
        }
        else
        {
            if (index == 0) // First Anchor
                handles[0] += deltaMove;
            else if (index == AnchorsCount() - 1) // Last Anchor
                handles[HandlesCount() - 1] += deltaMove;
            else // Middle Anchors
            {
                handles[index * 2] += deltaMove;
                handles[index * 2 - 1] += deltaMove;
            }
        }
    }

    public void MoveHandlesPoint(int index, Vector2 newPosition)
    {
        // Move the Handles Point
        handles[index] = newPosition;

        // Move the Pair Handle Point accordingly
        if (pathIsClosed)
        {
            if (index >= 0 && index <= HandlesCount() - 1)
            {
                if (index % 2 == 0)
                {
                    if (index == 0) // First Closed Handle
                    {
                        distance = (anchors[0] - handles[HandlesCount() - 1]).magnitude;
                        difference = (anchors[0] - newPosition).normalized;
                        handles[HandlesCount() - 1] = anchors[0] + difference * distance;
                    }
                    else
                    {
                        distance = (anchors[index / 2] - handles[index - 1]).magnitude;
                        difference = (anchors[index / 2] - newPosition).normalized;
                        handles[index - 1] = anchors[index / 2] + difference * distance;
                    }
                }
                else
                {
                    if (index == HandlesCount() - 1) // Last Closed Handle
                    {
                        distance = (anchors[0] - handles[0]).magnitude;
                        difference = (anchors[0] - newPosition).normalized;
                        handles[0] = anchors[0] + difference * distance;
                    }
                    else
                    {
                        distance = (anchors[(index + 1) / 2] - handles[index + 1]).magnitude;
                        difference = (anchors[(index + 1) / 2] - newPosition).normalized;
                        handles[index + 1] = anchors[(index + 1) / 2] + difference * distance;
                    }
                }
            }
        }
        else
        {
            if (index >= 1 && index < HandlesCount() - 1)
            {
                if (index % 2 == 0)
                {
                    distance = (anchors[index / 2] - handles[index - 1]).magnitude;
                    difference = (anchors[index / 2] - newPosition).normalized;
                    handles[index - 1] = anchors[index / 2] + difference * distance;
                }
                else
                {
                    distance = (anchors[(index + 1) / 2] - handles[index + 1]).magnitude;
                    difference = (anchors[(index + 1) / 2] - newPosition).normalized;
                    handles[index + 1] = anchors[(index + 1) / 2] + difference * distance;
                }
            }
        }
    }

    public void ToggleCloseOpenPath()
    {
        pathIsClosed = !pathIsClosed;

        if (pathIsClosed) // Add 2 Extra Handles
        {
            handles.Add(anchors[anchors.Count - 1] * 2 - handles[handles.Count - 1]);
            handles.Add(anchors[0] * 2 - handles[0]);
        }
        else // Remove 2 Extra Handles
        {
            handles.RemoveRange(HandlesCount() - 2, 2);
        }
    }

    public void RemoveAnchor(int index)
    {
        // If Only 1 Segment, return
        if (AnchorsCount() <= 2)
            return;

        if (index == 0) // First Anchor
        {
            if (pathIsClosed)
            {
                ToggleCloseOpenPath();
                anchors.RemoveAt(index);
                handles.RemoveRange(0, 2);
                ToggleCloseOpenPath();
            }
            else
            {
                anchors.RemoveAt(index);
                handles.RemoveRange(0, 2);
            }
        }
        else if (index == AnchorsCount() - 1) // Last Anchor
        {
            if (pathIsClosed)
            {
                ToggleCloseOpenPath();
                anchors.RemoveAt(index);
                handles.RemoveRange(HandlesCount() - 2, 2);
                ToggleCloseOpenPath();
            }
            else
            {
                anchors.RemoveAt(index);
                handles.RemoveRange(HandlesCount() - 2, 2);
            }
        }
        else // Middle Anchors
        {
            anchors.RemoveAt(index);
            handles.RemoveRange((index * 2) - 1, 2);
        }
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(anchors[0]);
        Vector2 previousPoint = evenlySpacedPoints[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 1; segmentIndex <= SegmentsCount(); segmentIndex++)
        {
            Vector2[] anchors = GetAnchorsInSegment(segmentIndex);
            Vector2[] handles = GetHandlesInSegment(segmentIndex);

            float controlNetLength = Vector2.Distance(anchors[0], handles[0]) + Vector2.Distance(handles[0], handles[1]) + Vector2.Distance(handles[1], anchors[1]);
            float estimatedCurveLength = Vector2.Distance(anchors[0], anchors[1]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;

                Vector2 pointOnCurve;
                if (segmentIndex == 1) // First Segment
                    pointOnCurve = CalculateCubic(anchors[0], handles[0], handles[1], anchors[1], t);
                else if (segmentIndex == SegmentsCount()) // Last Segment
                    pointOnCurve = CalculateCubic(anchors[0], handles[1], handles[2], anchors[1], t);
                else // Middle Segment
                    pointOnCurve = CalculateCubic(anchors[0], handles[1], handles[2], anchors[1], t);

                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
        }
        return evenlySpacedPoints.ToArray();
    }

    private Vector2 CalculateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    private Vector2 CalculateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = CalculateQuadratic(a, b, c, t);
        Vector2 p1 = CalculateQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }
}