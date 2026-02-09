using UnityEngine;

public class ReportTestHarness : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public GameObject reportPanel;   // il tuo ReportPanel (Canvas_Report/ReportPanel)
    public ReportUI reportUI;        // componente ReportUI (quello con Build())

    [Header("Auto test all'avvio")]
    public bool runOnStart = true;

    private void Start()
    {
        if (runOnStart)
            RunTestCase_A();
    }

    private void Update()
    {
        // Tasti rapidi per provare casi diversi
        if (Input.GetKeyDown(KeyCode.F5)) RunTestCase_A(); // caso misto
        if (Input.GetKeyDown(KeyCode.F6)) RunTestCase_AllPass();
        if (Input.GetKeyDown(KeyCode.F7)) RunTestCase_AllFail();
        if (Input.GetKeyDown(KeyCode.F8)) HideReport();
    }

    public void RunTestCase_A()
    {
        if (MissionTracker.Instance == null)
        {
            Debug.LogError("[ReportTestHarness] MissionTracker.Instance è null. Assicurati che MissionTracker sia in scena.");
            return;
        }
        if (reportUI == null || reportPanel == null)
        {
            Debug.LogError("[ReportTestHarness] Assegna reportUI e reportPanel nell'Inspector.");
            return;
        }

        // reset
        MissionTracker.Instance.ResetAll();

        // ---- SIMULAZIONE: mettiamo risultati realistici ----
        // Missione 1
        MissionTracker.Instance.Set(ReportCheck.PasswordChanged, true);
        MissionTracker.Instance.Set(ReportCheck.TwoFactorEnabled, false);

        // Missione 2
        MissionTracker.Instance.Set(ReportCheck.PostItDelivered, true);
        MissionTracker.Instance.Set(ReportCheck.UsbDelivered, false);
        MissionTracker.Instance.Set(ReportCheck.HeadphonesDelivered, false); // in base al tuo report, questo può significare "segnalate" oppure "non segnalate"

        // Missione 3
        MissionTracker.Instance.Set(ReportCheck.BadgeDelivered, true);
        MissionTracker.Instance.Set(ReportCheck.ManualDelivered, false);
        MissionTracker.Instance.Set(ReportCheck.ScrewdriverDelivered, true);
        MissionTracker.Instance.Set(ReportCheck.IntruderKicked, true);

        // Missione 4 (se hai più email)
        MissionTracker.Instance.Set(ReportCheck.Email1Correct, true);
        MissionTracker.Instance.Set(ReportCheck.Email2Correct, false);
        MissionTracker.Instance.Set(ReportCheck.Email3Correct, true);

        ShowAndBuild();
    }

    public void RunTestCase_AllPass()
    {
        MissionTracker.Instance.ResetAll();

        foreach (ReportCheck check in System.Enum.GetValues(typeof(ReportCheck)))
            MissionTracker.Instance.Set(check, true);

        ShowAndBuild();
    }

    public void RunTestCase_AllFail()
    {
        MissionTracker.Instance.ResetAll();

        // Se non setti nulla, Get() restituisce false.
        // Lo facciamo esplicito solo per chiarezza.
        foreach (ReportCheck check in System.Enum.GetValues(typeof(ReportCheck)))
            MissionTracker.Instance.Set(check, false);

        ShowAndBuild();
    }

    private void ShowAndBuild()
    {
        reportPanel.SetActive(true);
        reportUI.OpenReport();
        Debug.Log("[ReportTestHarness] Report generato.");
    }

    public void HideReport()
    {
        if (reportPanel != null)
            reportPanel.SetActive(false);
    }
}
