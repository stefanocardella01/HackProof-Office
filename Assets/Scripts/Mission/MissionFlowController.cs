using System.Xml;
using UnityEngine;

public class MissionFlowController : MonoBehaviour
{
    [Header("Refs - Devices / PCs")]
    [SerializeField] private GameObject smartphoneRoot;                  // Canvas_Smartphone o root smartphone
    [SerializeField] private ComputerInteractable pcLogin;               // PC login (Missione 1)
    [SerializeField] private EmailComputerInteractable pcEmail;          // PC email (Missione 4)
    [SerializeField] private GameObject badgePickupRoot; // il badge "da trovare" in scena

    [Header("Refs - NPC")]
    [SerializeField] private NPCInteractable receptionist1;              // receptionist missione 1
    [SerializeField] private NPCInteractable paoloNpc;                   // npc missione 2
    [SerializeField] private NPCInteractable intrusoNpc;                 // npc missione 3 (se serve)
    [SerializeField] private NPCInteractable giulioNpc;                  // npc missione 4
    [SerializeField] private GameObject intrusoNpcRoot; // trascina qui il GO del personaggio

    [Header("Refs - Conversations (optional, if you enable/disable GO)")]
    [SerializeField] private GameObject conversazioneReceptionist1GO;
    [SerializeField] private GameObject conversazioneReceptionPostIspezioniGO;
    [SerializeField] private GameObject conversazioneReception2GO;

    [Header("Receptionist Conversations")]
    [SerializeField] private DialogueConversation convReceptionist1;
    [SerializeField] private DialogueConversation convReceptionistPostIspezioni2;
    [SerializeField] private DialogueConversation convReceptionistPostIspezioni3;
    [SerializeField] private DialogueConversation convReceptionist2;

    [Header("Refs - Pickups / Objects")]
    [SerializeField] private GameObject[] mission2Objects;               // i 3 oggetti M2 in scena
    [SerializeField] private GameObject[] mission3Objects;               // i 3 oggetti M3 in scena

    [Header("Refs - FX / Waypoints")]
    [SerializeField] private GameObject fxPlayerDesk;
    [SerializeField] private GameObject fxPaoloDesk;
    [SerializeField] private GameObject fxGiulio;

    [Header("Objective IDs (set in inspector)")]
    [SerializeField] private string obj_m1_talk_receptionist = "m1_talk_receptionist";
    [SerializeField] private string obj_m1_go_desk = "m1_go_to_desk";
    [SerializeField] private string obj_m1_login_done = "m1_login_done";

    [SerializeField] private string obj_m2_talk_paolo = "m2_go_to_paolo";
    [SerializeField] private string obj_m2_inspect_postit = "m2_inspect_postit";
    [SerializeField] private string obj_m2_inspect_headphones = "m2_inspect_headphones";
    [SerializeField] private string obj_m2_inspect_usb = "m2_inspect_usb";

    [SerializeField] private string obj_m2_deliver_items = "m2_deliver_items";

    [SerializeField] private string obj_m3_talk_receptionist = "m3_talk_receptionist";
    [SerializeField] private string obj_m3_inspect_relax = "m3_inspect_relax";
    [SerializeField] private string obj_m3_inspect_server = "m3_inspect_server";
    [SerializeField] private string obj_m3_deliver_items = "m3_deliver_items";

    [SerializeField] private string obj_m4_talk_giulio = "m4_go_to_giulio";
    [SerializeField] private string obj_m4_check_emails = "m4_check_emails";

    private MissionManager mm;

    private bool m3InspectServerDone = false;
    private bool m3InspectRelaxDone = false;

    private bool m2Inspectpostit = false;
    private bool m2Inspectheadphones = false;
    private bool m2Inspectusb = false;

    private void Awake()
    {
        Debug.Log("[MissionFlow] AWAKE on " + gameObject.name);
    }


