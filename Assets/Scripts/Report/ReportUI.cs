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
            playerController.enabled = false;
            playerController.ForceStopWalking();

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


        if (_isFinalReportOpen)
            SceneManager.LoadScene("EndMenu");
        else
            CloseReport();
    }

    public void CloseReport()
    {

        Debug.LogError($"[ReportUI] CloseReport instanceID={GetInstanceID()} selected={EventSystem.current?.currentSelectedGameObject?.name}");

        if (reportRoot == null) return;

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

        // righe
        // righe (ordinate: verdi, rossi, gialli)
        var ordered = new List<ReportEntry>(mission.entries);
        ordered.Sort((a, b) => GetEntryPriority(a).CompareTo(GetEntryPriority(b)));

        foreach (var entry in ordered)
        {
            bool value = MissionTracker.Instance.Get(entry.check);
            bool ok = (value == entry.expectedValue);

            string explanation = ok ? entry.okText : entry.badText;

            if (entry.check == ReportCheck.EmailScore)
            {
                explanation = $"Hai classificato correttamente {EmailFinalScore.LastScore} email su {EmailFinalScore.LastTotal}.";
                ok = true;
            }

            var row = Instantiate(rowPrefab, contentParent);
            row.GetComponent<ReportRowUI>().Setup(entry.check, entry.label, ok, explanation);
        }



    }

    public void Build()
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

        var t = Instantiate(headerPrefab, contentParent);
        var tmp = t.GetComponent<TextMeshProUGUI>();

        tmp.text = "REPORT FINALE";
        tmp.fontSize = 48;   // cambia valore come vuoi
        tmp.fontStyle = FontStyles.Bold; // opzionale


        foreach (var mission in missions)
        {
            // header
            var h = Instantiate(headerPrefab, contentParent);
            h.GetComponent<TextMeshProUGUI>().text = mission.missionTitle;

            // righe (ordinate: verdi, rossi, gialli)
            var ordered = new List<ReportEntry>(mission.entries);
            ordered.Sort((a, b) => GetEntryPriority(a).CompareTo(GetEntryPriority(b)));

            foreach (var entry in ordered)
            {
                bool value = MissionTracker.Instance.Get(entry.check);
                bool ok = (value == entry.expectedValue);

                string explanation = ok ? entry.okText : entry.badText;

                if (entry.check == ReportCheck.EmailScore)
                {
                    explanation = $"Hai classificato correttamente {EmailFinalScore.LastScore} email su {EmailFinalScore.LastTotal}.";
                    ok = true;
                }

                var row = Instantiate(rowPrefab, contentParent);
                row.GetComponent<ReportRowUI>().Setup(entry.check, entry.label, ok, explanation);
            }

        }


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
            playerController.enabled = false;
            playerController.ForceStopWalking();

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
            playerController.enabled = false;
            playerController.ForceStopWalking();

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
}





