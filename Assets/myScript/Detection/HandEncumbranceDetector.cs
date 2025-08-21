using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HandEncumbranceDetector : MonoBehaviour
{
    [Header("OVR References")]
    public OVRSkeleton skeleton;      // drag in your OVRSkeleton component
    public OVRHand hand;         // drag in your OVRHand component
    [SerializeField] public WalkDetector walkDetector;

    [Header("Debug UI")]
    public Text debugText;    // drag in a UI Text to show the debug info

    [Header("Encumbrance Settings")]
    public float curlThreshold = 100f;
    public float pinchThreshold = 0.5f; // threshold for pinch strength
    public float wristXThreshold = 32f;
    public float wristYThreshold = 32f;
    public float wristZThreshold = 32f;
    public float gripRequiredStableDuration = 0.3f;
    public float wristRequiredStableDuration = 1f;

    // public float encumbranceStateDuration = 1f; // Persistence duration



    public bool isEncumbrance { get; private set; }
    //private bool immediateEncumbranceState; // immediate state for encumbrance detection
    //public float TimeSinceLastEncumbrance { get; private set; }
    private bool bonesReady = false;

    // Wrist stability tracking
    private Vector3 referenceWristEuler;
    private float wristStableTime;
    private bool wasStable;

    // Grip/pinch stability tracking
    private float gripStableTime;
    private float pinchStableTime;
    // private float encumbranceStateTimer;
    

    public float CurlIndex { get; private set; }
    public float CurlMiddle { get; private set; }
    public float CurlRing { get; private set; }
    public float CurlPinky { get; private set; }
    public float AvgGripCurl { get; private set; }
    public float PinchIndex { get; private set; }
    public float PinchMiddle { get; private set; }
    public float PinchRing { get; private set; }
    public float PinchPinky { get; private set; }
    public float AvgPinch { get; private set; }
    public bool FingersHigh { get; private set; }
    public Vector3 WristRotation { get; private set; }
    public float DeltaX { get; private set; }
    public float DeltaY { get; private set; }
    public float DeltaZ { get; private set; }
    public float WristStableTime { get; private set; }
    public bool GripHeld { get; private set; }
    public bool PinchHeld { get; private set; }

    void Start()
    {
        if (skeleton == null) skeleton = GetComponent<OVRSkeleton>();
        if (hand == null) hand = GetComponent<OVRHand>();
        if (debugText == null)
            Debug.LogError("Please assign a UI Text to show debug output.", this);
    }

    void Update()
    {
        // wait until bones are initialized
        if (!bonesReady && skeleton.Bones.Count > 0 && skeleton.IsDataValid)
        {
            bonesReady = true;
            // you could reorder skeleton.Bones here if you want faster lookups
        }
        if (!bonesReady) return;

        // compute per-finger curls
        float curlIndex = ComputeAverageCurl(OVRSkeleton.BoneId.Hand_Index1,
                                          OVRSkeleton.BoneId.Hand_Index2,
                                          OVRSkeleton.BoneId.Hand_Index3); // 2.6 - 3.3
        float curlMiddle = ComputeAverageCurl(OVRSkeleton.BoneId.Hand_Middle1,
                                          OVRSkeleton.BoneId.Hand_Middle2,
                                          OVRSkeleton.BoneId.Hand_Middle3);
        float curlRing = ComputeAverageCurl(OVRSkeleton.BoneId.Hand_Ring1,
                                          OVRSkeleton.BoneId.Hand_Ring2,
                                          OVRSkeleton.BoneId.Hand_Ring3);
        float curlPinky = ComputeAverageCurl(OVRSkeleton.BoneId.Hand_Pinky0,
                                             OVRSkeleton.BoneId.Hand_Pinky1,
                                             OVRSkeleton.BoneId.Hand_Pinky2);
        // thumb is a special case; we’ll approximate using two segments
        float thumbCurl = ComputeAverageCurl(OVRSkeleton.BoneId.Hand_Thumb0,
                                             OVRSkeleton.BoneId.Hand_Thumb1,
                                             OVRSkeleton.BoneId.Hand_Thumb2);

        float avgGripCurl = (curlMiddle + curlRing + curlPinky) / 3f;

        // individual pinches
        float pinchIndex = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float pinchMiddle = hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float pinchRing = hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        float pinchPinky = hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);

        float avgPinch = (pinchIndex + pinchMiddle + pinchRing) / 3f;

        // individual finger confidence
        var confIndex = hand.GetFingerConfidence(OVRHand.HandFinger.Index);
        var confMiddle = hand.GetFingerConfidence(OVRHand.HandFinger.Middle);
        var confRing = hand.GetFingerConfidence(OVRHand.HandFinger.Ring);
        var confPinky = hand.GetFingerConfidence(OVRHand.HandFinger.Pinky);

        bool fingersHigh =
            confIndex  == OVRHand.TrackingConfidence.High &&
            confMiddle == OVRHand.TrackingConfidence.High &&
            confRing   == OVRHand.TrackingConfidence.High &&
            confPinky  == OVRHand.TrackingConfidence.High;


        // Wrist orientation
        var wristBone = skeleton.Bones.First(b => b.Id == OVRSkeleton.BoneId.Hand_WristRoot);
        Vector3 currentWristEuler = wristBone.Transform.rotation.eulerAngles;

        // Normalize angles to -180 to 180 range
        float Normalize(float a) => a > 180f ? a - 360f : a;

        float refX = Normalize(referenceWristEuler.x);
        float refY = Normalize(referenceWristEuler.y);
        float refZ = Normalize(referenceWristEuler.z);

        float currentX = Normalize(currentWristEuler.x);
        float currentY = Normalize(currentWristEuler.y);
        float currentZ = Normalize(currentWristEuler.z);

        // Compute delta angles using reference rotation
        float dx = Mathf.DeltaAngle(refX, currentX);
        float dy = Mathf.DeltaAngle(refY, currentY);
        float dz = Mathf.DeltaAngle(refZ, currentZ);

        // Determine if wrist is currently stable
        bool isCurrentlyStable =
            Mathf.Abs(dx) < wristXThreshold &&
            Mathf.Abs(dy) < wristYThreshold &&
            Mathf.Abs(dz) < wristZThreshold;

        // Update wrist stability timer
        if (isCurrentlyStable)
        {
            wristStableTime += Time.deltaTime;
        }
        else
        {
            // Update reference if wrist moved significantly
            referenceWristEuler = currentWristEuler;
            wristStableTime = 0;
        }

        // Check if wrist has been stable long enough
        bool wristStationary = wristStableTime >= wristRequiredStableDuration;
        
        // Update grip/pinch stability with duration-based activation
        bool isGripActive = avgGripCurl < curlThreshold;
        bool isPinchActive = avgPinch > pinchThreshold;

        gripStableTime = isGripActive ?
            Mathf.Min(gripStableTime + Time.deltaTime, gripRequiredStableDuration) :
            0;

        pinchStableTime = isPinchActive ?
            Mathf.Min(pinchStableTime + Time.deltaTime, gripRequiredStableDuration) :
            0;

        // Final encumbrance detection
        bool gripHeld = gripStableTime >= gripRequiredStableDuration && walkDetector.IsWalking == false;
        bool pinchHeld = pinchStableTime >= gripRequiredStableDuration;

        isEncumbrance = (wristStationary || gripHeld) && fingersHigh;

        // // Update walking persistence timer
        // if (immediateEncumbranceState)
        // {
        //     encumbranceStateTimer = encumbranceStateDuration;
        //     TimeSinceLastEncumbrance = 0f;
        // }
        // else
        // {
        //     encumbranceStateTimer = Mathf.Max(0, encumbranceStateTimer - Time.deltaTime);
        //     TimeSinceLastEncumbrance += Time.deltaTime;
        // }

        // isEncumbrance = encumbranceStateTimer > 0;
        
        CurlIndex = curlIndex;
        CurlMiddle = curlMiddle;
        CurlRing = curlRing;
        CurlPinky = curlPinky;
        AvgGripCurl = avgGripCurl;
        PinchIndex = pinchIndex;
        PinchMiddle = pinchMiddle;
        PinchRing = pinchRing;
        PinchPinky = pinchPinky;
        AvgPinch = avgPinch;
        FingersHigh = fingersHigh;
        WristRotation = currentWristEuler;
        DeltaX = dx;
        DeltaY = dy;
        DeltaZ = dz;
        WristStableTime = wristStableTime;
        GripHeld = gripHeld;
        PinchHeld = pinchHeld;

        // update debug UI
        debugText.text =
            $"Curl: I={curlIndex:F1}°, M={curlMiddle:F1}°, R={curlRing:F1}°, AVG={avgGripCurl:F1}°\n" +
            $"Pinch: I={pinchIndex:F1}, M={pinchMiddle:F1}, R={pinchRing:F1}, AVG={avgPinch:F1}\n" +
            $"Wrist rot: ({currentX:F0}, {currentY:F0}, {currentZ:F0})\n" +
            $"Wrist delta: ({dx:F1}, {dy:F1}, {dz:F1})\n" +
            $"Wrist stable: {wristStableTime:F1}/{wristRequiredStableDuration:F1}s\n" +
            $"Confidence: I={confIndex}, M={confMiddle}, R={confRing}, P={confPinky}\n" +
            $"Grip: {gripHeld}, Pinch: {pinchHeld}\n" +
            $"Encumbrance: {isEncumbrance}";
    }

    /// <summary>
    /// Computes the average angle between two consecutive bone segments.
    /// You supply three BoneIds: proximal, intermediate, distal.
    /// </summary>
    float ComputeAverageCurl(OVRSkeleton.BoneId id1, OVRSkeleton.BoneId id2, OVRSkeleton.BoneId id3)
    {
        // 1) grab joint world‐positions
        Vector3 p1 = skeleton.Bones.First(b => b.Id == id1).Transform.position;
        Vector3 p2 = skeleton.Bones.First(b => b.Id == id2).Transform.position;
        Vector3 p3 = skeleton.Bones.First(b => b.Id == id3).Transform.position;

        // 2) form the two bone vectors (normalized)
        Vector3 dir1 = (p2 - p1).normalized;
        Vector3 dir2 = (p3 - p2).normalized;

        // 3) raw angle between them (0 = bones aligned, 180 = bones flipped)
        float rawAngle = Vector3.Angle(dir1, dir2);

        // 4) invert so straight = 0°, fully folded = ~180°
        float curl = 180f - rawAngle;

        return curl;
    }
}
