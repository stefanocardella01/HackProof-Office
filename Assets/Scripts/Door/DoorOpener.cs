using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField] private Door door;
    [SerializeField] private float openRotation = 90f;

    [Header("Inventory")]
    [SerializeField] private InventoryManager inventory;   // assegna in inspector o auto-find

    [Header("Interaction Requirements")]
    [SerializeField] private bool requiresBadge = false;
    [SerializeField] private bool badgeAcquired = false;

    [Header("Auto Open / Close Settings")]
    [SerializeField] private bool autoOpen = true;
    [SerializeField] private bool autoClose = true;

    private HashSet<Transform> actorsInside = new HashSet<Transform>();

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();
    }


    private bool HasBadgeSelected()
    {
        if (inventory == null) return false;

        var item = inventory.GetSelectedItem();
        if (item == null) return false;

        if(item.id == "badgePersonale")
        {
            return true;
        }

        return false;
        // return item.itemType == ItemType.Badge;
    }

    public string GetInteractionText()
    {
        if (requiresBadge)
            return HasBadgeSelected() ? "Usa badge per aprire" : "Serve un badge";

        return door.IsOpen ? "Chiudi Porta" : "Apri Porta";
    }


    public void Interact(PlayerInteractor interactor)
    {
        if (requiresBadge && !HasBadgeSelected())
        {
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


        if (wasEmpty && !door.IsOpen)
        {
            if (requiresBadge && !HasBadgeSelected())
                return;

            door.OpenDoor(openRotation); // sempre stesso lato
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (!autoClose) return;

        Transform root = other.transform.root;
        if (!IsValidActor(root)) return;

        actorsInside.Remove(root);


        // Se nessun attore è rimasto dentro chiudi
        if (actorsInside.Count == 0 && door.IsOpen)
            door.CloseDoor();
    }

    private bool IsValidActor(Transform root)
    {
        return root.CompareTag("Player") || root.CompareTag("NPC");
    }

    public bool Badge()
    {
        return requiresBadge;
    } 
}