    private void Start()
    {
        mm = MissionManager.Instance;
        if (mm == null)
        {
            Debug.LogError("[MissionFlow] MissionManager.Instance NULL. Metti MissionManager in scena.");
            return;
        }

        mm.OnMissionStarted += OnMissionStarted;
        mm.OnObjectiveCompleted += OnObjectiveCompleted;
        mm.OnMissionCompleted += OnMissionCompleted;

        // Se la missione 1 parte automaticamente, OnMissionStarted arriverà da solo.
        // Se invece la lanci manualmente, puoi chiamare Setup per sicurezza:
        if (mm.CurrentMissionIndex == 0 && mm.IsMissionActive)
            SetupMission1Start();
    }

    private void OnDestroy()
    {
        if (mm == null) return;

        mm.OnMissionStarted -= OnMissionStarted;
        mm.OnObjectiveCompleted -= OnObjectiveCompleted;
        mm.OnMissionCompleted -= OnMissionCompleted;
    }

    // ─────────────────────────────────────────────────────────

    private void OnMissionStarted(int index, MissionData data)
    {
        Debug.Log("[MissionFlow] MissionStarted index=" + index + " id=" + data.missionId);

        switch (index)
        {
            case 0: SetupMission1Start(); break;
            case 1: SetupMission2Start(); break;
            case 2: SetupMission3Start(); break;
            case 3: SetupMission4Start(); break;
        }
    }

    private void OnObjectiveCompleted(MissionObjective obj)
    {
        string id = obj.ObjectiveId;
        Debug.Log($"[MissionFlow] ObjectiveCompleted id='{id}'");
        Debug.Log($"[MissionFlow] compare to obj_m2_talk_paolo='{obj_m2_talk_paolo}' -> {(id == obj_m2_talk_paolo)}");
        Debug.Log($"[MissionFlow] lengths: id={id.Length} obj={obj_m2_talk_paolo.Length}");

        Debug.Log($"[MissionFlow] ObjectiveCompleted id='{id}' | expect='{obj_m4_talk_giulio}' | equal={(id == obj_m4_talk_giulio)}");
        // ── Missione 1 ─────────────────────────
        if (id == obj_m1_talk_receptionist)
        {
            // dopo dialogo receptionist1:
            SetActiveGO(conversazioneReceptionist1GO, false);

            SetBadgeEnabled(true);
            SetSmartphone(true);
            SetPcLoginEnabled(true);

            SetFx(fxPlayerDesk, true);
        }
        else if (id == obj_m1_go_desk)
        {
            SetFx(fxPlayerDesk, false);
        }
        else if (id == obj_m1_login_done)
        {
            //// fine login: disabilita pc login e avvia missione 2
            //SetPcLoginEnabled(false);

            //// abilita paolo + fx
            //SetNpcEnabled(paoloNpc, true);
            //SetFx(fxPaoloDesk, true);

            //SetActiveGO(conversazioneReceptionPostIspezioniGO, true);

            //// se preferisci: mm.StartNextMission() SOLO quando si chiude il report
            //// qui la versione semplice: start subito
            //mm.StartNextMission();
        }

        // ── Missione 2 ─────────────────────────
        else if (id == obj_m2_talk_paolo)
        {
            // fine dialogo paolo: fx off, npc off, attiva i 3 oggetti m2
            SetFx(fxPaoloDesk, false);
            SetNpcEnabled(paoloNpc, false);
            Debug.Log("Se mi leggi dovrei attivare gli oggetti");
            SetObjectsActive(mission2Objects, true);

            




            // mm.RevealObjective(obj_m2_deliver_items);
        }
        
        else if(id == obj_m2_inspect_postit)
        {
            //receptionist1.SetEnabled(true);
            m2Inspectpostit = true;
            TryEnableReceptionistAfterM2Inspections();
        }
        else if (id == obj_m2_inspect_headphones)
        {
            //receptionist1.SetEnabled(true);
            m2Inspectheadphones = true;
            TryEnableReceptionistAfterM2Inspections();

        }
        else if (id == obj_m2_inspect_usb)
        {
            //receptionist1.SetEnabled(true);
            m2Inspectusb = true;
            TryEnableReceptionistAfterM2Inspections();

        }
        else if (id == obj_m2_deliver_items)
        {
            // dopo consegna m2: spegni oggetti (se sono ancora in scena) e vai a missione 3
            SetObjectsActive(mission2Objects, false);

            SetActiveGO(conversazioneReceptionPostIspezioniGO, false);
            SetActiveGO(conversazioneReception2GO, true);

            mm.StartNextMission();
        }

        // ── Missione 3 ─────────────────────────
        else if (id == obj_m3_talk_receptionist)
        {

            Debug.Log("Entro qui dopo che parlo con lei nella missione 3");
            SetActiveGO(conversazioneReception2GO, false);
            SetObjectsActive(mission3Objects, true);
            SetNpcEnabled(intrusoNpc, true); // se qui per te è l’NPC 4, cambia reference


            receptionist1.SetConversation(convReceptionistPostIspezioni3, completeObjectiveOnEnd: "", disableAfter: false);

            receptionist1.SetEnabled(false);

        }

        else if (id == obj_m3_inspect_relax)
        {
            m3InspectRelaxDone = true;
            TryEnableReceptionistAfterM3Inspections();
        }

        else if (id == obj_m3_inspect_server)
        {
            m3InspectServerDone = true;

            if (intrusoNpcRoot != null)
                intrusoNpcRoot.SetActive(false);
            else
                SetNpcEnabled(intrusoNpc, true); // fallback

            TryEnableReceptionistAfterM3Inspections();

        }
        else if (id == obj_m3_deliver_items)
        {
            SetObjectsActive(mission3Objects, false);
            mm.StartNextMission();


        }

        // ── Missione 4 ─────────────────────────
        else if (id == obj_m4_talk_giulio)
        {
            SetFx(fxGiulio, false);
            SetNpcEnabled(giulioNpc, false);
            SetPcEmailEnabled(true);
        }
        else if (id == obj_m4_check_emails)
        {
            // fine gioco
            Debug.Log("[MissionFlow] Fine. Tutte le attività completate.");
        }
    }

