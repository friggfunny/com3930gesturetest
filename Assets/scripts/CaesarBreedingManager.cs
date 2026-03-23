using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CaesarBreedingManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject babyCaesarPrefab;
    public string caesarTag = "Caesar";

    [Header("Settings")]
    public float touchDistance = 0.15f;
    public float breedingCooldown = 2.0f;

    private float nextBreedingTime = 0f;

    void Update()
    {
        if (Time.time < nextBreedingTime) return;

        // Get everything tagged Caesar
        GameObject[] caesars = GameObject.FindGameObjectsWithTag(caesarTag);

        for (int i = 0; i < caesars.Length; i++)
        {
            for (int j = i + 1; j < caesars.Length; j++)
            {
                // Is Caesar A being held?
                var grabA = caesars[i].GetComponent<XRGrabInteractable>();
                // Is Caesar B being held?
                var grabB = caesars[j].GetComponent<XRGrabInteractable>();

                if (grabA != null && grabA.isSelected && grabB != null && grabB.isSelected)
                {
                    float dist = Vector3.Distance(caesars[i].transform.position, caesars[j].transform.position);

                    if (dist < touchDistance)
                    {
                        SpawnBaby(caesars[i].transform.position, caesars[j].transform.position);
                        return;
                    }
                }
            }
        }
    }

    void SpawnBaby(Vector3 posA, Vector3 posB)
    {
        Vector3 spawnPos = (posA + posB) / 2f;
        if (babyCaesarPrefab != null)
        {
            Instantiate(babyCaesarPrefab, spawnPos, Quaternion.identity);
            nextBreedingTime = Time.time + breedingCooldown;
        }
    }
}