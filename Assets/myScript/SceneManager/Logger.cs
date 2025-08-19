using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Drawing;
using UnityEngine.UIElements;

public class Logger : MonoBehaviour
{
    // variables for FittsPoke and FittsRay
    private float buttonScale;
    private float buttonDistance;
    private int targetButton;
    private int clickedButton;
    public GameObject leftFingerLocation;
    public GameObject rightFingerLocation;
    public GameObject rayLocation;
    public List<GameObject> pokeCenterLocation;
    public List<GameObject> rayCenterLocation;

    // variables for Detection
    [SerializeField] private WalkDetector walkDetector;
    [SerializeField] private HandEncumbranceDetector encumbranceDetector;

    // global variables
    private bool startRecord;
    private string currentEntry;
    private List<string> allEntries;
    private List<string> allEntriesFull;
    private List<string> walkingEntries;
    private List<string> encumbranceEntries;
    private string logPath;
    private string logPathFull;
    private string logWalkingPath;
    private string logEncumbrancePath;
    private string iterationNumStr;
    private string sceneNumStr;
    private string sceneName;
    private bool logButtonActive;

    // Start is called before the first frame update
    void Start()
    {
        startRecord = false;
        logButtonActive = false;

        allEntries = new List<string>();
        allEntriesFull = new List<string>();
        walkingEntries = new List<string>();
        encumbranceEntries = new List<string>();

        sceneName = SceneManager.GetActiveScene().name;
        sceneNumStr = (SceneManager.GetActiveScene().buildIndex - 2).ToString();
        if (sceneName.Contains("FittsPoke"))
        {
            iterationNumStr = PointTask.currentIteration.ToString();
        }
        else if (sceneName.Contains("FittsRay"))
        {
            iterationNumStr = RayTask.currentIteration.ToString();
        }
        else
        {
            iterationNumStr = "0";
        }

        string fname = sceneName + "_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logPath = Path.Combine(Application.persistentDataPath, fname);

        string fnameFull = sceneName + "_Full_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logPathFull = Path.Combine(Application.persistentDataPath, fnameFull);

        string fnameWalking = sceneName + "_Walking_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logWalkingPath = Path.Combine(Application.persistentDataPath, fnameWalking);

        string fnameEncumbrance = sceneName + "_Encumbrance_" + System.DateTime.Now.ToString("dd-MMM HH-mm-ss") + ".csv";
        logEncumbrancePath = Path.Combine(Application.persistentDataPath, fnameEncumbrance);

        allEntries.Add("sceneName,sceneNum,iterationNum,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,currentTime");

        allEntriesFull.Add("sceneName,sceneNum,iterationNum,buttonScale,buttonDistance,targetButton,clickedButton,centerLocationX,centerLocationY,centerLocationZ,fingerLocationX,fingerLocationY,fingerLocationZ,currentTime");

        walkingEntries.Add("sceneName,sceneNum,iterationNum,SmoothedHorizontal,SmoothedVertical,DirectionStability,VerticalPattern,HorizontalPattern,AvgSpeed,IsWalking,RawMoveX,RawMoveY,RawMoveZ,currentTime");

        encumbranceEntries.Add("sceneName,sceneNum,iterationNum,CurlI,CurlM,CurlR,CurlP,AvgGripCurl,PinchI,PinchM,PinchR,PinchP,AvgPinch,WristRotX,WristRotY,WristRotZ,DeltaX,DeltaY,DeltaZ,WristStable,GripHeld,PinchHeld,Encumbrance,currentTime");
        
    }

