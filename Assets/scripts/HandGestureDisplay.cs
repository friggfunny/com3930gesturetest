using UnityEngine;
using UnityEngine.XR.Hands; // Requires 'XR Hands' package
using TMPro;

public class HandGestureDisplay : MonoBehaviour
{
    public TextMeshProUGUI uiText;
    private XRHandSubsystem handSubsystem;

    void Update()
    {
        // Find the hand subsystem if we don't have it
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        // 1. Detect individual hand poses
        string leftGesture = DetectPose(handSubsystem.leftHand);
        string rightGesture = DetectPose(handSubsystem.rightHand);

        // 2. Detect Two-Handed Poses (Like your small table thumbs)
        string combinedGesture = "";
        if (handSubsystem.leftHand.isTracked && handSubsystem.rightHand.isTracked)
        {
            var lThumb = handSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);
            var rThumb = handSubsystem.rightHand.GetJoint(XRHandJointID.ThumbTip);

            if (lThumb.TryGetPose(out Pose lPose) && rThumb.TryGetPose(out Pose rPose))
            {
                if (Vector3.Distance(lPose.position, rPose.position) < 0.05f)
                {
                    combinedGesture = "\n<color=#FFFF00>Combined:</color> THUMBS TOUCHING (Table)";
                }
            }
        }

        // Update the UI
        uiText.text = $"<color=#00FF00>Left:</color> {leftGesture}\n<color=#00FFFF>Right:</color> {rightGesture}{combinedGesture}";
    }

    string DetectPose(XRHand hand)
    {
        if (!hand.isTracked) return "Lost";

        // Get joint data
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        var ringTip = hand.GetJoint(XRHandJointID.RingTip);
        var pinkyTip = hand.GetJoint(XRHandJointID.LittleTip);
        var palm = hand.GetJoint(XRHandJointID.Palm);

        bool hasThumb = thumbTip.TryGetPose(out Pose tPose);
        bool hasIndex = indexTip.TryGetPose(out Pose iPose);
        bool hasMiddle = middleTip.TryGetPose(out Pose mPose);
        bool hasRing = ringTip.TryGetPose(out Pose rPose);
        bool hasPinky = pinkyTip.TryGetPose(out Pose pPose);
        bool hasPalm = palm.TryGetPose(out Pose palmPose);

        // --- NEW CUSTOM GESTURES ---

        // 1. Pinky Pinch (Caesar Spawner)
        if (hasThumb && hasPinky)
        {
            if (Vector3.Distance(tPose.position, pPose.position) < 0.02f) return "PINKY PINCH (Caesar)";
        }

        // 2. Squeeze / Fist (Menu Toggle) - Index, Middle, Ring all close to Palm
        if (hasIndex && hasMiddle && hasRing && hasPalm)
        {
            float iDist = Vector3.Distance(palmPose.position, iPose.position);
            float mDist = Vector3.Distance(palmPose.position, mPose.position);
            float rDist = Vector3.Distance(palmPose.position, rPose.position);

            if (iDist < 0.07f && mDist < 0.07f && rDist < 0.07f) return "SQUEEZE (Menu)";
        }

        // --- OLD STANDARD GESTURES ---

        // 3. Check for standard Pinch (Thumb to Index distance)
        if (hasThumb && hasIndex)
        {
            if (Vector3.Distance(tPose.position, iPose.position) < 0.02f) return "INDEX PINCH";
        }

        // 4. Check for Thumbs Up (Thumb high, Index low relative to palm)
        if (hasThumb && hasPalm)
        {
            // If thumb is significantly "above" the palm in world space
            if (tPose.position.y > palmPose.position.y + 0.1f) return "THUMBS UP";
        }

        return "Open Hand";
    }
}