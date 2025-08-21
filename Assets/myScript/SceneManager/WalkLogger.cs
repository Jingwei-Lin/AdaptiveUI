using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
public class WalkLogger : MonoBehaviour
{
    public WalkDetector detector;
    private StreamWriter csvWriter;
    private string filePath;
    private bool isInitialized;

    void Start()
    {
        if (detector == null)
        {
            detector = GetComponent<WalkDetector>();
            if (detector == null)
            {
                Debug.LogError("WalkDetectorLogger: No detector assigned or found!");
                return;
            }
        }

        try
        {
            filePath = Path.Combine(Application.persistentDataPath, 
                                   $"walk_detection_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            
            csvWriter = new StreamWriter(filePath, false);
            csvWriter.WriteLine("Timestamp,Time,SmoothedHorizontal,SmoothedVertical,DirectionStability,VerticalPattern,AvgSpeed,IsWalking");
            isInitialized = true;
            
            Debug.Log($"Walking detection logging started at: {filePath}");
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
                $"{detector.SmoothedHorizontal:F5}," +
                $"{detector.SmoothedVertical:F5}," +
                $"{detector.MovementDirectionStability:F2}," +
                $"{detector.VerticalPatternScore:F2}," +
                $"{detector.AverageHorizontalSpeed:F3}," +
                $"{(detector.IsWalking ? 1 : 0)}"
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
                Debug.Log($"Walking detection data saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error closing log file: {e.Message}");
            }
        }
        isInitialized = false;
    }
}