    private void OnMissionCompleted(int index, MissionData data)
    {
        Debug.Log("[MissionFlow] MissionCompleted index=" + index);
        // versione semplice: non fare nulla qui (perché già facciamo StartNextMission sugli obiettivi chiave)
    }

    // ─────────────────────────────────────────────────────────
    // SETUP per missione

    private void SetupMission1Start()
    {
        Debug.Log("[MissionFlow] SetupMission1Start");

        SetSmartphone(false);
        SetBadgeEnabled(false);

        SetPcLoginEnabled(false);
        SetPcEmailEnabled(false);

        SetObjectsActive(mission2Objects, false);
        SetObjectsActive(mission3Objects, false);

        SetNpcEnabled(paoloNpc, false);
        SetNpcEnabled(intrusoNpc, false);
        SetNpcEnabled(giulioNpc, false);

        SetActiveGO(conversazioneReceptionist1GO, true);
        SetActiveGO(conversazioneReception2GO, false);
        SetActiveGO(conversazioneReceptionPostIspezioniGO, false);

        SetFx(fxPlayerDesk, false);
        SetFx(fxPaoloDesk, false);
        SetFx(fxGiulio, false);

        // receptionist1 deve essere parlabile
        SetNpcEnabled(receptionist1, true);

        receptionist1.SetEnabled(true);
        receptionist1.SetConversation(convReceptionist1, completeObjectiveOnEnd: "m1_talk_receptionist", disableAfter: true);

    }

