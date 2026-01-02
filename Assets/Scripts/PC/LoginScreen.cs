using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestisce la schermata di login del PC.
/// Il giocatore deve inserire username e password corretti.
/// </summary>
public class LoginScreen : MonoBehaviour, IPCScreen
{
    [Header("Campi Input")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Pulsanti")]
    [SerializeField] private Button loginButton;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Configurazione")]
    [SerializeField] private string correctUsername = "dipendente";
    [SerializeField] private string correctPassword = "password123";
    [SerializeField] private float loginDelay = 1.5f;
  

    [Header("Messaggi")]
    [SerializeField] private string welcomeMessage = "Benvenuto! Effettua il login per accedere.";
    [SerializeField] private string errorMessage = "Username o password non corretti.";
    [SerializeField] private string emptyFieldsMessage = "Inserisci username e password.";
    [SerializeField] private string successMessage = "Login effettuato con successo!";

    private PCInterfaceManager manager;
    private bool isProcessing = false;

    public void Initialize(PCInterfaceManager pcManager)
    {
        manager = pcManager;

        // Setup pulsante login
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }

        // Setup input fields per submit con Enter
        if (usernameInput != null)
        {
            usernameInput.onSubmit.AddListener(_ => OnLoginButtonClicked());
        }

        if (passwordInput != null)
        {
            // Fa apparire ****** al posto della password
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.onSubmit.AddListener(_ => OnLoginButtonClicked());
        }
    }

    public void Show()
    {
        if (usernameInput != null)
        {
            usernameInput.text = "";
            usernameInput.interactable = true;
        }

        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.interactable = true;
        }

        if (loginButton != null)
            loginButton.interactable = true;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        SetFeedback(welcomeMessage, Color.white);

        if (usernameInput != null)
            usernameInput.Select();
    }

    public void Hide()
    {
        // Pulisci i campi per sicurezza
        if (usernameInput != null)
            usernameInput.text = "";

        if (passwordInput != null)
            passwordInput.text = "";

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        isProcessing = false;
    }

    private void OnLoginButtonClicked()
    {
        if (isProcessing) return;

        string username = usernameInput?.text?.Trim() ?? "";
        string password = passwordInput?.text ?? "";

        // Validazione campi vuoti
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetFeedback(emptyFieldsMessage, Color.yellow);
            ShakeInput();
            return;
        }

        // Avvia processo di login
        StartCoroutine(ProcessLogin(username, password));
    }

    private System.Collections.IEnumerator ProcessLogin(string username, string password)
    {
        isProcessing = true;
        SetInputsInteractable(false);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        SetFeedback("Verifica credenziali...", Color.white);

        yield return new WaitForSeconds(loginDelay);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        bool isValid = (username.ToLower() == correctUsername.ToLower()) &&
                       (password == correctPassword);

        if (isValid)
        {
            // Login riuscito
            SetFeedback(successMessage, Color.green);
            yield return new WaitForSeconds(1f);
            manager?.OnLoginSuccess();
        }
        else
        {
            // Login fallito - può riprovare all'infinito
            SetFeedback(errorMessage, Color.red);
            SetInputsInteractable(true);
            ShakeInput();

            if (passwordInput != null)
            {
                passwordInput.text = "";
                passwordInput.Select();
            }
        }

        isProcessing = false;
    }

    private void SetInputsInteractable(bool interactable)
    {
        if (usernameInput != null)
            usernameInput.interactable = interactable;

        if (passwordInput != null)
            passwordInput.interactable = interactable;

        if (loginButton != null)
            loginButton.interactable = interactable;
    }

    private void SetFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    private void ShakeInput()
    {
        // Effetto shake semplice 
        StartCoroutine(ShakeCoroutine());
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        if (usernameInput == null) yield break;

        RectTransform rect = usernameInput.GetComponent<RectTransform>();
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


    /// <summary>
    /// Imposta le credenziali corrette (per configurazione dinamica)
    /// </summary>
    public void SetCredentials(string username, string password)
    {
        correctUsername = username;
        correctPassword = password;
    }
}