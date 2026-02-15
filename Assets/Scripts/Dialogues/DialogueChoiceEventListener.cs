using UnityEngine;

public class DialogueChoiceEventListener : MonoBehaviour
{
    [SerializeField] private DialogueChoiceEventChannelSO channel;

    [Header("Refs")]
    [SerializeField] private InventoryManager inventory;

    [SerializeField] private ReportUI reportUI;

    [SerializeField] private DialogueUI dialogueUI;

    [Header("Audio receptionist (injected)")]
    [SerializeField] private AudioClip clipThreat;
    [SerializeField] private AudioClip clipFalseAlarm;




    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (reportUI == null) reportUI = FindFirstObjectByType<ReportUI>();

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<DialogueUI>();

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

                break;

            case "remove_selected_item":
                RemoveSelectedItem();
                break;

            case "deliver_m2":
                TryDeliver(missionTag: "m2", objectiveId: "m2_deliver_items");
                break;

            case "deliver_m3":
                TryDeliver(missionTag: "m3", objectiveId: "m3_deliver_items");
                break;

            case "conclude_talk_receptionist":
                MissionManager.Instance.CompleteObjective("m3_talk_receptionist");
                break;

            case "stay_server":
                MissionTracker.Instance.Set(ReportCheck.IntruderKicked, false);
                MissionManager.Instance.CompleteObjective("m3_inspect_server");
                break;

            case "kick_server":
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
            return; // se non consegni nulla, non dice niente
        }

        InventoryItem selected = inventory.GetSelectedItem();
        bool removed = inventory.RemoveItem();

        if (removed)
            Debug.Log("[RemoveSelected] Oggetto rimosso dall'inventario.");
        else
            Debug.Log("[RemoveSelected] Oggetto NON removibile (es: badge).");

        MarkReportForRemovedItem(selected);

        // Se ho consegnato davvero, la receptionist commenta subito
        if (removed && dialogueUI != null && selected != null)
        {
            string line = GetReceptionistLineForItem(selected.id);

            if (!string.IsNullOrWhiteSpace(line))
            {
                AudioClip clip = GetReceptionistClipForItem(selected.id);
                dialogueUI.EnqueueInjectedLine(line, clip);
            }
        }

    }

    private AudioClip GetReceptionistClipForItem(string itemId)
    {
        bool isThreat =
            itemId == "badgeTecnico" ||
            itemId == "post-it" ||
            itemId == "hardDisk";

        bool isFalseAlarm =
            itemId == "manuale" ||
            itemId == "cacciavite" ||
            itemId == "cuffie";

        if (isThreat) return clipThreat;
        if (isFalseAlarm) return clipFalseAlarm;
        return null;
    }


    private string GetReceptionistLineForItem(string itemId)
    {
        // “vera vulnerabilità / minaccia”
        bool isThreat =
            itemId == "badgeTecnico" ||
            itemId == "post-it" ||
            itemId == "hardDisk"; // usb

        // “falso allarme” (come richiesto dal prof)
        bool isFalseAlarm =
            itemId == "manuale" ||
            itemId == "cacciavite" ||
            itemId == "cuffie";

        if (isThreat)
            return "Capisco. Questo oggetto potrebbe davvero rappresentare una minaccia: lo consegnerò subito a chi di dovere.";

        if (isFalseAlarm)
            return "Capisco. Da una prima verifica non sembra un oggetto sospetto: lo registrerò comunque e ti farò sapere.";

        // se è un oggetto che non vuoi commentare
        return null;
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

        var mm = MissionManager.Instance;
        Debug.Log("[Deliver] MissionManager=" + (mm ? "OK" : "NULL"));
        if (mm == null) return;



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



        int missionIndex = mm.CurrentMissionIndex; // missione attiva al momento della consegna
        reportUI.OpenReportDelayed(missionIndex);

        Debug.Log($"[Deliver] Apro report missionIndex={missionIndex} (removed={removed})");
    }


}