    private void SetupMission2Start()
    {
        Debug.Log("[MissionFlow] SetupMission2Start");

        SetNpcEnabled(paoloNpc, true);
        SetFx(fxPaoloDesk, true);
        SetObjectsActive(mission2Objects, false); // li accendiamo dopo dialogo paolo
        SetActiveGO(conversazioneReceptionPostIspezioniGO, true);

        receptionist1.SetEnabled(false);

        SmartphoneManager.Instance.ReceiveMessage("Paolo Corti", "Ciao, puoi venire alla mia postazione? Ho bisogno del tuo aiuto con degli oggetti!");
    }

    private void SetupMission3Start()
    {
        Debug.Log("[MissionFlow] SetupMission3Start");

        m3InspectServerDone = false;
        m3InspectRelaxDone = false;

        SetObjectsActive(mission2Objects, false); // li accendiamo dopo dialogo paolo
        SetObjectsActive(mission3Objects, false);
        SetActiveGO(conversazioneReception2GO, true);
        // gli npc/effetti specifici li accendi quando completi obiettivo dialogo receptionist

        receptionist1.SetEnabled(true);
        receptionist1.SetConversation(convReceptionist2, completeObjectiveOnEnd: "", disableAfter: true);

    }

    private void SetupMission4Start()
    {
        Debug.Log("[MissionFlow] SetupMission4Start");
        SetObjectsActive(mission3Objects, false);
        SetPcEmailEnabled(false);
        SetNpcEnabled(giulioNpc, true);
        receptionist1.SetEnabled(false);
        SetFx(fxGiulio, true);

        SmartphoneManager.Instance.ReceiveMessage("Giulio Verdoni", "Ciao, ho ricevuto diverse mail e non capisco quali siano di phishing, vieni ad aiutarmi?");

    }

    // ─────────────────────────────────────────────────────────
    // Helpers

    private void SetObjectsActive(GameObject[] arr, bool active)
    {
        if (arr == null) return;

        // Nome layer di destinazione in base al bool
        string targetLayerName = active ? "Interactable" : "Default";
        int targetLayer = LayerMask.NameToLayer(targetLayerName);

        if (targetLayer < 0)
        {
            Debug.LogError($"[MissionFlow] Layer '{targetLayerName}' non esiste. Crealo in Project Settings > Tags and Layers.");
            return;
        }

        foreach (var go in arr)
        {
            if (go == null) continue;
            SetLayerRecursively(go, targetLayer);
        }
    }

    private void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;

        foreach (Transform child in root.transform)
            SetLayerRecursively(child.gameObject, layer);
    }


    private void SetFx(GameObject fx, bool active)
    {
        if (fx != null) fx.SetActive(active);
    }

    private void SetSmartphone(bool active)
    {
        if (smartphoneRoot != null) smartphoneRoot.SetActive(active);
    }

    private void SetPcLoginEnabled(bool enabled)
    {
        if (pcLogin != null) pcLogin.SetEnabled(enabled);
    }

    private void SetPcEmailEnabled(bool enabled)
    {
        if (pcEmail != null) pcEmail.SetEnabled(enabled);
    }

    private void SetNpcEnabled(NPCInteractable npc, bool enabled)
    {
        if (npc != null) npc.SetEnabled(enabled);
    }

    private void SetActiveGO(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private void SetBadgeEnabled(bool enabled)
    {
        if (badgePickupRoot != null)
            badgePickupRoot.SetActive(enabled);
    }

    private void TryEnableReceptionistAfterM3Inspections()
    {
        if (m3InspectServerDone && m3InspectRelaxDone)
        {
            receptionist1.SetEnabled(true);
 
        }
    }

    private void TryEnableReceptionistAfterM2Inspections()
    {
        if (m2Inspectpostit && m2Inspectheadphones && m2Inspectusb)
        {
            receptionist1.SetEnabled(true);
            receptionist1.SetConversation(convReceptionistPostIspezioni2, completeObjectiveOnEnd: "", disableAfter: false);

        }
    }

}
