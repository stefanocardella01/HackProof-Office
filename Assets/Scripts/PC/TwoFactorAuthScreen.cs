using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestisce la schermata di attivazione dell'autenticazione a due fattori (2FA).
/// Il codice viene inviato allo smartphone del giocatore che deve:
/// 1. Uscire dall'interfaccia PC
/// 2. Aprire lo smartphone (P)
/// 3. Leggere il codice dal messaggio
/// 4. Tornare al PC e inserirlo
/// </summary>
public class TwoFactorAuthScreen : MonoBehaviour, IPCScreen
{
    [Header("Step 1 - Introduzione")]
    [SerializeField] private GameObject step1Panel;
    [SerializeField] private Button activateButton;
    [SerializeField] private Button skipButton;

    [Header("Step 2 - Invio Codice")]
    [SerializeField] private GameObject step2Panel;
    [SerializeField] private TextMeshProUGUI step2InfoText;
    [SerializeField] private Button sendCodeButton;

    [Header("Step 3 - Inserimento Codice")]
    [SerializeField] private GameObject step3Panel;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private Button verifyButton;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Step 4 - Completato")]
    [SerializeField] private GameObject step4Panel;
    [SerializeField] private Button finishButton;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Configurazione Codice")]
    [SerializeField] private bool useRandomCode = true;
    [SerializeField] private string fixedCode = "123456"; // Solo se useRandomCode = false

    [Header("Configurazione Messaggio SMS")]
    [SerializeField] private string smsSenderName = "Sistema Sicurezza";
    [SerializeField] private Sprite smsSenderIcon;

    [Header("Messaggi UI")]
    [SerializeField] private string step1Message = "L'autenticazione a due fattori protegge il tuo account richiedendo un codice aggiuntivo ad ogni accesso.";
    [SerializeField] private string step2Message = "Invieremo un codice di verifica al tuo telefono aziendale.";
    [SerializeField] private string step3Message = "Controlla il tuo telefono aziendale (premi P) e inserisci il codice ricevuto.";
    [SerializeField] private string codeSentMessage = "Codice inviato! Premi ESC per uscire, poi P per aprire il telefono.";
    [SerializeField] private string codeErrorMessage = "Codice non valido. Riprova.";
    [SerializeField] private string successMessage = "2FA attivato con successo! Il tuo account è ora protetto.";

    private PCInterfaceManager manager;
    private int currentStep = 1;
    private string currentValidCode;
    private bool codeSent = false;
    private bool isProcessing = false;

    // Mantiene lo stato tra apertura/chiusura del PC
    private static int savedStep = 1;
    private static string savedCode = "";
    private static bool savedCodeSent = false;

    public void Initialize(PCInterfaceManager pcManager)
    {
        manager = pcManager;

        // Setup pulsanti Step 1
        if (activateButton != null)
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(() => GoToStep(2));
        }

        // Setup pulsante Skip (Step 1)
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        // Setup pulsanti Step 2
        if (sendCodeButton != null)
        {
            sendCodeButton.onClick.RemoveAllListeners();
            sendCodeButton.onClick.AddListener(OnSendCodeClicked);
        }

        // Setup pulsanti Step 3
        if (verifyButton != null)
        {
            verifyButton.onClick.RemoveAllListeners();
            verifyButton.onClick.AddListener(OnVerifyClicked);
        }

        if (codeInput != null)
        {
            codeInput.characterLimit = 6;
            codeInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            codeInput.onSubmit.AddListener(_ => OnVerifyClicked());
        }

