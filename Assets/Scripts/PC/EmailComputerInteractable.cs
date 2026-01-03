using UnityEngine;

/// <summary>
/// Permette l'interazione col PC della missione email.
/// Implementa IInteractable per integrarsi col sistema di interazione esistente.
/// </summary>
public class EmailComputerInteractable : MonoBehaviour, IInteractable
{
    [Header("Riferimenti")]
    [SerializeField] private EmailInterfaceManager emailManager;

    [Header("Configurazione")]
    [SerializeField] private string interactionText = "Controlla Email";
    [SerializeField] private string disabledText = "Email già controllate";

    // Stato
    private bool isEnabled = true;

    private void Awake()
    {
        // Auto-trova EmailInterfaceManager se non assegnato
        if (emailManager == null)
        {
            emailManager = GetComponentInChildren<EmailInterfaceManager>(true);

            if (emailManager == null)
                emailManager = GetComponentInParent<EmailInterfaceManager>();
        }

        if (emailManager == null)
        {
            Debug.LogError("[EmailComputerInteractable] EmailInterfaceManager non trovato!");
        }
    }

    /// <summary>
    /// Restituisce il testo da mostrare quando il giocatore guarda il computer
    /// </summary>
    public string GetInteractionText()
    {
        if (!isEnabled)
            return disabledText;

        return interactionText;
    }

    /// <summary>
    /// Chiamato quando il giocatore interagisce col computer
    /// </summary>
    public void Interact(PlayerInteractor interactor)
    {
        if (!isEnabled)
        {
            Debug.Log("[EmailComputerInteractable] Interazione disabilitata");
            return;
        }

        if (emailManager == null)
        {
            Debug.LogError("[EmailComputerInteractable] EmailInterfaceManager non assegnato!");
            return;
        }

        Debug.Log("[EmailComputerInteractable] Apertura interfaccia email");
        emailManager.Open(interactor);
    }

    /// <summary>
    /// Abilita/disabilita l'interazione col computer
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        Debug.Log($"[EmailComputerInteractable] Interazione {(enabled ? "abilitata" : "disabilitata")}");
    }

    /// <summary>
    /// Verifica se l'interazione è abilitata
    /// </summary>
    public bool IsEnabled()
    {
        return isEnabled;
    }

    /// <summary>
    /// Verifica se il computer può essere interagito (richiesto da alcuni sistemi)
    /// </summary>
    public bool CanInteract()
    {
        return isEnabled && emailManager != null && !emailManager.IsOpen;
    }
}