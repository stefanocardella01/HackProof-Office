using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField] private Door door;
    [SerializeField] private float openRotation = 90f;

    [Header("Interaction Requirements")]
    [SerializeField] private bool requiresBadge = false;
    [SerializeField] private bool badgeAcquired = false;

    [Header("Auto Open / Close Settings")]
    [SerializeField] private bool autoOpen = true;
    [SerializeField] private bool autoClose = true;

    // Tiene traccia SOLO degli attori unici (Player / NPC)
    private HashSet<Transform> actorsInside = new HashSet<Transform>();

    public string GetInteractionText()
    {
        if (requiresBadge && !badgeAcquired)
            return "Serve un badge";
        if(requiresBadge && badgeAcquired)
            return "Usa Badge per aprire la porta";

        return door.IsOpen ? "Chiudi Porta" : "Apri Porta";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (requiresBadge && !badgeAcquired)
        {
            Debug.Log("Richiede un badge!");
            return;
        }

        door.ToggleDoor(openRotation); // sempre stesso lato
    }

    public void SetBadgeAcquired(bool value)
    {
        badgeAcquired = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoOpen) return;

        Transform root = other.transform.root;
        if (!IsValidActor(root)) return;

        bool wasEmpty = actorsInside.Count == 0;
        actorsInside.Add(root);

        Debug.Log($"ENTER: {root.name}");

        if (wasEmpty && !door.IsOpen)
            door.OpenDoor(openRotation); // 🔹 sempre stesso lato
    }

    private void OnTriggerExit(Collider other)
    {
        if (!autoClose) return;

        Transform root = other.transform.root;
        if (!IsValidActor(root)) return;

        actorsInside.Remove(root);

        Debug.Log($"EXIT: {root.name}");

        // Se nessun attore è rimasto dentro → chiudi
        if (actorsInside.Count == 0 && door.IsOpen)
            door.CloseDoor();
    }

    private bool IsValidActor(Transform root)
    {
        return root.CompareTag("Player") || root.CompareTag("NPC");
    }
}
