using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class WalkDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float horizontalThreshold = 0.01f;
    [SerializeField] private float verticalThreshold = 0.001f;
    [SerializeField] private float patternDuration = 1f;
    [SerializeField] private float minWalkingSpeed = 0.2f;
    [SerializeField] private float smoothingFactor = 0.2f;

    [Header("UI Elements")]
    [SerializeField] private Text debugText;
    [SerializeField] private Text isWalkingText;

    public bool IsWalking { get; private set; }
    //public float TimeSinceLastWalking { get; private set; }


    private Vector3 previousPosition;
    private float[] verticalMovementBuffer;
    private int bufferIndex;
    private float timeSinceLastBufferUpdate;
    private float bufferUpdateInterval = 0.05f;
    private float smoothedHorizontal;
    private float smoothedVertical;
    private float movementDirectionStability;
    private Vector2 previousMovementDirection;
    private float directionStabilityThreshold = 0.9f;
    //private float walkingStateTimer;
    //private bool immediateWalkingState;

    public float SmoothedHorizontal { get; private set; }
    public float SmoothedVertical { get; private set; }
    public float MovementDirectionStability { get; private set; }
    public float VerticalPatternScore { get; private set; }
    public float AverageHorizontalSpeed { get; private set; }
    public Vector3 RawFrameMovement { get; private set; }

    void Start()
    {
        previousPosition = Camera.main.transform.localPosition;

        // Initialize circular buffers to store movement patterns
        int bufferSize = Mathf.CeilToInt(patternDuration / bufferUpdateInterval);
        verticalMovementBuffer = new float[bufferSize];

        bufferIndex = 0;
        timeSinceLastBufferUpdate = 0f;
        //TimeSinceLastWalking = float.MaxValue;
    }

    void Update()
    {
        Vector3 currentPosition = Camera.main.transform.localPosition;

        // Calculate frame-to-frame movement
        Vector3 frameMovement = currentPosition - previousPosition;
        Vector2 horizontalMovement = new Vector2(frameMovement.x, frameMovement.z);

        // Calculate smoothed movement values
        smoothedHorizontal = smoothingFactor * smoothedHorizontal +
                            (1 - smoothingFactor) * horizontalMovement.magnitude;
        smoothedVertical = smoothingFactor * smoothedVertical +
                           (1 - smoothingFactor) * Mathf.Abs(frameMovement.y);

        // Update movement pattern buffers at fixed intervals
        timeSinceLastBufferUpdate += Time.deltaTime;
        if (timeSinceLastBufferUpdate >= bufferUpdateInterval)
        {
            verticalMovementBuffer[bufferIndex] = smoothedVertical;
            bufferIndex = (bufferIndex + 1) % verticalMovementBuffer.Length;
            timeSinceLastBufferUpdate = 0f;
        }

        // Calculate movement direction stability
        Vector2 currentDirection = horizontalMovement.normalized;
        if (horizontalMovement.magnitude > horizontalThreshold && previousMovementDirection.magnitude > 0.1f)
        {
            float directionSimilarity = Vector2.Dot(currentDirection, previousMovementDirection);
            movementDirectionStability = smoothingFactor * movementDirectionStability +
                                        (1 - smoothingFactor) * Mathf.Clamp01(directionSimilarity);
        }
        previousMovementDirection = currentDirection;

        // Analyze movement patterns
        float verticalPatternScore = AnalyzeMovementPattern(verticalMovementBuffer, verticalThreshold);
        // Walking detection logic
        bool isMovingHorizontally = smoothedHorizontal >= horizontalThreshold;
        bool hasVerticalPattern = verticalPatternScore >= 0.7f;
        bool hasStableDirection = movementDirectionStability >= directionStabilityThreshold;
        bool hasWalkingSpeed = CalculateHorizontalSpeed() >= minWalkingSpeed;

        IsWalking = isMovingHorizontally &&
                        hasVerticalPattern &&
                        hasStableDirection &&
                        hasWalkingSpeed;

        // Update walking persistence timer
        // if (immediateWalkingState)
        // {
        //     walkingStateTimer = walkingStateDuration;
        //     TimeSinceLastWalking = 0f;
        // }
        // else
        // {
        //     walkingStateTimer = Mathf.Max(0, walkingStateTimer - Time.deltaTime);
        //     TimeSinceLastWalking += Time.deltaTime;
        // }

        // IsWalking = walkingStateTimer > 0;

        SmoothedHorizontal = smoothedHorizontal;
        SmoothedVertical = smoothedVertical;
        MovementDirectionStability = movementDirectionStability;
        VerticalPatternScore = verticalPatternScore;
        AverageHorizontalSpeed = CalculateHorizontalSpeed();
        RawFrameMovement = frameMovement;

        // Update UI
        UpdateDebugDisplay(verticalPatternScore, IsWalking);

        // Update previous values
        previousPosition = currentPosition;
    }

    private float AnalyzeMovementPattern(float[] buffer, float threshold)
    {
        if (buffer.Length < 3) return 0f; // Need at least 3 elements for pattern analysis
        
        int peakCount = 0;
        float totalMovement = 0f;
        float minPeakHeight = threshold * 2f;

        for (int i = 1; i < buffer.Length - 1; i++)
        {
            totalMovement += buffer[i];

            // Detect significant peaks only
            if (buffer[i] > buffer[i - 1] && 
                buffer[i] > buffer[i + 1] && 
                buffer[i] > minPeakHeight)
            {
                peakCount++;
            }
        }

        // Calculate average movement
        int numElements = buffer.Length - 2;
        float avgMovement = numElements > 0 ? totalMovement / numElements : 0f;
        
        // If movement is negligible, return 0
        if (avgMovement < threshold * 0.5f)
            return 0f;

        // Pattern score based on peak count and movement consistency
        float expectedPeaks = patternDuration * 2f; // Expect 2 steps/sec
        float peakScore = Mathf.Clamp01(peakCount / expectedPeaks);
        float movementConsistency = Mathf.Clamp01(avgMovement / threshold);

        return (peakScore + movementConsistency) / 2f;
    }

    private float CalculateHorizontalSpeed()
    {
        Vector3 currentPosition = Camera.main.transform.localPosition;
        Vector3 rawMovementThisFrame = currentPosition - previousPosition;
        Vector2 rawHorizontalMovement = new Vector2(rawMovementThisFrame.x, rawMovementThisFrame.z);
        float HorizontalSpeed = rawHorizontalMovement.magnitude / Time.deltaTime; // Instantaneous speed
        return HorizontalSpeed;
    }

    private void UpdateDebugDisplay(float verticalScore, bool isWalking)
    {
        if (debugText != null)
        {
            debugText.text = $"Horizontal: {smoothedHorizontal:F5}\n" +
                           $"Vertical: {smoothedVertical:F5}\n" +
                           $"Direction Stability: {movementDirectionStability:F2}\n" +
                           $"Vertical Pattern: {verticalScore:F2}\n" +
                           $"Avg Speed: {CalculateHorizontalSpeed():F3} m/s";
        }

        if (isWalkingText != null)
        {
            isWalkingText.text = $"Walking: {(isWalking ? "YES" : "NO")}";
            isWalkingText.color = isWalking ? Color.green : Color.red;
        }
    }
}