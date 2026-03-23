using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class PinkyPinchCaesarSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject caesarPrefab;
    public float spawnCooldown = 1.0f;

    [Header("Pinch Settings")]
    [Tooltip("Distance between pinky and thumb to trigger")]
    public float pinchThreshold = 0.03f;

    private XRHandSubsystem handSubsystem;
    private float nextSpawnTime = 0f;

    void Update()
    {
        if (Time.time < nextSpawnTime) return;

        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        // Check both hands
        CheckHandPinch(handSubsystem.leftHand);
        CheckHandPinch(handSubsystem.rightHand);
    }

    void CheckHandPinch(XRHand hand)
    {
        if (!hand.isTracked) return;

        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var pinkyTip = hand.GetJoint(XRHandJointID.LittleTip);

        if (thumbTip.TryGetPose(out Pose tPose) && pinkyTip.TryGetPose(out Pose pPose))
        {
            float distance = Vector3.Distance(tPose.position, pPose.position);

            if (distance < pinchThreshold)
            {
                SpawnCaesar(tPose.position);
            }
        }
    }

    void SpawnCaesar(Vector3 position)
    {
        if (caesarPrefab != null)
        {
            Instantiate(caesarPrefab, position, Quaternion.identity);
            nextSpawnTime = Time.time + spawnCooldown;
            Debug.Log("Caesar Head Spawned!");
        }
    }
}