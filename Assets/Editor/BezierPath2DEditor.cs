using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierPathMaker))]
public class BezierPath2DEditor : Editor
{
    // Private
    private BezierPathMaker pathMaker;
    private BezierPath2D path;
    private SerializedProperty spacing, precision, generateWithSphereMesh;
    private bool helpEnabled, debugEnabled, showColorPickers;
    private string pathCloseOpenButtonText;

    private string[] pathColorRGB, firstAnchorColorRGB, lastAnchorColorRGB,
        anchorsColorRGB, handlesColorRGB, handleLinesColorRGB;
    private Color pathColor, firstAnchorColor, lastAnchorColor,
        anchorsColor, handlesColor, handleLinesColor;

    private Vector2 mousePosition, newPosition;
    private Vector2[] anchorsInSegment, anchorsInSegment2, handlesInSegment, handlesInSegment2;
    private Event guiEvent;
    private float minDistanceToAnchor, distance;
    private int closestAnchorIndex;

    // Create a starting Segment if it doesn't exist
    private void OnEnable()
    {

        // Initialization
        helpEnabled = true;
        debugEnabled = false;
        showColorPickers = false;
        pathColor = Color.yellow;
        firstAnchorColor = Color.cyan;
        lastAnchorColor = Color.blue;
        anchorsColor = Color.red;
        handlesColor = Color.black;
        handleLinesColor = Color.gray;

        pathMaker = (BezierPathMaker)target;

        // Create Initial Bezier Path
        if (pathMaker.path == null)
            pathMaker.CreatePath();

        path = pathMaker.path;

        // Load Tool Config
        BezierPathIO.Instance.LoadConfig(ref helpEnabled, ref debugEnabled,
            ref showColorPickers, ref pathColorRGB, ref firstAnchorColorRGB,
            ref lastAnchorColorRGB, ref anchorsColorRGB, ref handlesColorRGB,
            ref handleLinesColorRGB);

        // Load Tool Colors
        if (pathColorRGB != null)
        {
            pathColor = new Color(float.Parse(pathColorRGB[0]), float.Parse(pathColorRGB[1]),
                float.Parse(pathColorRGB[2]), float.Parse(pathColorRGB[3]));

            firstAnchorColor = new Color(float.Parse(firstAnchorColorRGB[0]), float.Parse(firstAnchorColorRGB[1]),
                float.Parse(firstAnchorColorRGB[2]), float.Parse(firstAnchorColorRGB[3]));

            lastAnchorColor = new Color(float.Parse(lastAnchorColorRGB[0]), float.Parse(lastAnchorColorRGB[1]),
                float.Parse(lastAnchorColorRGB[2]), float.Parse(lastAnchorColorRGB[3]));

            anchorsColor = new Color(float.Parse(anchorsColorRGB[0]), float.Parse(anchorsColorRGB[1]),
                float.Parse(anchorsColorRGB[2]), float.Parse(anchorsColorRGB[3]));

            handlesColor = new Color(float.Parse(handlesColorRGB[0]), float.Parse(handlesColorRGB[1]),
                float.Parse(handlesColorRGB[2]), float.Parse(handlesColorRGB[3]));

            handleLinesColor = new Color(float.Parse(handleLinesColorRGB[0]), float.Parse(handleLinesColorRGB[1]),
                float.Parse(handleLinesColorRGB[2]), float.Parse(handleLinesColorRGB[3]));
        }
    }

    private void OnDisable()
    {
        // Save Tool Config
        BezierPathIO.Instance.SaveConfig(
            "helpEnabled=" + helpEnabled + "\n" +
            "debugEnabled=" + debugEnabled + "\n" +
            "showColorPickers=" + showColorPickers + "\n" +
            "pathColor=" + pathColor.r + ',' + pathColor.g + ',' + pathColor.b + ',' + pathColor.a + "\n" +
            "firstAnchorColor=" + firstAnchorColor.r + ',' + firstAnchorColor.g + ',' + firstAnchorColor.b + ',' + firstAnchorColor.a + "\n" +
            "lastAnchorColor=" + lastAnchorColor.r + ',' + lastAnchorColor.g + ',' + lastAnchorColor.b + ',' + lastAnchorColor.a + "\n" +
            "anchorsColor=" + anchorsColor.r + ',' + anchorsColor.g + ',' + anchorsColor.b + ',' + anchorsColor.a + "\n" +
            "handlesColor=" + handlesColor.r + ',' + handlesColor.g + ',' + handlesColor.b + ',' + handlesColor.a + "\n" +
            "handleLinesColor=" + handleLinesColor.r + ',' + handleLinesColor.g + ',' + handleLinesColor.b + ',' + handleLinesColor.a
            );
    }

