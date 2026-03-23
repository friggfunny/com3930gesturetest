using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
// Needed to detect grabbable objects

public class MiddleFingerHatSpawner : MonoBehaviour
{
    [Header("Hat Settings")]
    [Tooltip("Drag your Hat 2 Prefab here.")]
    public GameObject hat2Prefab; // Updated for Hat 2

    [Header("Spawn Prevention")]
    [Tooltip("How close your hand can be to an existing object before spawning is blocked (in meters).")]
    public float noSpawnRadius = 0.15f; // 15cm radius

    private XRHandSubsystem handSubsystem;
    private bool wasMiddlePinching = false;

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        CheckLeftHandForMiddlePinch(handSubsystem.leftHand);
    }

    void CheckLeftHandForMiddlePinch(XRHand hand)
    {
        if (!hand.isTracked)
        {
            wasMiddlePinching = false;
            return;
        }

        // HERE IS THE MAGIC SWITCH: We changed RingTip to MiddleTip
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);

        if (thumbTip.TryGetPose(out Pose tPose) && middleTip.TryGetPose(out Pose mPose))
        {
            float pinchDist = Vector3.Distance(tPose.position, mPose.position);
            bool isMiddlePinching = pinchDist < 0.02f;

            if (isMiddlePinching && !wasMiddlePinching)
            {
                Vector3 spawnPosition = Vector3.Lerp(tPose.position, mPose.position, 0.5f);

                // Check if the area is clear before spawning
                if (IsSpawnAreaClear(spawnPosition))
                {
                    SpawnHat(spawnPosition);
                }
                else
                {
                    Debug.Log("Hat 2 spawn blocked: Hand is too close to an existing grabbable object!");
                }
            }

            wasMiddlePinching = isMiddlePinching;
        }
    }

    bool IsSpawnAreaClear(Vector3 checkPosition)
    {
        // Draw an invisible bubble at the pinch location
        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, noSpawnRadius);

        foreach (var hitCollider in hitColliders)
        {
            // If the collider (or its parent) has an XRGrabInteractable, it's a grabbable object!
            var grabbable = hitCollider.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            if (grabbable != null)
            {
                return false; // Area is NOT clear. Block the spawn.
            }
        }

        return true; // No grabbable objects found. Clear to spawn!
    }

    void SpawnHat(Vector3 position)
    {
        if (hat2Prefab != null)
        {
            Instantiate(hat2Prefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("You forgot to assign the Hat 2 Prefab in the Inspector!");
        }
    }
}