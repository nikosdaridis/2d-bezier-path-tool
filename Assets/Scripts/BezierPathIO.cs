using System.IO;

public class BezierPathIO
{
    // Private
    private static BezierPathIO instance = new BezierPathIO();

    private FileStream fileStream;
    private bool initialized;
    private string path;
    private string[] splitResult, splitResult2, configLines;
    private char[] seperators, colorSeperators;

    // Static Explicit Constructor 
    static BezierPathIO()
    {

    }

    public static BezierPathIO Instance
    {
        get
        {
            return instance;
        }
    }

    private void Initialization()
    {
        path = Directory.GetCurrentDirectory().ToString() + "/config.cfg";
        fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        fileStream.Close();
        seperators = new char[] { '=' };
        colorSeperators = new char[] { ',' };
    }

    public void SaveConfig(string text)
    {
        // Initialization Or Doesn't Exist
        if (!initialized || !File.Exists(path))
        {
            Initialization();
            initialized = true;
        }

        // Write Save text to the file
        File.WriteAllText(path, text);
    }

    public void LoadConfig(ref bool helpEnabled, ref bool debugEnabled,
        ref bool showColorPickers, ref string[] pathColorRGB, ref string[] firstAnchorColorRGB,
        ref string[] lastAnchorColorRGB, ref string[] anchorsColorRGB,
        ref string[] handlesColorRGB, ref string[] handleLinesColorRGB)
    {
        // Initialization Or Doesn't Exist
        if (!initialized || !File.Exists(path))
        {
            Initialization();
            initialized = true;
        }
        else
        {
            // Get each Line from Config file
            configLines = File.ReadAllLines(path);

            // Split each Line and Update the references
            for (int i = 0; i <= configLines.Length - 1; i++)
            {
                splitResult = configLines[i].Split(seperators);

                switch (splitResult[0])
                {
                    case "helpEnabled":
                        if (splitResult[1] == "True")
                            helpEnabled = true;
                        else if (splitResult[1] == "False")
                            helpEnabled = false;
                        break;
                    case "debugEnabled":
                        if (splitResult[1] == "True")
                            debugEnabled = true;
                        else if (splitResult[1] == "False")
                            debugEnabled = false;
                        break;
                    case "showColorPickers":
                        if (splitResult[1] == "True")
                            showColorPickers = true;
                        else if (splitResult[1] == "False")
                            showColorPickers = false;
                        break;
                    case "pathColor":
                        splitResult2 = splitResult[1].Split(colorSeperators);
                        pathColorRGB = new string[4] { splitResult2[0],
                        splitResult2[1], splitResult2[2], splitResult2[3] };
                        break;
                    case "firstAnchorColor":
                        splitResult2 = splitResult[1].Split(colorSeperators);
                        firstAnchorColorRGB = new string[4] { splitResult2[0],
                        splitResult2[1], splitResult2[2], splitResult2[3] };
                        break;
                    case "lastAnchorColor":
                        splitResult2 = splitResult[1].Split(colorSeperators);
                        lastAnchorColorRGB = new string[4] { splitResult2[0],
                        splitResult2[1], splitResult2[2], splitResult2[3] };
                        break;
                    case "anchorsColor":
                        splitResult2 = splitResult[1].Split(colorSeperators);
                        anchorsColorRGB = new string[4] { splitResult2[0],
                        splitResult2[1], splitResult2[2], splitResult2[3] };
                        break;
                    case "handlesColor":
                        splitResult2 = splitResult[1].Split(colorSeperators);
                        handlesColorRGB = new string[4] { splitResult2[0],
                        splitResult2[1], splitResult2[2], splitResult2[3] };
                        break;
                    case "handleLinesColor":
                        splitResult2 = splitResult[1].Split(colorSeperators);
                        handleLinesColorRGB = new string[4] { splitResult2[0],
                        splitResult2[1], splitResult2[2], splitResult2[3] };
                        break;
                    default:
                        break;
                }
            }
        }
    }
}