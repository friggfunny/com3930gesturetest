using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class RingFingerHatSpawner : MonoBehaviour
{
    [Header("Hat Settings")]
    [Tooltip("Drag your TestHat Prefab here.")]
    public GameObject hatPrefab;

    private XRHandSubsystem handSubsystem;

    // We only need to track the left hand's state
    private bool wasRingPinching = false;

    void Update()
    {
        // 1. Hook up the Hand Subsystem
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        // 2. ONLY check the left hand
        CheckLeftHandForRingPinch(handSubsystem.leftHand);
    }

    void CheckLeftHandForRingPinch(XRHand hand)
    {
        if (!hand.isTracked)
        {
            wasRingPinching = false;
            return;
        }

        // Get Thumb and Ring Finger tips
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var ringTip = hand.GetJoint(XRHandJointID.RingTip);

        if (thumbTip.TryGetPose(out Pose tPose) && ringTip.TryGetPose(out Pose rPose))
        {
            // Calculate distance between thumb and ring finger
            float pinchDist = Vector3.Distance(tPose.position, rPose.position);
            bool isRingPinching = pinchDist < 0.02f;

            // Trigger ONLY on the exact frame the pinch starts
            if (isRingPinching && !wasRingPinching)
            {
                // Spawn the hat exactly between the thumb and ring finger
                Vector3 spawnPosition = Vector3.Lerp(tPose.position, rPose.position, 0.5f);
                SpawnHat(spawnPosition);
            }

            wasRingPinching = isRingPinching;
        }
    }

    void SpawnHat(Vector3 position)
    {
        if (hatPrefab != null)
        {
            // Spawn the hat upright
            Instantiate(hatPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("You forgot to assign the Hat Prefab in the Inspector!");
        }
    }
}