using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WalkDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float horizontalThreshold = 0.001f;
    [SerializeField] private float verticalThreshold = 0.0005f;
    [SerializeField] private float patternDuration = 1f;
    [SerializeField] private float minWalkingSpeed = 0.2f;
    [SerializeField] private float smoothingFactor = 0.2f;
    [SerializeField] private float walkingStateDuration = 1f; // Persistence duration

    [Header("UI Elements")]
    [SerializeField] private Text debugText;
    [SerializeField] private Text isWalkingText;

    public bool IsWalking { get; private set; }
    public float TimeSinceLastWalking { get; private set; }


    private Vector3 previousPosition;
    private float[] verticalMovementBuffer;
    private float[] horizontalMovementBuffer;
    private int bufferIndex;
    private float timeSinceLastBufferUpdate;
    private float bufferUpdateInterval = 0.05f;
    private float smoothedHorizontal;
    private float smoothedVertical;
    private float movementDirectionStability;
    private Vector2 previousMovementDirection;
    private float directionStabilityThreshold = 0.9f;
    private float walkingStateTimer;
    private bool immediateWalkingState;

    void Start()
    {
        previousPosition = Camera.main.transform.localPosition;

        // Initialize circular buffers to store movement patterns
        int bufferSize = Mathf.CeilToInt(patternDuration / bufferUpdateInterval);
        verticalMovementBuffer = new float[bufferSize];
        horizontalMovementBuffer = new float[bufferSize];

        bufferIndex = 0;
        timeSinceLastBufferUpdate = 0f;
        TimeSinceLastWalking = float.MaxValue;
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
            horizontalMovementBuffer[bufferIndex] = smoothedHorizontal;

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
        float verticalPatternScore = AnalyzeMovementPattern(verticalMovementBuffer);
        float horizontalPatternScore = AnalyzeMovementPattern(horizontalMovementBuffer);

        // Walking detection logic
        bool hasHorizontalMovement = smoothedHorizontal >= horizontalThreshold;
        bool hasVerticalPattern = verticalPatternScore >= 0.7f;
        bool hasStableDirection = movementDirectionStability >= directionStabilityThreshold;
        bool hasWalkingSpeed = CalculateAverageHorizontalSpeed() >= minWalkingSpeed;

        bool immediateWalkingState  = hasHorizontalMovement &&
                        hasVerticalPattern &&
                        hasStableDirection &&
                        hasWalkingSpeed;

        // Update walking persistence timer
        if (immediateWalkingState)
        {
            walkingStateTimer = walkingStateDuration;
            TimeSinceLastWalking = 0f;
        }
        else
        {
            walkingStateTimer = Mathf.Max(0, walkingStateTimer - Time.deltaTime);
            TimeSinceLastWalking += Time.deltaTime;
        }

        IsWalking = walkingStateTimer > 0;

        // Update UI
        UpdateDebugDisplay(verticalPatternScore, horizontalPatternScore, IsWalking);

        // Update previous values
        previousPosition = currentPosition;
    }

    private float AnalyzeMovementPattern(float[] buffer)
    {
        // Calculate the oscillation pattern in the buffer
        int peakCount = 0;
        float totalMovement = 0f;

        for (int i = 1; i < buffer.Length - 1; i++)
        {
            totalMovement += buffer[i];

            // Detect peaks (local maxima)
            if (buffer[i] > buffer[i - 1] && buffer[i] > buffer[i + 1])
            {
                peakCount++;
            }
        }

        // Calculate average movement
        float avgMovement = totalMovement / buffer.Length;

        // Pattern score based on peak count and movement consistency
        float peakScore = Mathf.Clamp01(peakCount / (patternDuration * 2f)); // Expect 2 steps/sec
        float movementConsistency = Mathf.Clamp01(avgMovement / horizontalThreshold);

        return (peakScore + movementConsistency) / 2f;
    }

    private float CalculateAverageHorizontalSpeed()
    {
        float total = 0f;
        foreach (float movement in horizontalMovementBuffer)
        {
            total += movement;
        }
        return total / (bufferUpdateInterval * horizontalMovementBuffer.Length);
    }

    private void UpdateDebugDisplay(float verticalScore, float horizontalScore, bool isWalking)
    {
        if (debugText != null)
        {
            debugText.text = $"Horizontal: {smoothedHorizontal:F5}\n" +
                           $"Vertical: {smoothedVertical:F5}\n" +
                           $"Direction Stability: {movementDirectionStability:F2}\n" +
                           $"Vertical Pattern: {verticalScore:F2}\n" +
                           $"Horizontal Pattern: {horizontalScore:F2}\n" +
                           $"Avg Speed: {CalculateAverageHorizontalSpeed():F3} m/s";
        }

        if (isWalkingText != null)
        {
            isWalkingText.text = $"Walking: {(isWalking ? "YES" : "NO")}";
            isWalkingText.color = isWalking ? Color.green : Color.red;
        }
    }
}