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
    public GameObject leftFingerLocation;
    public GameObject rightFingerLocation;
    public GameObject rayLocation;
    public List<GameObject> pokeCenterLocation;
    public List<GameObject> rayCenterLocation;

    // variables for Detection
    [SerializeField] private WalkDetector walkDetector;
    [SerializeField] private HandEncumbranceDetector encumbranceDetector;


    // global variables
    private string tutorialName;
    private bool startRecord;
    private string currentEntry;
    private List<string> allEntries;
    private List<string> walkingEntries;
    private List<string> encumbranceEntries;
    private string logPath;
    private string logWalkingPath;
    private string logEncumbrancePath;
    private string sceneName;
    private bool logButtonActive;

    // Start is called before the first frame update
    void Start()
    {
        startRecord = false;

        allEntries = new List<string>();
        walkingEntries = new List<string>();
        encumbranceEntries = new List<string>();
        sceneName = SceneManager.GetActiveScene().name;

        string fname = sceneName + "_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logPath = Path.Combine(Application.persistentDataPath, fname);

        string fnameWalking = sceneName + "_Walking_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logWalkingPath = Path.Combine(Application.persistentDataPath, fnameWalking);

        string fnameEncumbrance = sceneName + "_Encumbrance_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logEncumbrancePath = Path.Combine(Application.persistentDataPath, fnameEncumbrance);

        Debug.Log("CSV will be saved to: " + Application.persistentDataPath);

        if (sceneName == "00_Tutorial")
        {
            allEntries.Add("sceneName,currentTime,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ");
            walkingEntries.Add("sceneName,SmoothedHorizontal,SmoothedVertical,DirectionStability,VerticalPattern,HorizontalPattern,AvgSpeed,IsWalking,RawMoveX,RawMoveY,RawMoveZ,currentTime");
            encumbranceEntries.Add("sceneName,CurlI,CurlM,CurlR,CurlP,AvgGripCurl,PinchI,PinchM,PinchR,PinchP,AvgPinch,WristRotX,WristRotY,WristRotZ,DeltaX,DeltaY,DeltaZ,WristStable,GripHeld,PinchHeld,Encumbrance,currentTime");
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
            LogWalking();
            LogEncumbrance();
        }
    }

    public void WriteToCSV()
    {
        File.AppendAllLines(logPath, allEntries);
        allEntries.Clear();
        Debug.Log("WriteToCSV");

        File.AppendAllLines(logWalkingPath, walkingEntries);
        walkingEntries.Clear();
        Debug.Log("WriteWalkingToCSV");

        File.AppendAllLines(logEncumbrancePath, encumbranceEntries);
        encumbranceEntries.Clear();
        Debug.Log("WriteEncumbranceToCSV");
    }

    // public void WriteWalkingToCSV()
    // {
    //     File.AppendAllLines(logWalkingPath, walkingEntries);
    //     walkingEntries.Clear();
    //     Debug.Log("WriteWalkingToCSV");
    // }
    // public void WriteEncumbranceToCSV()
    // {
    //     File.AppendAllLines(logEncumbrancePath, encumbranceEntries);
    //     encumbranceEntries.Clear();
    //     Debug.Log("WriteEncumbranceToCSV");
    // }

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

    // public void StopRecordWalkingLog()
    // {
    //     startRecord = false;
    //     Debug.Log("StopRecordWalkingLog");
    // }
    // public void StopRecordEncumbranceLog()
    // {
    //     startRecord = false;
    //     Debug.Log("StopRecordEncumbranceLog");
    // }

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
        Vector3 location = rightFingerLocation.transform.position;
        Vector3 pokeLoc = pokeCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            tutorialName + "," +
            GetTimeStamp() + "," +
            buttonScale.ToString() + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(pokeLoc) + "," +
            Vector3ToString(location) + ","
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
    
    void LogWalking()
    {
        if (walkDetector == null) return;

        Vector3 rawMove = walkDetector.RawFrameMovement;

        currentEntry = new string(
            tutorialName + "," +
            walkDetector.SmoothedHorizontal.ToString("F5") + "," +
            walkDetector.SmoothedVertical.ToString("F5") + "," +
            walkDetector.MovementDirectionStability.ToString("F2") + "," +
            walkDetector.VerticalPatternScore.ToString("F2") + "," +
            walkDetector.HorizontalPatternScore.ToString("F2") + "," +
            walkDetector.AverageHorizontalSpeed.ToString("F3") + "," +
            (walkDetector.IsWalking ? 1 : 0) + "," +
            rawMove.x.ToString("F5") + "," +  // Raw X movement
            rawMove.y.ToString("F5") + "," +  // Raw Y movement
            rawMove.z.ToString("F5") + "," +  // Raw Z movement
            GetTimeStamp());

        walkingEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogEncumbrance()
    {
        if (encumbranceDetector == null) return;

        Vector3 wristRot = encumbranceDetector.WristRotation;

        currentEntry = new string(
            tutorialName + "," +
            encumbranceDetector.CurlIndex.ToString("F1") + "," +
            encumbranceDetector.CurlMiddle.ToString("F1") + "," +
            encumbranceDetector.CurlRing.ToString("F1") + "," +
            encumbranceDetector.CurlPinky.ToString("F1") + "," +
            encumbranceDetector.AvgGripCurl.ToString("F1") + "," +
            encumbranceDetector.PinchIndex.ToString("F2") + "," +
            encumbranceDetector.PinchMiddle.ToString("F2") + "," +
            encumbranceDetector.PinchRing.ToString("F2") + "," +
            encumbranceDetector.PinchPinky.ToString("F2") + "," +
            encumbranceDetector.AvgPinch.ToString("F2") + "," +
            wristRot.x.ToString("F1") + "," + 
            wristRot.y.ToString("F1") + "," + 
            wristRot.z.ToString("F1") + "," +
            encumbranceDetector.DeltaX.ToString("F1") + "," +
            encumbranceDetector.DeltaY.ToString("F1") + "," +
            encumbranceDetector.DeltaZ.ToString("F1") + "," +
            encumbranceDetector.WristStableTime.ToString("F2") + "," +
            (encumbranceDetector.GripHeld ? 1 : 0) + "," +
            (encumbranceDetector.PinchHeld ? 1 : 0) + "," +
            (encumbranceDetector.isEncumbrance ? 1 : 0) + "," +
            GetTimeStamp());

        encumbranceEntries.Add(currentEntry);

        currentEntry = new string("");
    }
}