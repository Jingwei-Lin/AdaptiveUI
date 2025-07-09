using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LogTutorial : MonoBehaviour
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
    public TextMeshProUGUI targetSentence;
    // public static string targetSentence { get; set; }
    public static string enteredSentence { get; set; }

    // global variables
    private string tutorialName;
    private bool startRecord;
    private string currentEntry;
    private List<string> allEntries;
    private string logPath;
    private string sceneName;
    private bool logButtonActive;

    // Start is called before the first frame update
    void Start()
    {
        startRecord = false;

        allEntries = new List<string>();
        sceneName = SceneManager.GetActiveScene().name;

        string fname = sceneName + "_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logPath = Path.Combine(Application.persistentDataPath, fname);

        if (sceneName == "00_Tutorial")
        {
            allEntries.Add("sceneName,currentTime,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,ballLocationX,ballLocationY,ballLocationZ,targetSentence,enteredSentence");
        }
    }

    void FixedUpdate()
    {
        if (startRecord && sceneName == "00_Tutorial")
        {
            if (tutorialName == "01_FittsPoke")
            {
                LogFittsPoke();
            }
            else if (tutorialName == "02_FittsRay")
            {
                LogFittsRay();
            }
            else if (tutorialName == "Line6")
            {
                LogTracingTask();
            }
            else if (tutorialName == "04_TypingTask")
            {
                if (logButtonActive)
                {
                    LogTypingTask();
                    LogButtonDeactive();
                }
            }
        }
    }

    public void WriteToCSV()
    {
        File.AppendAllLines(logPath, allEntries);
        allEntries.Clear();
        Debug.Log("WriteToCSV");
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

    public void GetName(GameObject gameObject)
    {
        tutorialName = gameObject.name.ToString();
    }

    public void StartRecordLog()
    {
        startRecord = true;
        Debug.Log("StartRecord");
    }

    public void StopRecordLog()
    {
        startRecord = false;
        Debug.Log("StopRecord");
    }

    public void LogButtonActive()
    {
        logButtonActive = true;
    }

    void LogButtonDeactive()
    {
        logButtonActive = false;
    }

    void LogFittsPoke()
    {
        // sceneName,currentTime,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,ballLocationX,ballLocationY,ballLocationZ,targetSentence,enteredSentence
        buttonScale = PointTaskTutorial.scale;
        buttonDistance = PointTaskTutorial.distance;
        targetButton = PointTaskTutorial.currentIndex;
        clickedButton = PointTaskTutorial.buttonNumber;
        // centerLocation = PointTaskTutorial.centerLocation;
        // fingerLocation = PointTaskTutorial.fingerLocation;
        Vector3 location = fingerLocation.transform.position;
        Vector3 pokeLoc = pokeCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            tutorialName + "," +
            GetTimeStamp() + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(pokeLoc) + "," +
            Vector3ToString(location) + "," +
            "," +
            "," +
            "," +
            "," +
            ""
            );

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogFittsRay()
    {
        // sceneName,currentTime,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,ballLocationX,ballLocationY,ballLocationZ,targetSentence,enteredSentence
        buttonScale = FittsRayTutorial.scale;
        buttonDistance = FittsRayTutorial.distance;
        targetButton = FittsRayTutorial.currentIndex;
        clickedButton = FittsRayTutorial.buttonNumber;
        // centerLocation = FittsRayTutorial.centerLocation;
        // fingerLocation = FittsRayTutorial.fingerLocation;
        Vector3 location = rayLocation.transform.position;
        Vector3 rayLoc = rayCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            tutorialName + "," +
            GetTimeStamp() + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(rayLoc) + "," +
            Vector3ToString(location) + "," +
            "," +
            "," +
            "," +
            "," +
            ""
            );

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogTracingTask()
    {
        // sceneName,currentTime,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,ballLocationX,ballLocationY,ballLocationZ,targetSentence,enteredSentence
        shapeName = getBallLocation.GetShapeName();
        ballLocation = getBallLocation.GetBallPosition();

        currentEntry = new string(
            shapeName + "," +
            GetTimeStamp() + "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            Vector3ToString(ballLocation)
        );

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }
    void LogTypingTask()
    {
        // sceneName,currentTime,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,ballLocationX,ballLocationY,ballLocationZ,targetSentence,enteredSentence
        currentEntry = new string(
            tutorialName + "," +
            GetTimeStamp() + "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            "," +
            targetSentence.text + "," +
            enteredSentence);

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }
}