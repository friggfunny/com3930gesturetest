using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HatCollisionHandler : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Collider[] hatColliders;

    void Awake()
    {
        // Grab the components off the hat
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        hatColliders = GetComponentsInChildren<Collider>();
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // If the object grabbing this hat is a Socket (the Caesar Head)
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
        {
            SetColliders(false); // Turn off physics!
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // When removed from the socket, turn physics back on so it can hit the floor/table
        SetColliders(true);
    }

    private void SetColliders(bool state)
    {
        foreach (Collider col in hatColliders)
        {
            col.enabled = state;
        }
    }
}