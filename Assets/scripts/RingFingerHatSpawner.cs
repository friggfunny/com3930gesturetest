using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
 // Needed to detect grabbable objects

public class RingFingerHatSpawner : MonoBehaviour
{
    [Header("Hat Settings")]
    [Tooltip("Drag your TestHat Prefab here.")]
    public GameObject hatPrefab;

    [Header("Spawn Prevention")]
    [Tooltip("How close your hand can be to an existing object before spawning is blocked (in meters).")]
    public float noSpawnRadius = 0.15f; // 15cm radius

    private XRHandSubsystem handSubsystem;
    private bool wasRingPinching = false;

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        CheckLeftHandForRingPinch(handSubsystem.leftHand);
    }

    void CheckLeftHandForRingPinch(XRHand hand)
    {
        if (!hand.isTracked)
        {
            wasRingPinching = false;
            return;
        }

        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var ringTip = hand.GetJoint(XRHandJointID.RingTip);

        if (thumbTip.TryGetPose(out Pose tPose) && ringTip.TryGetPose(out Pose rPose))
        {
            float pinchDist = Vector3.Distance(tPose.position, rPose.position);
            bool isRingPinching = pinchDist < 0.02f;

            if (isRingPinching && !wasRingPinching)
            {
                Vector3 spawnPosition = Vector3.Lerp(tPose.position, rPose.position, 0.5f);

                // NEW: Check if the area is clear before spawning
                if (IsSpawnAreaClear(spawnPosition))
                {
                    SpawnHat(spawnPosition);
                }
                else
                {
                    Debug.Log("Hat spawn blocked: Hand is too close to an existing grabbable object!");
                }
            }

            wasRingPinching = isRingPinching;
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
        if (hatPrefab != null)
        {
            Instantiate(hatPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("You forgot to assign the Hat Prefab in the Inspector!");
        }
    }
}