    public override void OnInspectorGUI()
    {
        // Serialized Property Assign
        SerializedProperty spacing = serializedObject.FindProperty("spacing");
        SerializedProperty precision = serializedObject.FindProperty("precision");
        SerializedProperty generateWithSphereMesh = serializedObject.FindProperty("generateWithSphereMesh");

        // Fetch Serialized Properties Values
        serializedObject.Update();

        // Tool Default Color
        GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);

        // Create New Bezier Path Button
        if (GUILayout.Button("Create New Bezier Path"))
        {
            // Show Window to Verify User wants to Create New Bezier Path
            if (EditorUtility.DisplayDialog("Create New Bezier Path?",
                    "Are you sure you want to Create New Bezier Path?",
                    "Yes",
                    "No"))

            // Record Undo and Create New Bezier Path
            {
                Undo.RecordObject(pathMaker, "Created New Path");
                pathMaker.CreatePath();
                path = pathMaker.path;
            }
        }

        // Open/Close Bezier Path Button Color
        if (path.pathIsClosed)
            GUI.backgroundColor = new Color(0.45f, 0.45f, 0.45f, 1.0f);
        else
            GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);

        // Open/Close Bezier Path Text
        if (path.pathIsClosed)
            pathCloseOpenButtonText = "Open Path";
        else
            pathCloseOpenButtonText = "Close Path";

        // Open/Close Bezier Path Button
        if (GUILayout.Button(pathCloseOpenButtonText))
        {
            Undo.RecordObject(pathMaker, "Opened/Closed Path");
            path.ToggleCloseOpenPath();
        }

        // Tool Default Color
        GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);

        // Colors Foldout
        showColorPickers = EditorGUILayout.Foldout(showColorPickers, "Colors");

        // Color Pickers
        if (showColorPickers)
        {
            pathColor = EditorGUILayout.ColorField("Path", pathColor);
            firstAnchorColor = EditorGUILayout.ColorField("First Anchor", firstAnchorColor);
            lastAnchorColor = EditorGUILayout.ColorField("Last Anchor", lastAnchorColor);
            anchorsColor = EditorGUILayout.ColorField("Anchors", anchorsColor);
            handlesColor = EditorGUILayout.ColorField("Handles", handlesColor);
            handleLinesColor = EditorGUILayout.ColorField("Handle Lines", handleLinesColor);

            if (GUILayout.Button("Reset to Default Colors"))
            {
                // Show Window to Verify User wants to Create New Bezier Path
                if (EditorUtility.DisplayDialog("Reset to Default Colors?",
                        "Are you sure you want to Reset to Default Colors?",
                        "Yes",
                        "No"))

                // Reset to Default Colors
                {
                    pathColor = Color.yellow;
                    firstAnchorColor = Color.cyan;
                    lastAnchorColor = Color.blue;
                    anchorsColor = Color.red;
                    handlesColor = Color.black;
                    handleLinesColor = Color.gray;
                }
            }
        }

        // Spacing Path Points Generator Slider
        EditorGUILayout.Slider(spacing, 0.05f, 5.0f, "Spacing");

        // Precision Path Points Generator Slider
        EditorGUILayout.Slider(precision, 0.1f, 100.0f, "Precision");

        // Sphere Mesh Toggle
        generateWithSphereMesh.boolValue = EditorGUILayout.Toggle("Add Spheres", generateWithSphereMesh.boolValue);

        // Red Color
        GUI.backgroundColor = Color.red;

        // Save Path Points as Prefab
        if (GUILayout.Button("Generate Path Points and Save it as Prefab"))
        {
            pathMaker.GeneratePathPoints();

            // Set the path of the Path Prefab
            string prefabPath = "Assets/" + pathMaker.pathGameObject.name + ".prefab";

            // Check if Prefab Already Exists at the Path
            if (AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)))
            {
                // Ask User to Overwrite Existing Prefab
                if (EditorUtility.DisplayDialog("The Prefab Already Exists!",
                    "The Prefab already exists. Do you want to overwrite it?",
                    "Yes, overwrite",
                    "No"))

                // Create/Overwrite the Prefab
                {
                    Debug.Log(pathMaker.pathGameObject.name + " overwrited as Prefab in Assets!");
                    CreateNewPrefab(pathMaker.pathGameObject, prefabPath);
                }
            }
            // Create New Prefab
            else
            {
                Debug.Log(pathMaker.pathGameObject.name + " saved as Prefab in Assets!");
                CreateNewPrefab(pathMaker.pathGameObject, prefabPath);
            }

            // Delete Path after Saving it as Prefab
            pathMaker.DeletePath();
        }

        // If anything Changed, Repaint Everything
        if (GUI.changed)
            SceneView.RepaintAll();

        // Apply Changes to Serialized Properties
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        Input();
        DrawPathBeziersAndHandleLines();
        DrawAndMoveAnchors();
        DrawAndMoveHandles();
        HelpWindow();
    }

    // Help Window in Scene View
    void HelpWindow()
    {
        if (helpEnabled)
        {
            Handles.BeginGUI();

            GUI.Label(new Rect(5, 5, 250, 20), "Help : F1", GUI.skin.box);
            GUI.Label(new Rect(5, 25, 250, 150),
                "Add Segment : CTRL + LClick\n\n" +
                "Remove Anchor : RClick At Anchor\n\n" +
                "Open/Close Path : C\n\n" +
                "Debug Info : F2"
                , GUI.skin.box);

            Handles.EndGUI();
        }
    }

    // GUI Input
    void Input()
    {
        // Get Current GUI Event
        guiEvent = Event.current;

        // Input for Adding new Segments to the Path (CTRL + LClick)
        if (guiEvent.control && guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            // Get Mouse Position in Scene View
            mousePosition = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            // Record Undo and Add New Segment
            Undo.RecordObject(pathMaker, "Added segment");
            path.AddSegment(mousePosition);
        }

        // Show/Hide Help Window
        if (guiEvent.type == EventType.KeyDown && guiEvent.Equals(Event.KeyboardEvent("F1")))
            helpEnabled = !helpEnabled;

        // Show/Hide Debug Info
        if (guiEvent.type == EventType.KeyDown && guiEvent.Equals(Event.KeyboardEvent("F2")))
            debugEnabled = !debugEnabled;

        // Open/Close The Bezier Path
        if (guiEvent.type == EventType.KeyDown && guiEvent.Equals(Event.KeyboardEvent("C")))
        {
            Undo.RecordObject(pathMaker, "Opened/Closed Path");
            path.ToggleCloseOpenPath();
        }

        // Remove Anchor Point
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            // Get Mouse Position in Scene View
            mousePosition = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            // Calculate Min Distance Required from Mouse Position to Anchor
            minDistanceToAnchor = 0.15f * HandleUtility.GetHandleSize(path.GetAnchorPosition(0)) * 0.8f;
            closestAnchorIndex = -int.MaxValue;

            for (int i = 0; i <= path.AnchorsCount() - 1; i++)
            {
                distance = Vector2.Distance(mousePosition, path.GetAnchorPosition(i));

                if (distance <= minDistanceToAnchor)
                {
                    minDistanceToAnchor = distance;
                    closestAnchorIndex = i;
                }
            }

            // Remove Segment Of the Anchor
            if (closestAnchorIndex != -int.MaxValue)
            {
                Undo.RecordObject(pathMaker, "Delete segment");
                path.RemoveAnchor(closestAnchorIndex);
            }
        }
    }

    // Draw the Bezier of each Segment of the Path and Handle Lines
    void DrawPathBeziersAndHandleLines()
    {
        Handles.color = handleLinesColor;

        for (int i = 1; i <= path.SegmentsCount(); i++)
        {
            // Get Anchors and Handles in current Segment
            anchorsInSegment = path.GetAnchorsInSegment(i);
            handlesInSegment = path.GetHandlesInSegment(i);

            if (i == 1) // First Segment
            {
                Handles.DrawBezier(anchorsInSegment[0], anchorsInSegment[1],
                handlesInSegment[0], handlesInSegment[1], pathColor, null, 3.0f);

                Handles.DrawLine(anchorsInSegment[0], handlesInSegment[0]);
                Handles.DrawLine(anchorsInSegment[1], handlesInSegment[1]);
            }
            else if (i == path.SegmentsCount()) // Last Segment
            {
                Handles.DrawBezier(anchorsInSegment[0], anchorsInSegment[1],
                handlesInSegment[1], handlesInSegment[2], pathColor, null, 3.0f);

                Handles.DrawLine(anchorsInSegment[0], handlesInSegment[0]);
                Handles.DrawLine(anchorsInSegment[0], handlesInSegment[1]);
                Handles.DrawLine(anchorsInSegment[1], handlesInSegment[2]);
            }
            else // Middle Segments
            {
                Handles.DrawBezier(anchorsInSegment[0], anchorsInSegment[1],
                handlesInSegment[1], handlesInSegment[2], pathColor, null, 3.0f);

                Handles.DrawLine(anchorsInSegment[0], handlesInSegment[0]);
                Handles.DrawLine(anchorsInSegment[0], handlesInSegment[1]);
                Handles.DrawLine(anchorsInSegment[1], handlesInSegment[2]);
                Handles.DrawLine(anchorsInSegment[1], handlesInSegment[3]);
            }
        }
    }

    // Draw the Anchor Handles and Update to new Positions
    void DrawAndMoveAnchors()
    {
        for (int i = 0; i < path.AnchorsCount(); i++)
        {
            // First, Middle, Last Anchor Handles Colors
            if (i == 0)
                Handles.color = firstAnchorColor;
            else if (i == path.AnchorsCount() - 1)
                Handles.color = lastAnchorColor;
            else
                Handles.color = anchorsColor;

            // Draw a Handle for each Anchor and save the new Position
            newPosition = Handles.FreeMoveHandle(
                path.GetAnchorPosition(i),
                Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(path.GetAnchorPosition(i)),
                Vector2.zero,
                Handles.CylinderHandleCap);

            // Debug Info, Name and Position
            if (debugEnabled)
                Handles.Label(newPosition - new Vector2(-0.15f, -0.15f),
                    "A " + i.ToString() + " " + newPosition.ToString("F2"));

            // Update the new Postion and Record Undo
            if (path.GetAnchorPosition(i) != newPosition)
            {
                Undo.RecordObject(pathMaker, "Moved Anchor");
                path.MoveAnchorPoint(i, newPosition);
            }
        }
    }

    // Draw the Handle Handles and Update to new Postions
    void DrawAndMoveHandles()
    {
        Handles.color = handlesColor;

        for (int i = 0; i < path.HandlesCount(); i++)
        {
            // Draw a Handle for each Handle and save the new Position
            newPosition = Handles.FreeMoveHandle(
                path.GetHandlesPosition(i),
                Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(path.GetHandlesPosition(i)),
                Vector2.zero,
                Handles.SphereHandleCap);

            // Debug Info, Name and Position
            if (debugEnabled)
                Handles.Label(newPosition - new Vector2(-0.08f, -0.08f),
                    "H " + i.ToString() + " " + newPosition.ToString("F2"));

            // Update the new Postion and Record Undo
            if (path.GetHandlesPosition(i) != newPosition)
            {
                Undo.RecordObject(pathMaker, "Moved Handle");
                path.MoveHandlesPoint(i, newPosition);
            }
        }
    }

    // Create New Prefab
    private void CreateNewPrefab(GameObject obj, string localPath)
    {
        //Create a new Prefab at the path given
        Object prefab = PrefabUtility.CreatePrefab(localPath, obj);
        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
}