using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    [System.Serializable]
    public class HandSettings
    {
        public OVRHand hand;
        public float gripThreshold     = 0.3f;   // average pinch across fingers
        [HideInInspector] public Vector3 lastPosition;
        [HideInInspector] public float   encumbranceTimer;
    }

    [Header("Hand Settings")]
    public HandSettings leftHand;
    public HandSettings rightHand;

    [Header("Detection Params")]
    public float holdTimeRequired = 3f;
    public float cooldownTime     = 2f;

    private float cooldownTimer;

    // Head tracking variables
    private Transform playerHead;
    private Vector3 lastHeadPosition;

    [Header("UI Elements")]
    public Text leftDebugText;
    public Text rightDebugText;
    public Text encumbranceText;

    void Start()
    {
        // Initialize head reference
        playerHead = FindObjectOfType<OVRCameraRig>().centerEyeAnchor;
        if (playerHead == null)
        {
            Debug.LogError("OVRCameraRig not found in scene!");
            enabled = false;
            return;
        }
        lastHeadPosition = playerHead.position;

        AssignHands();
        if (leftHand.hand != null)  leftHand.lastPosition  = GetHandPosition(leftHand.hand);
        if (rightHand.hand != null) rightHand.lastPosition = GetHandPosition(rightHand.hand);
        encumbranceText.text = "";
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            encumbranceText.text = $"Cooldown: {cooldownTimer:F1}s";
            return;
        }
        else
        {
            encumbranceText.text = "";
        }

        // Calculate head movement since last frame
        Vector3 headMovement = playerHead.position - lastHeadPosition;
        lastHeadPosition = playerHead.position;

        CheckHand(leftHand, "Left", leftDebugText, headMovement);
        CheckHand(rightHand, "Right", rightDebugText, headMovement);

        if (leftHand.encumbranceTimer  >= holdTimeRequired ||
            rightHand.encumbranceTimer >= holdTimeRequired)
        {
            OnEncumbranceDetected();
        }
    }

    private void CheckHand(HandSettings hs, string label, Text debugText, Vector3 headMovement)
    {
        if (hs.hand == null)
        {
            debugText.text = $"{label}: hand null";
            return;
        }

        bool isTracked = hs.hand.IsTracked;
        var confidence = hs.hand.HandConfidence;

        if (!isTracked)
        {
            hs.encumbranceTimer = 0;
            debugText.text = $"{label}: Tracked={isTracked}";
            return;
        }

        // individual pinches
        float pinchIndex  = hs.hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float pinchMiddle = hs.hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float pinchRing   = hs.hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        float pinchPinky  = hs.hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);
        float avgPinch = (pinchIndex + pinchMiddle + pinchRing + pinchPinky) / 4f;

        // Calculate relative hand velocity
        Vector3 currentPos = GetHandPosition(hs.hand);
        Vector3 rawHandMovement = currentPos - hs.lastPosition;
        Vector3 relativeMovement = rawHandMovement - headMovement;
        float velocity = relativeMovement.magnitude / Time.deltaTime;
        hs.lastPosition = currentPos;

        bool isSlowGrip = avgPinch >= hs.gripThreshold;
        if (isSlowGrip)
            hs.encumbranceTimer += Time.deltaTime;
        else
            hs.encumbranceTimer = Mathf.Max(0, hs.encumbranceTimer - Time.deltaTime * 2);

        debugText.text =
            $"{label}: Tracked={isTracked}, Conf={confidence}\n" +
            $"Pinch I={pinchIndex:F2}, M={pinchMiddle:F2}, R={pinchRing:F2}, P={pinchPinky:F2}\n" +
            $"Avg={avgPinch:F2}, Vel={velocity:F2} m/s\n" +
            $"Time={hs.encumbranceTimer:F1}/{holdTimeRequired}";
    }

    private Vector3 GetHandPosition(OVRHand hand)
    {
        // use wrist bone as hand position fallback
        var skel = hand.GetComponent<OVRSkeleton>();
        if (skel != null && skel.Bones != null)
        {
            var wrist = skel.Bones.FirstOrDefault(b => b.Id == OVRSkeleton.BoneId.Hand_WristRoot);
            if (wrist != null)
                return wrist.Transform.position;
        }
        return hand.transform.position;
    }

    private void OnEncumbranceDetected()
    {
        encumbranceText.text = "Encumbrance Detected!";

        leftHand.encumbranceTimer  = 0;
        rightHand.encumbranceTimer = 0;
        cooldownTimer              = cooldownTime;
    }

    private void AssignHands()
    {
        foreach (var hand in FindObjectsOfType<OVRHand>())
        {
            var skel = hand.GetComponent<OVRSkeleton>();
            if (skel.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft)
                leftHand.hand = hand;
            else if (skel.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight)
                rightHand.hand = hand;
        }
    }
}


