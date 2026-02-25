using UnityEngine;
using UnityEngine.XR.Hands;

public class PinchSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject cubePrefab;

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

        // Get the required joints
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        var middleTip = hand.GetJoint(XRHandJointID.MiddleTip); // Used to check for a fist
        var palm = hand.GetJoint(XRHandJointID.Palm);

        if (thumbTip.TryGetPose(out Pose tPose) &&
            indexTip.TryGetPose(out Pose iPose) &&
            middleTip.TryGetPose(out Pose mPose) &&
            palm.TryGetPose(out Pose pPose))
        {
            // 1. Check if thumb and index are touching (Pinch)
            float pinchDist = Vector3.Distance(tPose.position, iPose.position);
            bool fingersTouching = pinchDist < 0.02f;

            // 2. Check if the middle finger is extended away from the palm
            // (If it is less than ~0.06m, the hand is likely curled into a fist)
            float middleToPalmDist = Vector3.Distance(mPose.position, pPose.position);
            bool handIsOpen = middleToPalmDist > 0.06f;

            // 3. ONLY trigger if fingers are touching AND the hand is open
            bool isPinching = fingersTouching && handIsOpen;

            if (isPinching && !wasPinching)
            {
                Vector3 spawnPosition = Vector3.Lerp(tPose.position, iPose.position, 0.5f);
                SpawnCube(spawnPosition);
            }

            wasPinching = isPinching;
        }
    }

    void SpawnCube(Vector3 position)
    {
        GameObject newCube;

        if (cubePrefab != null)
        {
            newCube = Instantiate(cubePrefab, position, Quaternion.identity);
        }
        else
        {
            newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newCube.transform.position = position;
            newCube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            newCube.AddComponent<Rigidbody>();
        }
    }
}