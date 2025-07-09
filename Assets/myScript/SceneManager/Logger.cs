using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Logger : MonoBehaviour
{
    // variables for 01_FittsPoke and 02_FittsRay
    private float buttonScale;
    private float buttonDistance;
    private int targetButton;
    private int clickedButton;
    // private Vector3 centerLocation;
    // private Vector3 fingerLocation;
    public GameObject fingerLocation;
    public GameObject rayLocation;
    public List<GameObject> pokeCenterLocation;
    public List<GameObject> rayCenterLocation;

    // variables for 03_TracingTask
    public GetBallLocation getBallLocation;
    private string shapeName;
    private Vector3 ballLocation;

    // variables for 04_TypingTask
    public static string targetSentence { get; set; }
    public static string enteredSentence { get; set; }
    public static int fixedError { get; set; }

    // global variables
    private bool startRecord;
    private string currentEntry;
    private List<string> allEntries;
    private List<string> allEntriesFull;
    private string logPath;
    private string logPathFull;
    private string iterationNumStr;
    private string sceneNumStr;
    private string sceneName;
    private bool logButtonActive;
    private string currentIndex;

    // Start is called before the first frame update
    void Start()
    {
        startRecord = false;
        logButtonActive = false;

        allEntries = new List<string>();
        allEntriesFull = new List<string>();

        iterationNumStr = (RandomSceneManager.currentIndex / 6).ToString();
        currentIndex = RandomSceneManager.currentIndex.ToString();
        sceneNumStr = SceneManager.GetActiveScene().buildIndex.ToString();
        sceneName = SceneManager.GetActiveScene().name;

        string fname = iterationNumStr + "_" + currentIndex + "_" + sceneName + "_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logPath = Path.Combine(Application.persistentDataPath, fname);

        string fnameFull = iterationNumStr + "_" + currentIndex + "_" + sceneName + "_Full_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logPathFull = Path.Combine(Application.persistentDataPath, fnameFull);

        if (sceneName == "01_FittsPoke" || sceneName == "02_FittsRay")
        {
            allEntries.Add("sceneName,sceneNum,iterationNum,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,currentTime");

            allEntriesFull.Add("sceneName,sceneNum,iterationNum,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,currentTime");
        }
        else if (sceneName == "03_TracingTask")
        {
            allEntries.Add("sceneName,sceneNum,iterationNum,shapeName,currentTime,ballLocationX,ballLocationY,ballLocationZ");
        }
        else if (sceneName == "04_TypingTask")
        {
            allEntries.Add("sceneName,sceneNum,iterationNum,targetSentence,enteredSentence,fixedError,currentTime");
        }
    }

    void FixedUpdate()
    {
        if (startRecord)
        {
            if (sceneName == "01_FittsPoke")
            {
                if (logButtonActive)
                {
                    LogFittsPoke();
                    LogButtonDeactive();
                }
                LogFittsPokeFull();
            }
            else if (sceneName == "02_FittsRay")
            {
                if (logButtonActive)
                {
                    LogFittsRay();
                    LogButtonDeactive();
                }
                LogFittsRayFull();
            }
            else if (sceneName == "03_TracingTask")
            {
                LogTracingTask();
            }
            else if (sceneName == "04_TypingTask")
            {
                if (logButtonActive)
                {
                    LogTypingTask();
                    LogButtonDeactive();
                }
            }
        }
    }

    void WriteToCSV()
    {
        File.AppendAllLines(logPath, allEntries);
        allEntries.Clear();
        Debug.Log("WriteToCSV");
    }

    void WriteToCSVFull()
    {
        File.AppendAllLines(logPathFull, allEntriesFull);
        allEntriesFull.Clear();
        Debug.Log("WriteToCSVFull");
    }

    string GetTimeStamp()
    {
        DateTime currentTime = DateTime.Now;
        return currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    string Vector3ToString(Vector3 position)
    {
        return position.x.ToString() + "," +
            position.y.ToString() + "," +
            position.z.ToString();
    }

    public void StartRecordLog()
    {
        startRecord = true;
        Debug.Log("StartRecordLog");
    }

    public void StopRecordLog()
    {
        startRecord = false;
        Debug.Log("StopRecordLog");
        WriteToCSV();
    }

    public void StopRecordLogFull()
    {
        startRecord = false;
        Debug.Log("StopRecordLogFull");
        WriteToCSVFull();
    }

    public void LogButtonActive()
    {
        logButtonActive = true;
    }

    void LogButtonDeactive()
    {
        logButtonActive = false;
    }

    void LogFittsPokeFull()
    {
        buttonScale = PointTask.scales[PointTask.randomList[PointTask.currentIteration - 1]];
        buttonDistance = PointTask.distances[PointTask.randomList[PointTask.currentIteration - 1]];
        targetButton = PointTask.currentIndex;
        clickedButton = PointTask.buttonNumber;
        // centerLocation = PointTask.centerLocation;
        // fingerLocation = PointTask.fingerLocation;
        Vector3 location = fingerLocation.transform.position;
        Vector3 pokeLoc = pokeCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            currentIndex + "," +
            iterationNumStr + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(pokeLoc) + "," +
            Vector3ToString(location) + "," +
            GetTimeStamp());

        allEntriesFull.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogFittsPoke()
    {
        buttonScale = PointTask.scales[PointTask.randomList[PointTask.currentIteration - 1]];
        buttonDistance = PointTask.distances[PointTask.randomList[PointTask.currentIteration - 1]];
        targetButton = PointTask.currentIndex;
        // clickedButton = PointTask.buttonNumber;
        // centerLocation = PointTask.centerLocation;
        // fingerLocation = PointTask.fingerLocation;
        Vector3 location = fingerLocation.transform.position;
        Vector3 pokeLoc = pokeCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            currentIndex + "," +
            iterationNumStr + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(pokeLoc) + "," +
            Vector3ToString(location) + "," +
            GetTimeStamp());

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogFittsRayFull()
    {
        buttonScale = RayTask.scales[RayTask.randomList[RayTask.currentIteration - 1]];
        buttonDistance = RayTask.distances[RayTask.randomList[RayTask.currentIteration - 1]];
        targetButton = RayTask.currentIndex;
        clickedButton = RayTask.buttonNumber;
        // centerLocation = RayTask.centerLocation;
        // fingerLocation = FittsRayTutorial.fingerLocation;
        Vector3 location = rayLocation.transform.position;
        Vector3 rayLoc = rayCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            currentIndex + "," +
            iterationNumStr + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(rayLoc) + "," +
            Vector3ToString(location) + "," +
            GetTimeStamp());

        allEntriesFull.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogFittsRay()
    {
        buttonScale = RayTask.scales[RayTask.randomList[RayTask.currentIteration - 1]];
        buttonDistance = RayTask.distances[RayTask.randomList[RayTask.currentIteration - 1]];
        targetButton = RayTask.currentIndex;
        clickedButton = RayTask.buttonNumber;
        // centerLocation = RayTask.centerLocation;
        // fingerLocation = FittsRayTutorial.fingerLocation;
        Vector3 location = rayLocation.transform.position;
        Vector3 rayLoc = rayCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            currentIndex + "," +
            iterationNumStr + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(rayLoc) + "," +
            Vector3ToString(location) + "," +
            GetTimeStamp());

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogTracingTask()
    {
        shapeName = getBallLocation.GetShapeName();
        ballLocation = getBallLocation.GetBallPosition();

        currentEntry = new string(
            sceneName + "," +
            currentIndex + "," +
            iterationNumStr + "," +
            shapeName + "," +
            GetTimeStamp() + "," +
            Vector3ToString(ballLocation));

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogTypingTask()
    {
        currentEntry = new string(
            sceneName + "," +
            currentIndex + "," +
            iterationNumStr + "," +
            targetSentence + "," +
            enteredSentence + "," +
            fixedError.ToString() + "," +
            GetTimeStamp());

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }
}