    void FixedUpdate()
    {
        if (startRecord)
        {
            if (sceneName.Contains("FittsPoke"))
            {
                if (logButtonActive)
                {
                    LogFittsPoke();
                    LogButtonDeactive();
                }
                LogFittsPokeFull();
            }
            else if (sceneName.Contains("FittsRay"))
            {
                if (logButtonActive)
                {
                    LogFittsRay();
                    LogButtonDeactive();
                }
                LogFittsRayFull();
            }
            LogWalking();
            LogEncumbrance();
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
    void WriteWalkingToCSV()
    {
        File.AppendAllLines(logWalkingPath, walkingEntries);
        walkingEntries.Clear();
        Debug.Log("WriteWalkingToCSV");
    }
    void WriteEncumbranceToCSV()
    {
        File.AppendAllLines(logEncumbrancePath, encumbranceEntries);
        encumbranceEntries.Clear();
        Debug.Log("WriteEncumbranceToCSV");
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
    public void StopRecordWalkingLog()
    {
        startRecord = false;
        Debug.Log("StopRecordWalkingLog");
        WriteWalkingToCSV();
    }
    public void StopRecordEncumbranceLog()
    {
        startRecord = false;
        Debug.Log("StopRecordEncumbranceLog");
        WriteEncumbranceToCSV();
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
        //buttonScale = PointTask.scales[PointTask.randomList[PointTask.currentIteration - 1]];
        buttonScale = pokeCenterLocation[PointTask.currentIndex].transform.localScale.x;
        //buttonDistance = PointTask.distances[PointTask.randomList[PointTask.currentIteration - 1]];
        buttonDistance = PointTask.distances[PointTask.randomList[PointTask.currentIteration - 1]];
        targetButton = PointTask.currentIndex;
        clickedButton = PointTask.buttonNumber;
        // centerLocation = PointTask.centerLocation;
        // fingerLocation = PointTask.fingerLocation;
        Vector3 location = rightFingerLocation.transform.position;
        Vector3 pokeLoc = pokeCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            sceneNumStr + "," +
            iterationNumStr + "," +
            buttonScale.ToString("F3") + "," +
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
        //buttonScale = PointTask.scales[PointTask.randomList[PointTask.currentIteration - 1]];
        buttonScale = pokeCenterLocation[PointTask.currentIndex].transform.localScale.x;
        //buttonDistance = PointTask.distances[PointTask.randomList[PointTask.currentIteration - 1]];
        buttonDistance = PointTask.distances[PointTask.randomList[PointTask.currentIteration - 1]];
        targetButton = PointTask.currentIndex;
        // clickedButton = PointTask.buttonNumber;
        // centerLocation = PointTask.centerLocation;
        // fingerLocation = PointTask.fingerLocation;
        Vector3 location = rightFingerLocation.transform.position;
        Vector3 pokeLoc = pokeCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            sceneNumStr + "," +
            iterationNumStr + "," +
            buttonScale.ToString("F3") + "," +
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
        //buttonScale = RayTask.scales[RayTask.randomList[RayTask.currentIteration - 1]];
        buttonScale = rayCenterLocation[RayTask.currentIndex].transform.localScale.x;
        buttonDistance = RayTask.distances[RayTask.randomList[RayTask.currentIteration - 1]];
        targetButton = RayTask.currentIndex;
        clickedButton = RayTask.buttonNumber;
        // centerLocation = RayTask.centerLocation;
        // fingerLocation = FittsRayTutorial.fingerLocation;
        Vector3 location = rayLocation.transform.position;
        Vector3 rayLoc = rayCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            sceneNumStr + "," +
            iterationNumStr + "," +
            buttonScale.ToString("F3") + "," +
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
        //buttonScale = RayTask.scales[RayTask.randomList[RayTask.currentIteration - 1]];
        buttonScale = rayCenterLocation[RayTask.currentIndex].transform.localScale.x;
        buttonDistance = RayTask.distances[RayTask.randomList[RayTask.currentIteration - 1]];
        targetButton = RayTask.currentIndex;
        clickedButton = RayTask.buttonNumber;
        // centerLocation = RayTask.centerLocation;
        // fingerLocation = FittsRayTutorial.fingerLocation;
        Vector3 location = rayLocation.transform.position;
        Vector3 rayLoc = rayCenterLocation[clickedButton].transform.position;

        currentEntry = new string(
            sceneName + "," +
            sceneNumStr + "," +
            iterationNumStr + "," +
            buttonScale.ToString("F3") + "," +
            buttonDistance.ToString() + "," +
            targetButton.ToString() + "," +
            clickedButton.ToString() + "," +
            Vector3ToString(rayLoc) + "," +
            Vector3ToString(location) + "," +
            GetTimeStamp());

        allEntries.Add(currentEntry);

        currentEntry = new string("");
    }

    void LogWalking()
    {
        if (walkDetector == null) return;

        Vector3 rawMove = walkDetector.RawFrameMovement;

        currentEntry = new string(
            sceneName + "," +
            sceneNumStr + "," +
            iterationNumStr + "," +
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
            sceneName + "," +
            sceneNumStr + "," +
            iterationNumStr + "," +
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
            encumbranceDetector.DeltaX.ToString("F2") + "," +
            encumbranceDetector.DeltaY.ToString("F2") + "," +
            encumbranceDetector.DeltaZ.ToString("F2") + "," +
            encumbranceDetector.WristStableTime.ToString("F2") + "," +
            (encumbranceDetector.GripHeld ? 1 : 0) + "," +
            (encumbranceDetector.PinchHeld ? 1 : 0) + "," +
            (encumbranceDetector.isEncumbrance ? 1 : 0) + "," +
            GetTimeStamp());

        encumbranceEntries.Add(currentEntry);

        currentEntry = new string("");
    }
}