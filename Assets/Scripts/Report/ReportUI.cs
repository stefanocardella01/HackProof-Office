using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86.Avx;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;


public class ReportUI : MonoBehaviour
{
    [Header("Dati report (uno per missione, nello stesso ordine Mission1, Mission2, ...)")]
    public List<MissionReportData> missions;

    public Transform contentParent;
    public GameObject headerPrefab;
    public GameObject rowPrefab;

    [Header("UI")]
    public GameObject reportRoot;
    public Button continueButton;

    [Header("Final Report Prefabs")]
    [SerializeField] private GameObject finalSectionPrefab;
    [SerializeField] private GameObject summaryBoxPrefab;

    [Header("Email (Missione 4)")]
    [SerializeField] private EmailInterfaceManager emailInterfaceManager;



    [Header("Blocco player")]
    public FirstPersonController playerController;
    public StarterAssetsInputs starterInputs;

    [Header("Canvas con inventario, interact text e crosshair")]
    public GameObject hudCanvas;

    [Header("Canvas smartphone")]
    public GameObject hudSmartphone;
    [Header("Canvas MissionCheckList")]
    public GameObject hudMissionCheckList;

    [Header("Altre UI da disattivare quando il report è aperto")]
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private GameObject smartphoneRoot;
    [SerializeField] private GameObject missionChecklistRoot;

    // Audio
    [SerializeField] private ManagerAudio mixer;

    private bool isOpen;

    public bool IsOpen => isOpen;

    // quale missione mostrare
    private int _missionIndexToShow = -1;

    private bool _isFinalReportOpen = false;

    private bool _returnToFinalOnClose = false;
    private int _lastFinalScrollY = 0; // opzionale (se vuoi ricordare la posizione)


    // modalità: report di fine missione vs micro-report (email)
    private bool _isMissionReport = true;
    private System.Action _onCloseCallback;
    private int _closeSeq = 0;

    private static ReportUI _instance;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (emailInterfaceManager == null)
            emailInterfaceManager = FindFirstObjectByType<EmailInterfaceManager>();



        Debug.Log($"[ReportUI] Awake instanceID={GetInstanceID()} GO={name} scene={gameObject.scene.name}");

