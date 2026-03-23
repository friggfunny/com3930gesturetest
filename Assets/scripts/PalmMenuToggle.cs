using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class PalmMenuToggle : MonoBehaviour
{
    [Header("Menu Setup")]
    public GameObject controlsPlane;
    public float hoverHeight = 0.05f; // 5cm hover distance

    [Tooltip("Tweak this if the image is upside down or sideways (e.g., 90, 180, 0)")]
    public Vector3 rotationOffset = new Vector3(0, 0, 0);

    [Header("Squeeze Settings")]
    [Tooltip("How close fingertips must be to the palm to count as a squeeze (in meters)")]
    public float fistThreshold = 0.07f;

    private XRHandSubsystem handSubsystem;
    private bool menuActive = false;
    private bool wasSqueezing = false;

    void Start()
    {
        // Start with the menu hidden
        if (controlsPlane != null) controlsPlane.SetActive(false);
    }

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        TrackAndToggleRightHand(handSubsystem.rightHand);
    }

    void TrackAndToggleRightHand(XRHand hand)
    {
        if (!hand.isTracked)
        {
            // If the hand completely loses tracking, optionally hide the menu
            controlsPlane.SetActive(false);
            return;
        }

        // Grab the necessary joints
        var palm = hand.GetJoint(XRHandJointID.Palm);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        var ringTip = hand.GetJoint(XRHandJointID.RingTip);

        if (palm.TryGetPose(out Pose palmPose) &&
            indexTip.TryGetPose(out Pose indexPose) &&
            middleTip.TryGetPose(out Pose middlePose) &&
            ringTip.TryGetPose(out Pose ringPose))
        {
            // 1. Check the distance from fingertips to the center of the palm
            float indexDist = Vector3.Distance(palmPose.position, indexPose.position);
            float middleDist = Vector3.Distance(palmPose.position, middlePose.position);
            float ringDist = Vector3.Distance(palmPose.position, ringPose.position);

            // If all three fingers are curled in close to the palm, it's a fist!
            bool isSqueezing = (indexDist < fistThreshold && middleDist < fistThreshold && ringDist < fistThreshold);

            // 2. The Toggle Logic
            if (isSqueezing && !wasSqueezing)
            {
                menuActive = !menuActive; // Flip the switch!
            }
            wasSqueezing = isSqueezing;

            // 3. Update the Hologram
            if (menuActive)
            {
                controlsPlane.SetActive(true);

                // THE FIX: We changed a + to a - so it goes into the palm, not out the back
                controlsPlane.transform.position = palmPose.position - (palmPose.up * hoverHeight);

                // Apply rotation, plus our custom offset just in case the image is sideways
                controlsPlane.transform.rotation = palmPose.rotation * Quaternion.Euler(rotationOffset);
            }
            else
            {
                controlsPlane.SetActive(false);
            }
        }
    }
}