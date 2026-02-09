using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;

public class ReportUI : MonoBehaviour
{
    [Header("Dati report")]
    public List<MissionReportData> missions;
    public Transform contentParent;
    public GameObject headerPrefab;
    public GameObject rowPrefab;



    [Header("UI")]
    public GameObject reportRoot;      // es: ReportPanel (o Canvas_Report)
    public Button continueButton;      // bottone "Continua"

    [Header("Blocco player")]
    public FirstPersonController playerController;
    public StarterAssetsInputs starterInputs;

    private bool isOpen;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseReport);

        if (reportRoot != null)
            reportRoot.SetActive(false);

        // Se non li hai assegnati a mano nell'Inspector, prova a recuperarli
        if (playerController == null)
            playerController = FindFirstObjectByType<FirstPersonController>();

        if (starterInputs == null)
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();

        Debug.Log("[ReportUI] Awake chiamato");
    }

    // CHIAMA QUESTO quando vuoi mostrare il report a fine missione
    public void OpenReport()
    {
        if (reportRoot == null) return;

        reportRoot.SetActive(true);
        isOpen = true;

        Debug.Log("[ReportUI] OpenReport chiamato");

        // mostra cursore
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // blocca player
        if (playerController != null)
            playerController.enabled = false;

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        Debug.Log($"PlayerInput: {(playerController != null ? "OK" : "NULL")}, StarterInputs enabled: {starterInputs?.enabled}");


        Build();
    }

    public void CloseReport()
    {
        if (reportRoot == null) return;

        reportRoot.SetActive(false);
        isOpen = false;

        // nascondi cursore
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // riabilita player
        if (playerController != null)
            playerController.enabled = true;

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = true;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }
    }

    private void Update()
    {
        // opzionale: ESC chiude il report
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseReport();

        if (Input.GetKeyDown(KeyCode.F9))
            OpenReport();
    }

    public void Build()
    {
        foreach (Transform c in contentParent)
        {
            if (continueButton != null && c.gameObject == continueButton.gameObject)
                continue;

            Destroy(c.gameObject);
        }

        foreach (var mission in missions)
        {
            var h = Instantiate(headerPrefab, contentParent);
            h.GetComponent<TextMeshProUGUI>().text = mission.missionTitle;

            foreach (var entry in mission.entries)
            {
                bool ok = MissionTracker.Instance.Get(entry.check);
                string explanation = ok ? entry.okText : entry.badText;

                var row = Instantiate(rowPrefab, contentParent);
                row.GetComponent<ReportRowUI>()
                   .Setup(entry.label, ok, explanation);
            }
        }

        if (continueButton != null)
            continueButton.transform.SetAsLastSibling();
    }

}
