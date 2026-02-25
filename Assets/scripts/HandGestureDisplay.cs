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

        string leftGesture = DetectPose(handSubsystem.leftHand);
        string rightGesture = DetectPose(handSubsystem.rightHand);

        uiText.text = $"<color=#00FF00>Left:</color> {leftGesture}\n<color=#00FFFF>Right:</color> {rightGesture}";
    }

    string DetectPose(XRHand hand)
    {
        if (!hand.isTracked) return "Lost";

        // Get joint data
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var palm = hand.GetJoint(XRHandJointID.Palm);

        // Check for Pinch (Thumb to Index distance)
        if (thumbTip.TryGetPose(out Pose tPose) && indexTip.TryGetPose(out Pose iPose))
        {
            float pinchDist = Vector3.Distance(tPose.position, iPose.position);
            if (pinchDist < 0.02f) return "PINCHING";
        }

        // Check for Thumbs Up (Thumb high, Index low relative to palm)
        if (thumbTip.TryGetPose(out Pose thumbP) && palm.TryGetPose(out Pose palmP))
        {
            // If thumb is significantly "above" the palm in world space
            if (thumbP.position.y > palmP.position.y + 0.1f) return "THUMBS UP";
        }

        return "Open Hand";
    }
}