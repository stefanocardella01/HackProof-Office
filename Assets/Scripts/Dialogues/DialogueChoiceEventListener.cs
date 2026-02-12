using UnityEngine;

public class DialogueChoiceEventListener : MonoBehaviour
{
    [SerializeField] private DialogueChoiceEventChannelSO channel;

    [Header("Refs")]
    [SerializeField] private InventoryManager inventory;

    [SerializeField] private ReportUI reportUI;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (reportUI == null) reportUI = FindFirstObjectByType<ReportUI>();
    }

    private void OnEnable()
    {
        if (channel == null)
        {
            Debug.LogError("[Listener] Channel NULL nel listener!");
            return;
        }

        Debug.Log("[Listener] ON. Channel instanceID=" + channel.GetInstanceID());
        channel.OnEventRaised += Handle;
    }

    private void OnDisable()
    {
        if (channel != null) channel.OnEventRaised -= Handle;
    }

    private void Handle(string eventId)
    {
        Debug.Log("[DialogueEvent] " + eventId);

        Debug.Log("[Listener] RICEVUTO eventId=" + eventId);

        switch (eventId)
        {
            case "open_email_minigame":
                // Qui puoi: abilitare PC Email / aprire UI / ecc.
                // (meglio: chiama un tuo FlowController)
                break;

            case "remove_selected_item":
                RemoveSelectedItem();
                break;

            case "deliver_m2":
                Debug.LogError("### SONO NEL CASE deliver_m2 ###");
                TryDeliver(missionTag: "m2", objectiveId: "m2_deliver_items");
                break;

            case "deliver_m3":
                Debug.LogError("### SONO NEL CASE deliver_m3 ###");
                TryDeliver(missionTag: "m3", objectiveId: "m3_deliver_items");
                break;

            case "conclude_talk_receptionist":
                Debug.LogError("Provo a concludere dal listener");
                MissionManager.Instance.CompleteObjective("m3_talk_receptionist");
                break;

            case "stay_server":
                Debug.LogError("Faccio rimanere l'intruso e completo sala server");
                MissionTracker.Instance.Set(ReportCheck.IntruderKicked, false);
                MissionManager.Instance.CompleteObjective("m3_inspect_server");
                break;

            case "kick_server":
                Debug.LogError("Caccio l'intruso e completo sala server");
                MissionTracker.Instance.Set(ReportCheck.IntruderKicked, true);
                MissionManager.Instance.CompleteObjective("m3_inspect_server");
                break;

            case "giulio_talk":
                MissionManager.Instance.CompleteObjective("m4_go_to_giulio");
                break;
        }
    }

    private void RemoveSelectedItem()
    {
        if (inventory == null) return;

        if (!inventory.HasSelectedItem())
        {
            Debug.Log("[RemoveSelected] Nessun oggetto selezionato.");
            return;
        }
        InventoryItem selected = inventory.GetSelectedItem();
        bool removed = inventory.RemoveItem();

        if (removed)
            Debug.Log("[RemoveSelected] Oggetto rimosso dall'inventario.");
        else
            Debug.Log("[RemoveSelected] Oggetto NON removibile (es: badge).");

        MarkReportForRemovedItem(selected);
    }

    private void MarkReportForRemovedItem(InventoryItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Report] item null, non posso segnare nulla.");
            return;
        }

        if (MissionTracker.Instance == null)
        {
            Debug.LogError("[Report] MissionTracker.Instance NULL (metti MissionTracker in scena).");
            return;
        }

        // SCEGLI UN CRITERIO STABILE: item.id (consigliato)
        // Assicurati che negli InspectableObject tu abbia itemId coerenti:
        // es: "postit", "headphones", "usb"

        switch (item.id)
        {
            case "post-it":
                MissionTracker.Instance.Set(ReportCheck.PostItDelivered, true);
                Debug.Log("[Report] Set PostItDelivered = true");
                break;

            case "cuffie":
                MissionTracker.Instance.Set(ReportCheck.HeadphonesDelivered, true);
                Debug.Log("[Report] Set HeadphonesDelivered = true");
                break;

            case "hardDisk":
                MissionTracker.Instance.Set(ReportCheck.UsbDelivered, true);
                Debug.Log("[Report] Set UsbDelivered = true");
                break;

            // Missione 3 esempi (se vuoi):
            case "badgeTecnico":
                MissionTracker.Instance.Set(ReportCheck.BadgeDelivered, true);
                break;

            case "manuale":
                MissionTracker.Instance.Set(ReportCheck.ManualDelivered, true);
                break;

            case "cacciavite":
                MissionTracker.Instance.Set(ReportCheck.ScrewdriverDelivered, true);
                break;

            default:
                Debug.Log("[Report] Nessuna regola per item.id=" + item.id);
                break;
        }
    }

    private void TryDeliver(string missionTag, string objectiveId)
    {
        Debug.LogError("### ENTRO IN TryDeliver ###");

        var mm = MissionManager.Instance;
        Debug.Log("[Deliver] MissionManager=" + (mm ? "OK" : "NULL"));
        if (mm == null) return;

        //bool vis = mm.IsObjectiveVisible(objectiveId);
        //bool comp = mm.IsObjectiveCompleted(objectiveId);
        //Debug.Log($"[Deliver] objectiveId={objectiveId} visible={vis} completed={comp}");

        //if (!vis || comp)
        //{
        //    Debug.LogWarning($"[Deliver] BLOCCATO dal gating: visible={vis} completed={comp}");
        //    return;
        //}

        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        Debug.Log("[Deliver] Inventory=" + (inventory ? inventory.name : "NULL"));
        if (inventory == null)
        {
            Debug.LogError("[Deliver] InventoryManager NON trovato");
            return;
        }

        int countBefore = inventory.CountItemsByMissionTag(missionTag);
        Debug.Log($"[Deliver] countBefore(tag={missionTag})={countBefore}");

        int removed = inventory.RemoveItemsByMissionTag(missionTag);
        Debug.Log($"[Deliver] removed(tag={missionTag})={removed}");

        //if (removed <= 0)
        //{
        //    Debug.LogWarning("[Deliver] Nessun item rimosso, quindi NON completo l'obiettivo.");
        //    return;
        //}

        int missionIndex = mm.CurrentMissionIndex; // missione attiva al momento della consegna
        reportUI.OpenReportDelayed(missionIndex);

        Debug.Log($"[Deliver] Apro report missionIndex={missionIndex} (removed={removed})");
    }


}
