using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Collections;

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

    // modalità: report di fine missione vs micro-report (email)
    private bool _isMissionReport = true;
    private System.Action _onCloseCallback;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseReport);

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


    public void CloseReport()
    {
        if (reportRoot == null) return;

        reportRoot.SetActive(false);
        isOpen = false;

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
        foreach (var entry in mission.entries)
        {
            bool value = MissionTracker.Instance.Get(entry.check);
            bool ok = (value == entry.expectedValue);

            // prima assegni explanation "base"
            string explanation = ok ? entry.okText : entry.badText;

            // poi, se è EmailScore, la sovrascrivi con il testo dinamico
            if (entry.check == ReportCheck.EmailScore)
            {
                explanation = $"Hai classificato correttamente {EmailFinalScore.LastScore} email su {EmailFinalScore.LastTotal}.";
                ok = true; // (opzionale) così la riga risulta “verde” sempre
            }

            var row = Instantiate(rowPrefab, contentParent);
            row.GetComponent<ReportRowUI>().Setup(entry.label, ok, explanation);
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

        foreach (var mission in missions)
        {
            // header
            var h = Instantiate(headerPrefab, contentParent);
            h.GetComponent<TextMeshProUGUI>().text = mission.missionTitle;

            // righe
            foreach (var entry in mission.entries)
            {
                bool ok = MissionTracker.Instance.Get(entry.check);

                // explanation base
                string explanation = ok ? entry.okText : entry.badText;

                // ✅ caso speciale: punteggio email
                if (entry.check == ReportCheck.EmailScore)
                {
                    explanation = $"Hai classificato correttamente {EmailFinalScore.LastScore} email su {EmailFinalScore.LastTotal}.";
                    ok = true; // sempre verde (informativo)
                }

                var row = Instantiate(rowPrefab, contentParent);
                row.GetComponent<ReportRowUI>().Setup(entry.label, ok, explanation);
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





