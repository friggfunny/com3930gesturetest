using UnityEngine;

public class ClapSpawner : MonoBehaviour
{
    [Header("Hand Setup")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Bust Setup")]
    public GameObject caesarPrefab;
    public float clapDistance = 0.15f; // How close hands must be to count as a clap

    private bool wasClapping = false;

    void Update()
    {
        // Measure the distance between the center of your hands
        float dist = Vector3.Distance(leftHand.position, rightHand.position);
        bool isClapping = dist < clapDistance;

        if (isClapping && !wasClapping)
        {
            // Spawn exactly halfway between your hands
            Vector3 spawnPos = (leftHand.position + rightHand.position) / 2f;
            Instantiate(caesarPrefab, spawnPos, Quaternion.identity);
        }

        wasClapping = isClapping;
    }
}