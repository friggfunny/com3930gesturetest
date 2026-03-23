using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HeadBreeder : MonoBehaviour
{
    [Header("Baby Setup")]
    public GameObject babyHeadPrefab;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor hatSocket;

    private float breedCooldown = 2.0f;
    private float nextBreedTime = 0f;

    void Start()
    {
        // Find the grab component on this head
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Find the socket hiding in the child object
        hatSocket = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextBreedTime) return;

        // 1. Did we hit another Caesar head?
        HeadBreeder otherHead = collision.gameObject.GetComponent<HeadBreeder>();
        if (otherHead != null)
        {
            // 2. Are BOTH heads currently being held by the player?
            if (this.grabInteractable.isSelected && otherHead.grabInteractable.isSelected)
            {
                // 3. Do BOTH heads have a hat sitting in their socket?
                if (this.HasHat() && otherHead.HasHat())
                {
                    // Breed! Spawn the baby at the exact point they crashed into each other
                    SpawnBaby(collision.contacts[0].point);

                    // Trigger cooldown so they don't spawn 500 babies instantly
                    nextBreedTime = Time.time + breedCooldown;
                    otherHead.nextBreedTime = Time.time + breedCooldown;
                }
            }
        }
    }

    bool HasHat()
    {
        // Checks if the socket exists and if something is currently snapped into it
        return hatSocket != null && hatSocket.hasSelection;
    }

    void SpawnBaby(Vector3 position)
    {
        Instantiate(babyHeadPrefab, position, Quaternion.identity);
    }
}