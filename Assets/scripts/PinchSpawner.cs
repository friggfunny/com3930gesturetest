using UnityEngine;
using UnityEngine.XR.Hands;
 // Needed to detect grabbable objects

public class PinchSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject cubePrefab;

    [Header("Spawn Prevention")]
    [Tooltip("How close your hand can be to an existing object before spawning is blocked (in meters).")]
    public float noSpawnRadius = 0.15f; // 15cm radius

    private XRHandSubsystem handSubsystem;
    private bool leftWasPinching = false;
    private bool rightWasPinching = false;

    void Update()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        CheckHandForPinch(handSubsystem.leftHand, ref leftWasPinching);
        CheckHandForPinch(handSubsystem.rightHand, ref rightWasPinching);
    }

    void CheckHandForPinch(XRHand hand, ref bool wasPinching)
    {
        if (!hand.isTracked)
        {
            wasPinching = false;
            return;
        }

        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip);
        var palm = hand.GetJoint(XRHandJointID.Palm);

        if (thumbTip.TryGetPose(out Pose tPose) &&
            indexTip.TryGetPose(out Pose iPose) &&
            middleTip.TryGetPose(out Pose mPose) &&
            palm.TryGetPose(out Pose pPose))
        {
            float pinchDist = Vector3.Distance(tPose.position, iPose.position);
            bool fingersTouching = pinchDist < 0.02f;

            float middleToPalmDist = Vector3.Distance(mPose.position, pPose.position);
            bool handIsOpen = middleToPalmDist > 0.06f;

            bool isPinching = fingersTouching && handIsOpen;

            if (isPinching && !wasPinching)
            {
                Vector3 spawnPosition = Vector3.Lerp(tPose.position, iPose.position, 0.5f);

                // NEW: Check if the area is clear before spawning
                if (IsSpawnAreaClear(spawnPosition))
                {
                    SpawnCube(spawnPosition);
                }
                else
                {
                    Debug.Log("Spawn blocked: Hand is too close to an existing grabbable object!");
                }
            }

            wasPinching = isPinching;
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
                return false; // The area is NOT clear. Block the spawn.
            }
        }

        return true; // No grabbable objects found. Clear to spawn!
    }

    void SpawnCube(Vector3 position)
    {
        if (cubePrefab != null)
        {
            Instantiate(cubePrefab, position, Quaternion.identity);
        }
    }
}