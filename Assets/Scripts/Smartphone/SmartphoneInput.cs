using UnityEngine;
using StarterAssets;

/// <summary>
/// Gestisce l'input per lo smartphone (tasto P) e la disattivazione
/// dei controlli del player quando lo smartphone è aperto.
/// </summary>
public class SmartphoneInput : MonoBehaviour
{
    [Header("Riferimenti Player")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private StarterAssetsInputs starterInputs;
    [SerializeField] private PlayerInteractor playerInteractor;

    [Header("Input Settings")]
    [SerializeField] private KeyCode openCloseKey = KeyCode.P;

    private SmartphoneManager manager;

    // Stato salvato dei controlli
    private bool wasControllerEnabled;
    private bool wasCursorLocked;

    private void Start()
    {
        manager = SmartphoneManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[SmartphoneInput] SmartphoneManager non trovato!");
            return;
        }

        // Iscrizione agli eventi per gestire abilitazione/disabilitazione controlli
        manager.OnSmartphoneOpened += DisablePlayerControls;
        manager.OnSmartphoneClosed += EnablePlayerControls;

        // Auto-find dei riferimenti se non assegnati
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<FirstPersonController>();
        }

        if (starterInputs == null)
        {
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (playerInteractor == null)
        {
            playerInteractor = FindFirstObjectByType<PlayerInteractor>();
        }
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnSmartphoneOpened -= DisablePlayerControls;
            manager.OnSmartphoneClosed -= EnablePlayerControls;
        }
    }

    private void Update()
    {
        if (manager == null) return;

        // Tasto P per toggle smartphone
        if (Input.GetKeyDown(openCloseKey))
        {
            manager.Toggle();
        }

    }

    /// <summary>
    /// Disabilita i controlli del player quando lo smartphone è aperto.
    /// </summary>
    private void DisablePlayerControls()
    {
        Debug.Log("[SmartphoneInput] Disabilitando controlli player...");

        // Salva lo stato attuale
        if (playerController != null)
        {
            wasControllerEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        // Disabilita l'input
        if (starterInputs != null)
        {
            // Reset degli input per evitare movimento residuo
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
            starterInputs.jump = false;
            starterInputs.sprint = false;
        }

        // Disabilita le interazioni
        if (playerInteractor != null)
        {
            playerInteractor.enabled = false;
        }

        // Sblocca e mostra il cursore per navigare la UI
        wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Riabilita i controlli del player quando lo smartphone viene chiuso.
    /// </summary>
    private void EnablePlayerControls()
    {
        Debug.Log("[SmartphoneInput] Riabilitando controlli player...");

        // Ripristina il controller
        if (playerController != null)
        {
            playerController.enabled = wasControllerEnabled;
        }

        // Riabilita le interazioni
        if (playerInteractor != null)
        {
            playerInteractor.enabled = true;
        }

        // Ripristina lo stato del cursore
        if (wasCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Verifica se lo smartphone può essere aperto.
    /// Potresti aggiungere condizioni qui (es: non durante dialoghi).
    /// </summary>
    public bool CanOpenSmartphone()
    {
        // !TODO: Aggiungi condizioni specifiche se necessario
        // Esempio: non aprire durante l'ispezione di un oggetto
        // if (FindFirstObjectByType<InspectUI>()?.IsOpen == true) return false;

        return true;
    }
}