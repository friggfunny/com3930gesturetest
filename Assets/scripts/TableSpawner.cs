using UnityEngine;
using UnityEngine.InputSystem;

public class TableSpawner : MonoBehaviour
{
    [Header("Hand Setup")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Input Setup (To check if hands are open)")]
    public InputActionReference leftGrip;
    public InputActionReference rightGrip;

    [Header("Table Setup")]
    public GameObject tablePrefab;
    public float heightTolerance = 0.15f; // How close in height hands must be
    public float dropDistance = 0.2f;     // How far below hands to spawn table

    private float cooldown = 2.0f;        // Stop from spawning 100 tables a second
    private float nextSpawnTime = 0f;

    void Update()
    {
        if (Time.time < nextSpawnTime) return;

        // 1. Check if both hands are open (not pressing grip)
        float leftGripValue = leftGrip.action.ReadValue<float>();
        float rightGripValue = rightGrip.action.ReadValue<float>();

        if (leftGripValue > 0.1f || rightGripValue > 0.1f) return; // Hands are closed, abort!

        // 2. Check if hands are at the same level
        float heightDiff = Mathf.Abs(leftHand.position.y - rightHand.position.y);

        if (heightDiff < heightTolerance)
        {
            // 3. Check if palms are facing down. 
            // Depending on your specific hand models, "Up" might be the back of the hand.
            // If the back of the hand faces the sky, the palm faces the floor.
            float leftPalmDown = Vector3.Dot(leftHand.up, Vector3.up);
            float rightPalmDown = Vector3.Dot(rightHand.up, Vector3.up);

            // If dot product is > 0.5, it means it's mostly pointing upwards.
            if (leftPalmDown > 0.5f && rightPalmDown > 0.5f)
            {
                SpawnTable();
                nextSpawnTime = Time.time + cooldown;
            }
        }
    }

    void SpawnTable()
    {
        // Find the middle point between both hands
        Vector3 spawnPos = (leftHand.position + rightHand.position) / 2f;

        // Drop it slightly below the hands
        spawnPos.y -= dropDistance;

        // Spawn it!
        Instantiate(tablePrefab, spawnPos, Quaternion.identity);
    }
}