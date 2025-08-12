using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EnumbranceLogger : MonoBehaviour
{
    public HandEncumbranceDetector detector;
    private StreamWriter csvWriter;
    private string filePath;
    private bool isInitialized;

    void Start()
    {
        if (detector == null)
        {
            detector = GetComponent<HandEncumbranceDetector>();
            if (detector == null)
            {
                Debug.LogError("HandEncumbranceLogger: No detector assigned or found!");
                return;
            }
        }

        try
        {
            filePath = Path.Combine(Application.persistentDataPath, 
                                   $"hand_encumbrance_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            
            csvWriter = new StreamWriter(filePath, false);
            csvWriter.WriteLine("Timestamp,Time,CurlI,CurlM,CurlR,AvgGripCurl,PinchI,PinchM,PinchR,AvgPinch,DeltaX,DeltaY,DeltaZ,WristStable,GripHeld,PinchHeld,Encumbrance");
            isInitialized = true;
            
            Debug.Log($"Hand encumbrance logging started at: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize logger: {e.Message}");
            isInitialized = false;
        }
    }

    void LateUpdate()
    {
        if (!isInitialized || detector == null) return;
        
        try
        {
            // Get current timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            csvWriter.WriteLine(
                $"{timestamp}," +  // Timestamp first
                $"{Time.time:F3}," +  // Then game time
                $"{detector.CurlIndex:F1}," +
                $"{detector.CurlMiddle:F1}," +
                $"{detector.CurlRing:F1}," +
                $"{detector.AvgGripCurl:F1}," +
                $"{detector.PinchIndex:F2}," +
                $"{detector.PinchMiddle:F2}," +
                $"{detector.PinchRing:F2}," +
                $"{detector.AvgPinch:F2}," +
                $"{detector.DeltaX:F1}," +
                $"{detector.DeltaY:F1}," +
                $"{detector.DeltaZ:F1}," +
                $"{detector.WristStableTime:F2}," +
                $"{(detector.GripHeld ? 1 : 0)}," +
                $"{(detector.PinchHeld ? 1 : 0)}," +
                $"{(detector.isEncumbrance ? 1 : 0)}"
            );
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Logging error: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        CloseWriter();
    }

    void OnDestroy()
    {
        CloseWriter();
    }

    private void CloseWriter()
    {
        if (csvWriter != null)
        {
            try
            {
                csvWriter.Flush();
                csvWriter.Close();
                Debug.Log($"Hand encumbrance data saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error closing log file: {e.Message}");
            }
        }
        isInitialized = false;
    }
}
