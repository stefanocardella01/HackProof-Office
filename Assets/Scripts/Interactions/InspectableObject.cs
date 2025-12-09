using UnityEngine;

public class InspectableObject : MonoBehaviour, IInteractable
{
    [Header("Dati oggetto")]
    public string objectName = "Oggetto"; //es: "Post-it"
    public Sprite inspectImage;  //Immagine mostrata a schermo intero nella UI di ispeziona
    public Sprite inventoryIcon; //Icona per lo slot dell'inventario
    public string itemId = "item_default"; //id logico

    [Header("Prefab 3D per ispezione")]
    public GameObject inspectPrefab;

    public string GetInteractionText()
    {
        return $"Ispeziona {objectName}";

    }

    public void Interact(PlayerInteractor interactor)
    {

        InspectUI inspectUI = interactor.inspectUI; // prendo la UI dal Player

        if(inspectUI != null)
        {

            inspectUI.Open(this);

        }
        else
        {
            Debug.LogWarning("Nessuna InspectUI assegnata al PlayerInteractor.");
        }
    }

    // Metodo helper per creare il dato da mettere nell'inventario
    public InventoryItem ToInventoryItem()
    {
        return new InventoryItem
        {
            id = itemId,
            displayName = objectName,
            icon = inventoryIcon,
            inspectPrefab = inspectPrefab != null ? inspectPrefab : gameObject
        };
    }
}
