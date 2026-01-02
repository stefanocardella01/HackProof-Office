using UnityEngine;

/// <summary>
/// Componente da aggiungere al modello 3D del computer.
/// Implementa IInteractable per permettere l'interazione con il PlayerInteractor.
/// </summary>
public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [Header("Riferimenti")]
    [SerializeField] private PCInterfaceManager pcInterface;

    [Header("Configurazione")]
    [SerializeField] private string computerName = "Computer";
    [SerializeField] private string interactionVerb = "Usa";

    [Header("Stato")]
    [SerializeField] private bool isEnabled = true;

    [Header("Effetti Visivi (Opzionale)")]
    [SerializeField] private GameObject screenGlow; // Effetto luminoso sullo schermo
    [SerializeField] private Material screenOnMaterial;
    [SerializeField] private Material screenOffMaterial;
    [SerializeField] private Renderer screenRenderer;

    private void Start()
    {
        // Se non è stato assegnato manualmente, cerca il PCInterfaceManager
        if (pcInterface == null)
        {
            pcInterface = GetComponentInChildren<PCInterfaceManager>();

            if (pcInterface == null)
            {
                Debug.LogWarning($"[ComputerInteractable] Nessun PCInterfaceManager trovato su {gameObject.name}");
            }
        }

        // Imposta lo stato iniziale dello schermo
        UpdateScreenVisuals();
    }

    public string GetInteractionText()
    {
        if (!isEnabled)
            return "";

        if (pcInterface != null && pcInterface.IsOpen)
            return ""; // Non mostrare testo se già aperto

        return $"{interactionVerb} {computerName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!isEnabled)
        {
            Debug.Log("[ComputerInteractable] Computer disabilitato");
            return;
        }

        if (pcInterface == null)
        {
            Debug.LogError("[ComputerInteractable] PCInterfaceManager non assegnato!");
            return;
        }

        if (pcInterface.IsOpen)
        {
            // Se già aperto, chiudi
            pcInterface.Close();
        }
        else
        {
            // Apri l'interfaccia
            pcInterface.Open(interactor);
        }
    }

    /// <summary>
    /// Abilita o disabilita l'interazione con il computer
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        UpdateScreenVisuals();

        // Se disabilitato, spegni anche il componente: così il raycast non lo trova più
        if (!enabled)
        {
            this.enabled = false;
        }
    }

    /// <summary>
    /// Aggiorna gli effetti visivi dello schermo
    /// </summary>
    private void UpdateScreenVisuals()
    {
        // Gestisci glow
        if (screenGlow != null)
        {
            screenGlow.SetActive(isEnabled);
        }

        // Gestisci materiale schermo
        if (screenRenderer != null)
        {
            if (isEnabled && screenOnMaterial != null)
            {
                screenRenderer.material = screenOnMaterial;
            }
            else if (!isEnabled && screenOffMaterial != null)
            {
                screenRenderer.material = screenOffMaterial;
            }
        }
    }

    /// <summary>
    /// Verifica se tutte le attività del PC sono completate
    /// </summary>
    public bool AreTasksCompleted()
    {
        return pcInterface != null && pcInterface.AreAllTasksCompleted();
    }

    /// <summary>
    /// Ottiene il riferimento al PCInterfaceManager
    /// </summary>
    public PCInterfaceManager GetPCInterface()
    {
        return pcInterface;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-find PCInterfaceManager in editor
        if (pcInterface == null)
        {
            pcInterface = GetComponentInChildren<PCInterfaceManager>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Disegna un'icona per indicare che è interagibile
        Gizmos.color = isEnabled ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
    }
#endif
}