        if (continueButton != null)
            Debug.Log($"[ReportUI] continueButton='{continueButton.name}' btnID={continueButton.GetInstanceID()}");
        else
            Debug.LogWarning("[ReportUI] continueButton NULL");

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnCloseButtonClicked);
            continueButton.onClick.AddListener(OnCloseButtonClicked);

            // IMPORTANTISSIMO: NON aggiungere CloseReport direttamente
            continueButton.onClick.RemoveListener(CloseReport);
        }

        if (reportRoot != null)
            reportRoot.SetActive(false);

        if (playerController == null)
            playerController = FindFirstObjectByType<FirstPersonController>();

        if (starterInputs == null)
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();
    }

    private bool TryGetEmailExplanationAndSubject(ReportCheck check, out string subject, out string explanation)
    {
        subject = null;
        explanation = null;

        if (emailInterfaceManager == null || emailInterfaceManager.Emails == null)
            return false;

        int index = check switch
        {
            ReportCheck.Email1Correct => 0,
            ReportCheck.Email2Correct => 1,
            ReportCheck.Email3Correct => 2,
            ReportCheck.Email4Correct => 3,
            ReportCheck.Email5Correct => 4,
            _ => -1
        };

        if (index < 0 || index >= emailInterfaceManager.Emails.Length)
            return false;

        var data = emailInterfaceManager.Emails[index];
        if (data == null) return false;

        subject = data.subject;
        explanation = data.explanation;
        return !string.IsNullOrWhiteSpace(explanation);
    }


    private void OpenReportFromFinal(int missionIndex)
    {
        // Rimaniamo in contesto "final report"
        _returnToFinalOnClose = true;

        // IMPORTANTE: non deve essere considerato report di fine missione
        // altrimenti CloseReport chiama StartNextMission
        _isMissionReport = false;

        // Manteniamo final report open: la X nel final report porta a EndMenu,
        // ma in questa view "dettaglio" deve tornare indietro (gestito dal flag sopra).
        _isFinalReportOpen = true;

        if (reportRoot == null) return;

        reportRoot.SetActive(true);
        isOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.ForceStopWalking();
        }

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        // HUD off
        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (hudSmartphone != null) hudSmartphone.SetActive(false);
        if (hudMissionCheckList != null) hudMissionCheckList.SetActive(false);

        mixer.SetDialog();

        // Mostra il report della missione selezionata
        BuildSingle(missionIndex);
    }

    public void OpenReport(int missionIndex)
    {

        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Nascondi l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(false);

        //Nascondi mission check list
        if(hudMissionCheckList != null)
            hudMissionCheckList.SetActive(false);

        mixer.SetDialog();

        _isFinalReportOpen = false;

        _isMissionReport = true;
        _onCloseCallback = null;


        if (reportRoot == null) return;

        _missionIndexToShow = missionIndex;

        reportRoot.SetActive(true);
        isOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.ForceStopWalking();
        }


        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        BuildSingle(_missionIndexToShow);
    }

    public void OpenReportDelayed(int missionIndex, float delay = 1f)
    {
        StartCoroutine(OpenDelayedRoutine(missionIndex, delay));
    }

    private IEnumerator OpenDelayedRoutine(int missionIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        OpenReport(missionIndex);
    }

    public void OnCloseButtonClicked()
    {
        Debug.Log($"[ReportUI] OnCloseButtonClicked instanceID={GetInstanceID()} selected={EventSystem.current?.currentSelectedGameObject?.name}");


        // Se sto tornando al report finale, NON andare all'EndMenu
        if (_isFinalReportOpen && !_returnToFinalOnClose)
            SceneManager.LoadScene("EndMenu");
        else
            CloseReport();

    }

    public void CloseReport()
    {

        Debug.LogError($"[ReportUI] CloseReport instanceID={GetInstanceID()} selected={EventSystem.current?.currentSelectedGameObject?.name}");

        if (reportRoot == null) return;

        // Caso speciale: ero nel dettaglio missione aperto dal report finale
        if (_returnToFinalOnClose)
        {
            _returnToFinalOnClose = false;

            // Ricostruisci la schermata finale SENZA uscire dalla UI
            BuildFinalSummary();

            // Mantieni stato "final report"
            _isMissionReport = false;
            _isFinalReportOpen = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerController != null) playerController.enabled = false;

            if (starterInputs != null)
            {
                starterInputs.cursorInputForLook = false;
                starterInputs.move = Vector2.zero;
                starterInputs.look = Vector2.zero;
            }

            // HUD resta off
            if (hudCanvas != null) hudCanvas.SetActive(false);
            if (hudSmartphone != null) hudSmartphone.SetActive(false);
            if (hudMissionCheckList != null) hudMissionCheckList.SetActive(false);

            mixer.SetDialog();
            return;
        }


        reportRoot.SetActive(false);
        isOpen = false;

        _closeSeq++;
        Debug.LogError($"### CLOSEREPORT #{_closeSeq} ###\n" + Environment.StackTrace);

        // Micro-report (Email): lascia cursore visibile e player bloccato
        if (!_isMissionReport)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // NON riabilitare il player qui
            if (playerController != null)
                playerController.enabled = false;

            if (starterInputs != null)
            {
                starterInputs.cursorInputForLook = false;
                starterInputs.move = Vector2.zero;
                starterInputs.look = Vector2.zero;
            }

            var cb = _onCloseCallback;
            _onCloseCallback = null;
            cb?.Invoke();
            return;
        }

        mixer.SetNormal();


        // Report fine missione: torna al gameplay normale
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = true;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
            hudCanvas.SetActive(true);

        // Nascondi l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(true);

        //Nascondi mission check list
        if (hudMissionCheckList != null)
            hudMissionCheckList.SetActive(true);

        if (MissionManager.Instance != null)
            MissionManager.Instance.StartNextMission();
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseReport();

        //debug
        //if (Input.GetKeyDown(KeyCode.F9)) OpenReport(0);

        // DEBUG
        if (Input.GetKeyDown(KeyCode.F10))
            OpenFinalReport();
    }

    private int GetEntryPriority(ReportEntry entry)
    {
        bool value = MissionTracker.Instance.Get(entry.check);
        bool ok = (value == entry.expectedValue);

        // Caso speciale EmailScore: trattalo sempre come ok (verde)
        if (entry.check == ReportCheck.EmailScore)
            ok = true;

        if (ok) return 0; // verdi prima

        bool isMinor = entry.check == ReportCheck.ManualDelivered ||
                       entry.check == ReportCheck.ScrewdriverDelivered;

        return isMinor ? 2 : 1; // rossi poi, gialli ultimi
    }

    private void BuildSingle(int missionIndex)
    {
        // pulizia
        foreach (Transform c in contentParent)
        {
            if (continueButton != null && c.gameObject == continueButton.gameObject)
                continue;

            Destroy(c.gameObject);
        }

        if (missions == null || missions.Count == 0)
        {
            Debug.LogWarning("[ReportUI] Lista missions vuota!");
            return;
        }

        if (missionIndex < 0 || missionIndex >= missions.Count)
        {
            Debug.LogWarning($"[ReportUI] missionIndex fuori range: {missionIndex} (missions.Count={missions.Count})");
            missionIndex = Mathf.Clamp(missionIndex, 0, missions.Count - 1);
        }

        var mission = missions[missionIndex];

        // header
        var h = Instantiate(headerPrefab, contentParent);
        h.GetComponent<TextMeshProUGUI>().text = mission.missionTitle;

        if (missionIndex == 3)
        {
            BuildEmailsList();
            return;
        }

        // righe (ordinate: verdi, rossi, gialli)
        var ordered = new List<ReportEntry>(mission.entries);
        ordered.Sort((a, b) => GetEntryPriority(a).CompareTo(GetEntryPriority(b)));

        foreach (var entry in ordered)
        {
            // (opzionale) se NON vuoi la riga "Risultato complessivo", salta EmailScore
            if (entry.check == ReportCheck.EmailScore)
                continue;

            bool value = MissionTracker.Instance.Get(entry.check);
            bool ok = (value == entry.expectedValue);

            string explanation = ok ? entry.okText : entry.badText;
            string labelToShow = entry.label;

            // Se è una riga Email1..Email5, usa ESATTAMENTE EmailData.explanation + subject
            if (TryGetEmailExplanationAndSubject(entry.check, out var subj, out var exp))
            {
                labelToShow = subj;
                explanation = exp;
            }

            var row = Instantiate(rowPrefab, contentParent);
            row.GetComponent<ReportRowUI>().Setup(entry.check, labelToShow, ok, explanation);
        }
    }


    public void Build()
    {
        BuildFinalSummary();
    }

    private void BuildFinalSummary()
    {
        // pulizia completa
        foreach (Transform c in contentParent)
            Destroy(c.gameObject);

        if (missions == null || missions.Count == 0)
        {
            Debug.LogWarning("[ReportUI] Lista missions vuota!");
            return;
        }

        // Header
        var header = Instantiate(headerPrefab, contentParent);
        var tmp = header.GetComponent<TextMeshProUGUI>();
        tmp.text = "REPORT FINALE";
        tmp.fontSize = 48;
        tmp.fontStyle = FontStyles.Bold;

        for (int i = 0; i < missions.Count; i++)
            CreateMissionSection(i);
    }

    private bool IsCorrect(ReportCheck check, bool expectedValue)
    {
        bool value = MissionTracker.Instance.Get(check);
        return value == expectedValue;
    }


    private void CreateMissionSection(int missionIndex)
    {
        if (finalSectionPrefab == null)
        {
            Debug.LogError("[ReportUI] finalSectionPrefab NULL (assegnalo in Inspector)");
            return;
        }

        if (summaryBoxPrefab == null)
        {
            Debug.LogError("[ReportUI] summaryBoxPrefab NULL (assegnalo in Inspector)");
            return;
        }

        var mission = missions[missionIndex];

        var sectionGO = Instantiate(finalSectionPrefab, contentParent);
        var sectionUI = sectionGO.GetComponent<FinalReportSectionUI>();

        if (sectionUI == null)
        {
            Debug.LogError("[ReportUI] FinalSectionPrefab NON ha FinalReportSectionUI attaccato.");
            return;
        }

        sectionUI.SetTitle(mission.missionTitle);

        switch (missionIndex)
        {
            case 0: BuildMission1(sectionUI); break;
            case 1: BuildMission2(sectionUI); break;
            case 2: BuildMission3(sectionUI); break;
            case 3: BuildMission4(sectionUI); break;
            default:
                // se hai più missioni in futuro
                sectionUI.AddSummaryBox(summaryBoxPrefab, "Nessun riepilogo configurato", "", SummaryStatus.Yellow);
                break;
        }

        // Bottone dettagli
        sectionUI.SetupDetailsButton(() =>
        {
            OpenReportFromFinal(missionIndex);
        });

    }

    private void BuildMission1(FinalReportSectionUI section)
    {
        int total = 2;
        int correct = 0;

        bool password = MissionTracker.Instance.Get(ReportCheck.PasswordChanged);
        bool twoFA = MissionTracker.Instance.Get(ReportCheck.TwoFactorEnabled);

        if (password) correct++;
        if (twoFA) correct++;

        string headline = $"Sicurezza account: {correct}/{total} azioni completate";
        string lines =
            $"• Cambio password: {(password ? "effettuato" : "non effettuato")}\n" +
            $"• Autenticazione a 2 fattori: {(twoFA ? "attivata" : "non attivata")}";

        SummaryStatus status = GetStatus(correct, total);

        section.AddSummaryBox(summaryBoxPrefab, headline, lines, status);
    }

    private void BuildMission2(FinalReportSectionUI section)
    {
        int total = 3;
        int correct = 0;

        // expectedValue IMPORTANTI:
        // Post-it e Hard Disk: corretti se CONSEGNATI (expected=true)
        // Cuffie: corrette se NON CONSEGNATE (expected=false)
        bool postItOk = IsCorrect(ReportCheck.PostItDelivered, true);
        bool usbOk = IsCorrect(ReportCheck.UsbDelivered, true);
        bool headphonesOk = IsCorrect(ReportCheck.HeadphonesDelivered, false);

        if (postItOk) correct++;
        if (usbOk) correct++;
        if (headphonesOk) correct++;

        string headline = $"Classificazione oggetti: {correct}/{total} corretti";

        // stato reale (serve per scrivere consegnato/non consegnato)
        bool postItDelivered = MissionTracker.Instance.Get(ReportCheck.PostItDelivered);
        bool usbDelivered = MissionTracker.Instance.Get(ReportCheck.UsbDelivered);
        bool headphonesDelivered = MissionTracker.Instance.Get(ReportCheck.HeadphonesDelivered);

        string lines =
            $"• Post-it: {(postItDelivered ? "consegnato" : "non consegnato")}\n" +
            $"• Hard Disk: {(usbDelivered ? "consegnato" : "non consegnato")}\n" +
            $"• Cuffie: {(headphonesDelivered ? "consegnate" : "non consegnate")}";

        SummaryStatus status = GetStatus(correct, total);

        section.AddSummaryBox(summaryBoxPrefab, headline, lines, status);
    }


    private void BuildMission3(FinalReportSectionUI section)
    {
        // BOX 1 - Oggetti (badge/manuale/cacciavite)
        int total = 3;
        int correct = 0;

        bool badgeOk = IsCorrect(ReportCheck.BadgeDelivered, true);
        bool manualOk = IsCorrect(ReportCheck.ManualDelivered, false);
        bool screwdriverOk = IsCorrect(ReportCheck.ScrewdriverDelivered, false);

        if (badgeOk) correct++;
        if (manualOk) correct++;
        if (screwdriverOk) correct++;

        string headline = $"Oggetti classificati: {correct}/{total} corretti";

        bool badgeDelivered = MissionTracker.Instance.Get(ReportCheck.BadgeDelivered);
        bool manualDelivered = MissionTracker.Instance.Get(ReportCheck.ManualDelivered);
        bool screwdriverDelivered = MissionTracker.Instance.Get(ReportCheck.ScrewdriverDelivered);

        string lines =
            $"• Badge: {(badgeDelivered ? "consegnato" : "non consegnato")}\n" +
            $"• Manuale: {(manualDelivered ? "consegnato" : "non consegnato")}\n" +
            $"• Cacciavite: {(screwdriverDelivered ? "consegnato" : "non consegnato")}";

        section.AddSummaryBox(summaryBoxPrefab, headline, lines, GetStatus(correct, total));

        // BOX 2 - Intruso (opzione 2)
        bool intruder = MissionTracker.Instance.Get(ReportCheck.IntruderKicked);

        string intruderHeadline = intruder
            ? "Scelta corretta: l’intruso è stato allontanato"
            : "Scelta non corretta: l’intruso non è stato allontanato";

        string intruderLines = $"• Intruso nella sala server: {(intruder ? "allontanato" : "non allontanato")}";

        section.AddSummaryBox(
            summaryBoxPrefab,
            intruderHeadline,
            intruderLines,
            intruder ? SummaryStatus.Green : SummaryStatus.Red
        );

    }

    private void BuildMission4(FinalReportSectionUI section)
    {
        int total = EmailFinalScore.LastTotal;   // es: 5
        int correct = EmailFinalScore.LastScore; // es: 3

        string headline = $"Email classificate correttamente: {correct}/{total}";
        string lines =
            $"• Email 1: {(MissionTracker.Instance.Get(ReportCheck.Email1Correct) ? "corretta" : "errata")}\n" +
            $"• Email 2: {(MissionTracker.Instance.Get(ReportCheck.Email2Correct) ? "corretta" : "errata")}\n" +
            $"• Email 3: {(MissionTracker.Instance.Get(ReportCheck.Email3Correct) ? "corretta" : "errata")}\n" +
            $"• Email 4: {(MissionTracker.Instance.Get(ReportCheck.Email4Correct) ? "corretta" : "errata")}\n" +
            $"• Email 5: {(MissionTracker.Instance.Get(ReportCheck.Email5Correct) ? "corretta" : "errata")}";

        section.AddSummaryBox(summaryBoxPrefab, headline, lines, GetStatus(correct, total));
    }

    private SummaryStatus GetStatus(int correct, int total)
    {
        if (correct <= 0) return SummaryStatus.Red;
        if (correct >= total) return SummaryStatus.Green;
        return SummaryStatus.Yellow;
    }




    public void OpenSingleFeedback(string title, string label, bool ok, string explanation, System.Action onClosed)
    {
        if (reportRoot == null) return;

        _isMissionReport = false;
        _onCloseCallback = onClosed;

        reportRoot.SetActive(true);
        isOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.ForceStopWalking();
        }


        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Nascondi l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(false);

        //Nascondi mission check list
        if (hudMissionCheckList != null)
            hudMissionCheckList.SetActive(false);

        mixer.SetDialog();


        BuildOne(title, label, ok, explanation);
    }

    private void BuildOne(string title, string label, bool ok, string explanation)
    {
        // pulizia
        foreach (Transform c in contentParent)
        {
            if (continueButton != null && c.gameObject == continueButton.gameObject)
                continue;

            Destroy(c.gameObject);
        }

        // header
        var h = Instantiate(headerPrefab, contentParent);
        h.GetComponent<TextMeshProUGUI>().text = title;

        // singola riga
        var row = Instantiate(rowPrefab, contentParent);
        row.GetComponent<ReportRowUI>().Setup(label, ok, explanation);

       
      
    }

    public void OpenFinalReport()
    {
        if (reportRoot == null) return;

        _isFinalReportOpen = true;


        _isMissionReport = false;     // così CloseReport NON chiama StartNextMission
        _onCloseCallback = null;

        reportRoot.SetActive(true);
        isOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.ForceStopWalking();
        }


        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        mixer.SetDialog();


        // Nascondi l'HUD (E + inventario + crosshair)
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Nascondi l'HUD (smartphone)
        if (hudSmartphone != null)
            hudSmartphone.SetActive(false);

        //Nascondi mission check list
        if (hudMissionCheckList != null)
            hudMissionCheckList.SetActive(false);

        Build(); //mostra TUTTE le missioni
    }

    private void BuildEmailsList()
    {
        if (emailInterfaceManager == null)
            emailInterfaceManager = FindFirstObjectByType<EmailInterfaceManager>();

        var emails = emailInterfaceManager != null ? emailInterfaceManager.Emails : null;
        int total = (emails != null) ? emails.Length : 0;

        if (total <= 0)
        {
            Debug.LogWarning("[ReportUI] BuildEmailsList: nessuna email trovata (emails null o vuoto).");
            return;
        }

        for (int i = 0; i < total; i++)
        {
            // check: Email1Correct..Email5Correct
            var check = (ReportCheck)((int)ReportCheck.Email1Correct + i);

            bool ok = MissionTracker.Instance != null && MissionTracker.Instance.Get(check);

            string subject = emails[i] != null ? emails[i].subject : $"Email {i + 1}";
            string explanation = emails[i] != null ? emails[i].explanation : "";

            // Titolo box: "Email 1 — <subject>"
            string label = $"Email {i + 1} — {subject}";

            var row = Instantiate(rowPrefab, contentParent);
            row.GetComponent<ReportRowUI>().Setup(check, label, ok, explanation);
        }
    }


    private ReportEntry FindEntryForCheck(int missionIndex, ReportCheck check)
    {
        if (missions == null || missionIndex < 0 || missionIndex >= missions.Count) return null;
        var m = missions[missionIndex];
        if (m == null || m.entries == null) return null;

        for (int i = 0; i < m.entries.Count; i++)
        {
            if (m.entries[i] != null && m.entries[i].check == check)
                return m.entries[i];
        }
        return null;
    }

    private ReportCheck GetEmailCheck(int emailIndex)
    {
        return emailIndex switch
        {
            1 => ReportCheck.Email1Correct,
            2 => ReportCheck.Email2Correct,
            3 => ReportCheck.Email3Correct,
            4 => ReportCheck.Email4Correct,
            5 => ReportCheck.Email5Correct,
            6 => ReportCheck.Email6Correct,
            7 => ReportCheck.Email7Correct,
            8 => ReportCheck.Email8Correct,
            9 => ReportCheck.Email9Correct,
            _ => ReportCheck.Email9Correct
        };
    }

}





