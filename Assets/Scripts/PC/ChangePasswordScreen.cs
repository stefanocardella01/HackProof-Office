using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

/// <summary>
/// Gestisce la schermata di cambio password.
/// Implementa validazione della sicurezza della password.
/// </summary>
public class ChangePasswordScreen : MonoBehaviour, IPCScreen
{
    [Header("Campi Input")]
    [SerializeField] private TMP_InputField currentPasswordInput;
    [SerializeField] private TMP_InputField newPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;

    [Header("Pulsanti")]
    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button skipButton; 

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI strengthIndicator;
    [SerializeField] private Slider strengthSlider;
    [SerializeField] private Image strengthFill;

    [Header("Requisiti UI")]
    [SerializeField] private TextMeshProUGUI reqLengthText;
    [SerializeField] private TextMeshProUGUI reqUppercaseText;
    [SerializeField] private TextMeshProUGUI reqLowercaseText;
    [SerializeField] private TextMeshProUGUI reqNumberText;
    [SerializeField] private TextMeshProUGUI reqSpecialText;

    [Header("Configurazione")]
    [SerializeField] private string currentPassword = "password123";
    [SerializeField] private int minLength = 8;
    [SerializeField] private bool requireUppercase = true;
    [SerializeField] private bool requireLowercase = true;
    [SerializeField] private bool requireNumber = true;
    [SerializeField] private bool requireSpecialChar = true;

    [Header("Messaggi")]
    [SerializeField] private string instructionMessage = "Crea una nuova password sicura per proteggere il tuo account.";
    [SerializeField] private string currentPasswordWrong = "La password attuale non è corretta.";
    [SerializeField] private string passwordMismatch = "Le password non coincidono.";
    [SerializeField] private string passwordTooWeak = "La password non soddisfa i requisiti di sicurezza.";
    [SerializeField] private string passwordSameAsOld = "La nuova password deve essere diversa dalla precedente.";
    [SerializeField] private string successMessage = "Password modificata con successo!";

    [Header("Colori")]
    [SerializeField] private Color requirementMetColor = Color.green;
    [SerializeField] private Color requirementNotMetColor = Color.gray;
    [SerializeField] private Color weakColor = Color.red;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color strongColor = Color.green;

    private PCInterfaceManager manager;
    private bool isProcessing = false;

    public void Initialize(PCInterfaceManager pcManager)
    {
        manager = pcManager;

        // Setup pulsanti
        if (changePasswordButton != null)
        {
            changePasswordButton.onClick.RemoveAllListeners();
            changePasswordButton.onClick.AddListener(OnChangePasswordClicked);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
            // Il pulsante Skip è visibile - permette all'utente di saltare
        }

        // Setup listener per la validazione in tempo reale
        if (newPasswordInput != null)
        {
            newPasswordInput.contentType = TMP_InputField.ContentType.Password;
            newPasswordInput.onValueChanged.AddListener(OnNewPasswordChanged);
        }

        if (currentPasswordInput != null)
            currentPasswordInput.contentType = TMP_InputField.ContentType.Password;

        if (confirmPasswordInput != null)
            confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
    }

    public void Show()
    {
        // Reset UI
        ClearInputs();
        SetFeedback(instructionMessage, Color.white);
        UpdateRequirementsUI("");
        UpdateStrengthIndicator(0);

        // Focus sul primo campo
        if (currentPasswordInput != null)
            currentPasswordInput.Select();
    }

    public void Hide()
    {
        ClearInputs();
        isProcessing = false;
    }

    private void ClearInputs()
    {
        if (currentPasswordInput != null) currentPasswordInput.text = "";
        if (newPasswordInput != null) newPasswordInput.text = "";
        if (confirmPasswordInput != null) confirmPasswordInput.text = "";
    }

    private void OnNewPasswordChanged(string newPassword)
    {
        UpdateRequirementsUI(newPassword);
        int strength = CalculatePasswordStrength(newPassword);
        UpdateStrengthIndicator(strength);
    }

    private void UpdateRequirementsUI(string password)
    {
        bool hasLength = password.Length >= minLength;
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasNumber = Regex.IsMatch(password, "[0-9]");
        bool hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

        UpdateRequirementText(reqLengthText, $"✓ Almeno {minLength} caratteri", hasLength);

        if (requireUppercase)
            UpdateRequirementText(reqUppercaseText, "✓ Almeno una lettera maiuscola", hasUpper);

        if (requireLowercase)
            UpdateRequirementText(reqLowercaseText, "✓ Almeno una lettera minuscola", hasLower);

        if (requireNumber)
            UpdateRequirementText(reqNumberText, "✓ Almeno un numero", hasNumber);

        if (requireSpecialChar)
            UpdateRequirementText(reqSpecialText, "✓ Almeno un carattere speciale (!@#$%...)", hasSpecial);
    }