        // Setup pulsanti Step 4
        if (finishButton != null)
        {
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(OnFinishClicked);
        }
    }

    public void Show()
    {
        // Ripristina lo stato salvato (se il player torna al PC)
        if (savedCodeSent)
        {
            currentStep = savedStep;
            currentValidCode = savedCode;
            codeSent = savedCodeSent;
        }
        else
        {
            currentStep = 1;
            codeSent = false;
        }

        isProcessing = false;

        // Mostra lo step appropriato
        GoToStep(currentStep);
    }

    public void Hide()
    {
        // Salva lo stato quando si esce
        savedStep = currentStep;
        savedCode = currentValidCode;
        savedCodeSent = codeSent;

        isProcessing = false;
    }

    private void GoToStep(int step)
    {
        currentStep = step;
        HideAllPanels();

        switch (step)
        {
            case 1:
                ShowStep1();
                break;
            case 2:
                ShowStep2();
                break;
            case 3:
                ShowStep3();
                break;
            case 4:
                ShowStep4();
                break;
        }
    }

    private void HideAllPanels()
    {
        if (step1Panel != null) step1Panel.SetActive(false);
        if (step2Panel != null) step2Panel.SetActive(false);
        if (step3Panel != null) step3Panel.SetActive(false);
        if (step4Panel != null) step4Panel.SetActive(false);
    }

    #region Step Implementations

    private void ShowStep1()
    {
        if (step1Panel != null)
            step1Panel.SetActive(true);

        SetFeedback(step1Message, Color.white);
    }

    private void ShowStep2()
    {
        if (step2Panel != null)
            step2Panel.SetActive(true);

        if (step2InfoText != null)
            step2InfoText.text = step2Message;

        SetFeedback(step2Message, Color.white);
    }

    private void ShowStep3()
    {
        if (step3Panel != null)
            step3Panel.SetActive(true);

        // Reset input
        if (codeInput != null)
        {
            codeInput.text = "";
            codeInput.Select();
        }

        // Mostra hint
        if (hintText != null)
        {
            if (codeSent)
            {
                hintText.text = "Hai già ricevuto il codice. Controllalo sul telefono (P).";
            }
            else
            {
                hintText.text = "";
            }
        }

        SetFeedback(step3Message, Color.white);
    }

    private void ShowStep4()
    {
        if (step4Panel != null)
            step4Panel.SetActive(true);

        // Reset dello stato salvato
        savedStep = 1;
        savedCode = "";
        savedCodeSent = false;

        SetFeedback(successMessage, Color.green);
    }

    #endregion

    #region Button Handlers

    private void OnSendCodeClicked()
    {
        if (isProcessing) return;

        StartCoroutine(SendCodeToPhone());
    }

    private System.Collections.IEnumerator SendCodeToPhone()
    {
        isProcessing = true;

        if (sendCodeButton != null)
            sendCodeButton.interactable = false;

        SetFeedback("Invio codice in corso...", Color.white);

        yield return new WaitForSeconds(0.8f);

        // Genera il codice
        GenerateNewCode();

        // Invia messaggio allo smartphone
        SendSMSToSmartphone();

        codeSent = true;

        SetFeedback(codeSentMessage, Color.green);

        yield return new WaitForSeconds(1.5f);

        // Passa allo step 3
        GoToStep(3);

        isProcessing = false;
    }



   

    private void OnVerifyClicked()
    {
        if (isProcessing) return;

        string enteredCode = codeInput?.text?.Trim() ?? "";

        if (enteredCode.Length != 6)
        {
            SetFeedback("Il codice deve essere di 6 cifre.", Color.yellow);
            return;
        }

        StartCoroutine(ProcessVerification(enteredCode));
    }

    private System.Collections.IEnumerator ProcessVerification(string code)
    {
        isProcessing = true;

        if (verifyButton != null)
            verifyButton.interactable = false;

        SetFeedback("Verifica in corso...", Color.white);

        yield return new WaitForSeconds(0.8f);

        if (code == currentValidCode)
        {
            // Codice corretto!
            GoToStep(4);
        }
        else
        {
            // Codice errato - può riprovare all'infinito
            SetFeedback(codeErrorMessage, Color.red);

            // Pulisci input
            if (codeInput != null)
            {
                codeInput.text = "";
                codeInput.Select();
            }

            ShakeField(codeInput);
        }

        if (verifyButton != null)
            verifyButton.interactable = true;

        isProcessing = false;
    }

    private void OnFinishClicked()
    {
        // 2FA completato con successo
        manager?.On2FASuccess();
    }
    private void OnSkipClicked()
    {
        // L'utente ha scelto di saltare l'attivazione 2FA
        manager?.On2FASkippedByUser();
    }

    #endregion

    #region Code Generation & SMS

    private void GenerateNewCode()
    {
        if (useRandomCode)
        {
            currentValidCode = Random.Range(100000, 999999).ToString();
        }
        else
        {
            currentValidCode = fixedCode;
        }

        Debug.Log($"[2FA] Codice generato: {currentValidCode}");
    }

    private void SendSMSToSmartphone()
    {
        // Verifica che SmartphoneManager esista
        if (SmartphoneManager.Instance == null)
        {
            Debug.LogError("[2FA] SmartphoneManager non trovato! Il messaggio non può essere inviato.");
            return;
        }

        // Crea il messaggio
        string smsText = $"Il tuo codice di verifica è: {currentValidCode}\n\nNon condividere questo codice con nessuno.";

        // Invia tramite SmartphoneManager
        SmartphoneManager.Instance.ReceiveMessage(smsSenderName, smsText);

        Debug.Log($"[2FA] SMS inviato allo smartphone con codice: {currentValidCode}");
    }

    #endregion

    #region Helpers

    private void SetFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    private void ShakeField(TMP_InputField field)
    {
        if (field == null) return;
        StartCoroutine(ShakeCoroutine(field.GetComponent<RectTransform>()));
    }

    private System.Collections.IEnumerator ShakeCoroutine(RectTransform rect)
    {
        if (rect == null) yield break;

        Vector3 originalPos = rect.localPosition;
        float elapsed = 0f;
        float duration = 0.3f;
        float magnitude = 10f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
            rect.localPosition = new Vector3(x, originalPos.y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localPosition = originalPos;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Mostra un messaggio finale nascondendo tutti gli altri elementi.
    /// Chiamato dal PCInterfaceManager prima di chiudere il PC.
    /// </summary>
    public void ShowFinalMessage(string message)
    {
        // Nascondi tutti i pannelli degli step
        HideAllPanels();

        // Mostra solo il messaggio di feedback
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = message;
            feedbackText.color = Color.white;
            feedbackText.fontSize = 28;
        }
    }

    #endregion
}