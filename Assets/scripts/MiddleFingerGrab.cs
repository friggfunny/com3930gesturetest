using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

public class MiddleFingerGrab : MonoBehaviour
{
    private XRHandSubsystem handSubsystem;

    // Track state for both hands
    private bool isGrabbingLeft = false;
    private bool isGrabbingRight = false;

    // The actual objects we are currently holding
    private Rigidbody leftGrabbedObj;
    private Rigidbody rightGrabbedObj;

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

        // 2. Process both hands independently every frame
        ProcessHand(handSubsystem.leftHand, ref isGrabbingLeft, ref leftGrabbedObj);
        ProcessHand(handSubsystem.rightHand, ref isGrabbingRight, ref rightGrabbedObj);
    }

    void ProcessHand(XRHand hand, ref bool isGrabbing, ref Rigidbody grabbedObj)
    {
        if (!hand.isTracked)
        {
            // If we lose tracking, drop the object
            if (isGrabbing) DropObject(ref isGrabbing, ref grabbedObj);
            return;
        }

        // Get Thumb and Middle Finger tips
        var thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        var middle = hand.GetJoint(XRHandJointID.MiddleTip);

        if (thumb.TryGetPose(out Pose tPose) && middle.TryGetPose(out Pose mPose))
        {
            // Calculate distance between middle finger and thumb
            float dist = Vector3.Distance(tPose.position, mPose.position);
            bool isMiddlePinching = dist < 0.025f; // 2.5cm threshold

            Vector3 pinchPoint = Vector3.Lerp(tPose.position, mPose.position, 0.5f);

            // START GRAB
            if (isMiddlePinching && !isGrabbing)
            {
                // Create an invisible 5cm bubble at the pinch point to find objects
                Collider[] colliders = Physics.OverlapSphere(pinchPoint, 0.05f);
                foreach (var col in colliders)
                {
                    Rigidbody rb = col.attachedRigidbody;
                    // Only grab things that have physics (Rigidbodies)
                    if (rb != null && !rb.isKinematic)
                    {
                        GrabObject(rb, ref isGrabbing, ref grabbedObj);
                        break; // Only grab one object at a time
                    }
                }
            }
            // STOP GRAB
            else if (!isMiddlePinching && isGrabbing)
            {
                DropObject(ref isGrabbing, ref grabbedObj);
            }

            // WHILE GRABBING: Update the object's position to follow the fingers perfectly
            if (isGrabbing && grabbedObj != null)
            {
                grabbedObj.MovePosition(pinchPoint);
            }
        }
    }

    void GrabObject(Rigidbody rb, ref bool isGrabbing, ref Rigidbody grabbedObj)
    {
        grabbedObj = rb;
        grabbedObj.isKinematic = true; // Turn off gravity/physics while holding it
        grabbedObj.useGravity = false;
        isGrabbing = true;
    }

    void DropObject(ref bool isGrabbing, ref Rigidbody grabbedObj)
    {
        if (grabbedObj != null)
        {
            grabbedObj.isKinematic = false; // Turn physics back on so it falls
            grabbedObj.useGravity = true;
            grabbedObj = null;
        }
        isGrabbing = false;
    }
}