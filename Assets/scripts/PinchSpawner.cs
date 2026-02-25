using UnityEngine;
using UnityEngine.XR.Hands;

public class PinchSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("Leave empty to spawn default cubes, or drag a Prefab here.")]
    public GameObject cubePrefab;

    private XRHandSubsystem handSubsystem;

    // We track the previous frame's state to prevent spawning 60 cubes a second
    private bool leftWasPinching = false;
    private bool rightWasPinching = false;

    void Update()
    {
        // 1. Hook up to the Hand Subsystem
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0) handSubsystem = subsystems[0];
            return;
        }

        // 2. Check both hands every frame
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

        // Get the joints
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);

        if (thumbTip.TryGetPose(out Pose tPose) && indexTip.TryGetPose(out Pose iPose))
        {
            // Calculate distance
            float pinchDist = Vector3.Distance(tPose.position, iPose.position);
            bool isPinching = pinchDist < 0.02f;

            // Trigger ONLY on the exact frame the pinch starts
            if (isPinching && !wasPinching)
            {
                // Spawn the cube exactly between the thumb and index finger
                Vector3 spawnPosition = Vector3.Lerp(tPose.position, iPose.position, 0.5f);
                SpawnCube(spawnPosition);
            }

            // Update the state for the next frame
            wasPinching = isPinching;
        }
    }

    void SpawnCube(Vector3 position)
    {
        GameObject newCube;

        if (cubePrefab != null)
        {
            // Spawn the custom prefab if you assigned one
            newCube = Instantiate(cubePrefab, position, Quaternion.identity);
        }
        else
        {
            // Fallback: Create a basic Unity cube, make it small, and add gravity
            newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newCube.transform.position = position;
            newCube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); // 5cm cube
            newCube.AddComponent<Rigidbody>(); // Makes it fall down
        }
    }
}