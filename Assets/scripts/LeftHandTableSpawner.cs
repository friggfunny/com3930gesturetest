using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class LeftHandTableSpawner : MonoBehaviour
{
    [Header("Table Prefab")]
    public GameObject tablePrefab;
    public float dropDistance = 0.4f;
    public Vector3 tableRotationOffset = new Vector3(0, -90f, 0);
    public float spawnCooldown = 2.0f;

    [Header("Squeeze Detection")]
    [Tooltip("Distance between pinky tip and wrist to trigger spawn")]
    public float squeezeDistanceThreshold = 0.08f;

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

        XRHand leftHand = handSubsystem.leftHand;
        if (!leftHand.isTracked) return;

        // Get the Pinky Tip and the Wrist
        var pinkyTip = leftHand.GetJoint(XRHandJointID.LittleTip);
        var wrist = leftHand.GetJoint(XRHandJointID.Wrist);

        if (pinkyTip.TryGetPose(out Pose pinkyPose) && wrist.TryGetPose(out Pose wristPose))
        {
            float dist = Vector3.Distance(pinkyPose.position, wristPose.position);

            // If pinky is pulled into the palm (Squeezing)
            if (dist < squeezeDistanceThreshold)
            {
                SpawnTable(wristPose.position);
            }
        }
    }

    void SpawnTable(Vector3 handPos)
    {
        Vector3 spawnPos = handPos;
        spawnPos.y -= dropDistance;

        if (tablePrefab != null)
        {
            Instantiate(tablePrefab, spawnPos, Quaternion.Euler(tableRotationOffset));
            nextSpawnTime = Time.time + spawnCooldown;
            Debug.Log("Table Spawned!");
        }
    }
}