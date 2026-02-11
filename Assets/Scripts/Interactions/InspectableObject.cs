using UnityEngine;

public class InspectableObject : MonoBehaviour, IInteractable
{
    [Header("Dati oggetto")]
    public string objectName = "Oggetto";
    public Sprite inspectImage;
    public Sprite inventoryIcon;
    public string itemId = "item_default";
    public bool objectremovable = false;

    [Tooltip("Esempio: m2 oppure m3 (puoi anche mettere m2_something)")]
    public string missionTag;

    [Header("Mission Objective (opzionale)")]
    [Tooltip("ID dell'obiettivo da aggiornare quando ispezioni (es: m3_inspect_relax)")]
    [SerializeField] private string objectiveIdOnInspect = "";

    [Tooltip("Quanto avanzare (solo per m3). Di solito 1")]
    [SerializeField] private int advanceAmount = 1;

    [Header("Prefab 3D per ispezione")]
    public GameObject inspectPrefab;

    private bool alreadyReported = false; // evita doppio completamento/avanzamento

    public string GetInteractionText() => $"Ispeziona {objectName}";

    public void Interact(PlayerInteractor interactor)
    {
        InspectUI inspectUI = interactor.inspectUI;

        if (inspectUI == null)
        {
            Debug.LogWarning("Nessuna InspectUI assegnata al PlayerInteractor.");
            return;
        }

        // Apri UI ispezione
        inspectUI.Open(this);

        // Mission logic
        var mm = MissionManager.Instance;
        if (mm == null) return;

        if (alreadyReported) return;
        if (string.IsNullOrWhiteSpace(objectiveIdOnInspect)) return;

        if (!mm.IsObjectiveVisible(objectiveIdOnInspect)) return;
        if (mm.IsObjectiveCompleted(objectiveIdOnInspect)) return;

        // Regola richiesta: m2 = complete, m3 = advance
        if (!string.IsNullOrWhiteSpace(missionTag) && missionTag.StartsWith("m2"))
        {
            alreadyReported = true;
            mm.CompleteObjective(objectiveIdOnInspect);
            Debug.Log($"[Inspectable] COMPLETE (tag={missionTag}) -> {objectiveIdOnInspect}");
        }
        else if (!string.IsNullOrWhiteSpace(missionTag) && missionTag.StartsWith("m3"))
        {
            alreadyReported = true;
            mm.AdvanceObjective(objectiveIdOnInspect, advanceAmount);
            Debug.Log($"[Inspectable] ADVANCE +{advanceAmount} (tag={missionTag}) -> {objectiveIdOnInspect}");
        }
        else
        {
            // opzionale: se tag non è m2/m3 non fai nulla
            // Debug.Log($"[Inspectable] Nessuna regola mission per tag={missionTag}");
        }
    }

    public InventoryItem ToInventoryItem()
    {
        return new InventoryItem
        {
            id = itemId,
            displayName = objectName,
            icon = inventoryIcon,
            removable = objectremovable,
            inspectPrefab = inspectPrefab != null ? inspectPrefab : gameObject,
            missionTag = missionTag
        };
    }
}