    private void UpdateRequirementText(TextMeshProUGUI text, string message, bool isMet)
    {
        if (text == null) return;

        text.text = message;
        text.color = isMet ? requirementMetColor : requirementNotMetColor;
    }

    private int CalculatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password)) return 0;

        int strength = 0;

        // Lunghezza
        if (password.Length >= minLength) strength += 20;
        if (password.Length >= 12) strength += 10;
        if (password.Length >= 16) strength += 10;

        // Complessità
        if (Regex.IsMatch(password, "[A-Z]")) strength += 15;
        if (Regex.IsMatch(password, "[a-z]")) strength += 15;
        if (Regex.IsMatch(password, "[0-9]")) strength += 15;
        if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]")) strength += 15;

        return Mathf.Min(strength, 100);
    }

    private void UpdateStrengthIndicator(int strength)
    {
        if (strengthSlider != null)
            strengthSlider.value = strength / 100f;

        Color strengthColor;
        string strengthText;

        if (strength < 40)
        {
            strengthColor = weakColor;
            strengthText = "Debole";
        }
        else if (strength < 70)
        {
            strengthColor = mediumColor;
            strengthText = "Media";
        }
        else
        {
            strengthColor = strongColor;
            strengthText = "Forte";
        }

        if (strengthFill != null)
            strengthFill.color = strengthColor;

        if (strengthIndicator != null)
        {
            strengthIndicator.text = strength > 0 ? $"Sicurezza: {strengthText}" : "";
            strengthIndicator.color = strengthColor;
        }
    }

    private bool ValidatePassword(string password)
    {
        if (password.Length < minLength) return false;
        if (requireUppercase && !Regex.IsMatch(password, "[A-Z]")) return false;
        if (requireLowercase && !Regex.IsMatch(password, "[a-z]")) return false;
        if (requireNumber && !Regex.IsMatch(password, "[0-9]")) return false;
        if (requireSpecialChar && !Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]")) return false;

        return true;
    }

    private void OnChangePasswordClicked()
    {
        if (isProcessing) return;

        string currentPwd = currentPasswordInput?.text ?? "";
        string newPwd = newPasswordInput?.text ?? "";
        string confirmPwd = confirmPasswordInput?.text ?? "";

        // Verifica password attuale
        if (currentPwd != currentPassword)
        {
            SetFeedback(currentPasswordWrong, Color.red);
            ShakeField(currentPasswordInput);
            return;
        }

        // Verifica che la nuova password sia diversa
        if (newPwd == currentPassword)
        {
            SetFeedback(passwordSameAsOld, Color.red);
            ShakeField(newPasswordInput);
            return;
        }

        // Verifica requisiti di sicurezza
        if (!ValidatePassword(newPwd))
        {
            SetFeedback(passwordTooWeak, Color.red);
            ShakeField(newPasswordInput);
            return;
        }

        // Verifica conferma password
        if (newPwd != confirmPwd)
        {
            SetFeedback(passwordMismatch, Color.red);
            ShakeField(confirmPasswordInput);
            return;
        }

        // Tutto ok - procedi
        StartCoroutine(ProcessPasswordChange(newPwd));
    }

    private System.Collections.IEnumerator ProcessPasswordChange(string newPassword)
    {
        isProcessing = true;
        SetInputsInteractable(false);
        SetFeedback("Modifica password in corso...", Color.white);

        yield return new WaitForSeconds(1f);

        // Aggiorna la password (in un gioco reale salveresti questo valore)
        currentPassword = newPassword;

        SetFeedback(successMessage, Color.green);

        yield return new WaitForSeconds(1.5f);

        // Notifica il manager
        manager?.OnPasswordChangeSuccess();

        isProcessing = false;
    }

    private void OnSkipClicked()
    {
        // L'utente ha scelto di saltare il cambio password
        manager?.OnPasswordChangeSkipped();
    }

    private void SetInputsInteractable(bool interactable)
    {
        if (currentPasswordInput != null) currentPasswordInput.interactable = interactable;
        if (newPasswordInput != null) newPasswordInput.interactable = interactable;
        if (confirmPasswordInput != null) confirmPasswordInput.interactable = interactable;
        if (changePasswordButton != null) changePasswordButton.interactable = interactable;
    }

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

    /// <summary>
    /// Imposta la password attuale (per configurazione)
    /// </summary>
    public void SetCurrentPassword(string password)
    {
        currentPassword = password;
    }

}