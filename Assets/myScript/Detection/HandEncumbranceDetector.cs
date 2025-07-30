using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HandEncumbranceDetector: MonoBehaviour
{
    [Header("OVR References")]
    public OVRSkeleton skeleton;      // drag in your OVRSkeleton component
    public OVRHand hand;         // drag in your OVRHand component

    [Header("Debug UI")]
    public Text debugText;    // drag in a UI Text to show the debug info

    [Header("Encumbrance Settings")]
    public float curlThreshold = 100f;  
    public float pinchThreshold = 0.5f; // threshold for pinch strength

    public float wristXThreshold = 0.1f;
    public float wristYThreshold = 0.1f;
    public float wristZThreshold = 0.1f;


    public bool isEncumbrance { get; private set; }
    bool bonesReady = false;
    Vector3 prevWristEuler;
    bool hasPrev = false;

    void Start()
    {
        if (skeleton == null) skeleton = GetComponent<OVRSkeleton>();
        if (hand     == null) hand     = GetComponent<OVRHand>();
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

        float avgGripCurl = (curlIndex + curlMiddle + curlRing) / 3f;

        // individual pinches
        float pinchIndex  = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float pinchMiddle = hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float pinchRing   = hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        float pinchPinky  = hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);

        float avgPinch = (pinchIndex + pinchMiddle + pinchRing) / 3f;

        // individual finger confidence
        var confIndex = hand.GetFingerConfidence(OVRHand.HandFinger.Index);
        var confMiddle = hand.GetFingerConfidence(OVRHand.HandFinger.Middle);
        var confRing = hand.GetFingerConfidence(OVRHand.HandFinger.Ring);
        var confPinch = hand.GetFingerConfidence(OVRHand.HandFinger.Pinky);

        // wrist orientation
        var wristBone = skeleton.Bones.First(b => b.Id == OVRSkeleton.BoneId.Hand_WristRoot);
        Vector3 wristEuler = wristBone.Transform.rotation.eulerAngles;
        float Normalize(float a) => a > 180f ? a - 360f : a;
        if (!hasPrev)
        {
            prevWristEuler = wristEuler; // store initial value
            hasPrev = true;
            return;
        }
        // normalize wrist angles to -180 to 180 range
        float wristX = Normalize(wristEuler.x);  // flexion/extension
        float wristY = Normalize(wristEuler.y);  // pronation/supination
        float wristZ = Normalize(wristEuler.z);  // radial/ulnar deviation

        float prevX = Normalize(prevWristEuler.x);
        float prevY = Normalize(prevWristEuler.y);
        float prevZ = Normalize(prevWristEuler.z);

        // compute the true smallest delta around wrap
        float dx = Mathf.DeltaAngle(prevX, wristX);
        float dy = Mathf.DeltaAngle(prevY, wristY); 
        float dz = Mathf.DeltaAngle(prevZ, wristZ);


        bool grip = avgGripCurl < curlThreshold;
        bool pinch = avgPinch > pinchThreshold;
        bool wristXOk = Mathf.Abs(dx) < wristXThreshold;
        bool wristYOk = Mathf.Abs(dy) < wristYThreshold;
        bool wristZOk = Mathf.Abs(dz) < wristZThreshold;
        bool wristStationary = wristXOk && wristYOk && wristZOk;

        isEncumbrance = wristStationary && (grip || pinch);

        prevWristEuler = wristEuler; // store for next frame

        // update debug UI
        debugText.text = 
            $"Curl: I={curlIndex:F1}°, M={curlMiddle:F1}°, R={curlRing:F1}°, AVG={avgGripCurl:F1}°\n" +
            $"Pinch: I={pinchIndex:F1}, M={pinchMiddle:F1}, R={pinchRing:F1}, AVG={avgPinch:F1}\n" +
            $"Wrist rot:   ({wristX:F0}, {wristY:F0}, {wristZ:F0})\n" +
            $"Confidence: I={confIndex}, M={confMiddle}, R={confRing}, P={confPinch}\n" +
            $"Grip: {grip}, Pinch: {pinch}\n" +
            $"Wrist X: {wristXOk} {Mathf.Abs(dx):F2}, Y: {wristYOk} {Mathf.Abs(dy):F2}, Z: {wristZOk} {Mathf.Abs(dz):F2}\n" +
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
