using UnityEngine;
using System.Collections;

/// <summary>
/// Gestisce il passaggio tra la camera del player e la camera del PC.
/// Usa due camere separate: una per il gameplay normale, una per il PC.
/// </summary>
public class PCCameraController : MonoBehaviour
{
    [Header("Camere")]
    [Tooltip("Camera dedicata al PC (posizionata davanti allo schermo)")]
    [SerializeField] private Camera pcCamera;

    [Tooltip("Camera del player (si auto-trova se vuota)")]
    [SerializeField] private Camera playerCamera;

    [Header("Canvas")]
    [Tooltip("Il Canvas del PC (per impostare Event Camera)")]
    [SerializeField] private Canvas pcCanvas;

    [Tooltip("Canvas della UI principale da nascondere (HUD, inventario, ecc)")]
    [SerializeField] private GameObject mainUICanvas;

    [Header("Transizione")]
    [SerializeField] private bool useFade = false;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Riferimenti Player (Auto-trovati se vuoti)")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private MonoBehaviour firstPersonController;
    [SerializeField] private MonoBehaviour playerInput;
    [SerializeField] private MonoBehaviour playerInteractor;
    [SerializeField] private MonoBehaviour smartphoneInput;

    // Stato
    private bool isAtScreen = false;
    private bool isTransitioning = false;

    // Callback
    public System.Action OnTransitionToScreenComplete;
    public System.Action OnTransitionBackComplete;

    private void Awake()
    {
        Debug.Log("[PCCameraController] Awake - Cercando riferimenti...");

        // Auto-trova la camera del player
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera != null)
            Debug.Log($"[PCCameraController] Player Camera trovata: {playerCamera.name}");
        else
            Debug.LogError("[PCCameraController] Player Camera NON TROVATA!");

        // Auto-trova il player
        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            Debug.Log($"[PCCameraController] Player trovato: {playerObject.name}");

            // Cerca FirstPersonController
            if (firstPersonController == null)
            {
                firstPersonController = playerObject.GetComponent("FirstPersonController") as MonoBehaviour;
                if (firstPersonController == null)
                    firstPersonController = playerObject.GetComponent("FPSController") as MonoBehaviour;
                if (firstPersonController == null)
                    firstPersonController = playerObject.GetComponent("PlayerController") as MonoBehaviour;
            }

            if (firstPersonController != null)
                Debug.Log($"[PCCameraController] Controller trovato: {firstPersonController.GetType().Name}");
            else
                Debug.LogWarning("[PCCameraController] FirstPersonController NON trovato!");

            // Cerca PlayerInput
            if (playerInput == null)
                playerInput = playerObject.GetComponent("PlayerInput") as MonoBehaviour;

            if (playerInput != null)
                Debug.Log("[PCCameraController] PlayerInput trovato");

            // Cerca PlayerInteractor
            if (playerInteractor == null)
                playerInteractor = playerObject.GetComponent("PlayerInteractor") as MonoBehaviour;

            if (playerInteractor != null)
                Debug.Log("[PCCameraController] PlayerInteractor trovato");
            else
                Debug.LogWarning("[PCCameraController] PlayerInteractor NON trovato!");
        }
        else
        {
            Debug.LogWarning("[PCCameraController] Player con tag 'Player' NON TROVATO!");
        }
        // Cerca SmartphoneInput (sul player) per bloccare il tasto P mentre sei al PC
        if (smartphoneInput == null && playerObject != null)
        {
            smartphoneInput = playerObject.GetComponentInChildren<SmartphoneInput>(true);
        }

        // Verifica che la PC Camera sia assegnata
        if (pcCamera == null)
        {
            Debug.LogError("[PCCameraController] PC Camera NON ASSEGNATA! Assegnala nell'Inspector.");
        }
        else
        {
            // Assicurati che la PC Camera sia disattiva all'inizio
            pcCamera.gameObject.SetActive(false);
            Debug.Log($"[PCCameraController] PC Camera trovata: {pcCamera.name}");
        }

        // Auto-trova il Canvas del PC se non assegnato
        if (pcCanvas == null)
        {
            pcCanvas = GetComponentInParent<Canvas>();
            if (pcCanvas == null)
                pcCanvas = GetComponent<Canvas>();
        }

        if (pcCanvas != null)
            Debug.Log($"[PCCameraController] PC Canvas trovato: {pcCanvas.name}");
        else
            Debug.LogWarning("[PCCameraController] PC Canvas non trovato - l'interazione potrebbe non funzionare");
    }

    /// <summary>
    /// Passa alla camera del PC
    /// </summary>
    public void TransitionToScreen()
    {
        Debug.Log("[PCCameraController] TransitionToScreen() chiamato");

        if (isTransitioning)
        {
            Debug.LogWarning("[PCCameraController] Già in transizione, ignoro");
            return;
        }

        if (isAtScreen)
        {
            Debug.LogWarning("[PCCameraController] Già allo schermo, ignoro");
            return;
        }

        if (pcCamera == null)
        {
            Debug.LogError("[PCCameraController] PC Camera non assegnata!");
            OnTransitionToScreenComplete?.Invoke();
            return;
        }

        StartCoroutine(TransitionToScreenCoroutine());
    }

    /// <summary>
    /// Torna alla camera del player
    /// </summary>
    public void TransitionBack()
    {
        Debug.Log("[PCCameraController] TransitionBack() chiamato");

        if (isTransitioning)
        {
            Debug.LogWarning("[PCCameraController] Già in transizione, ignoro");
            return;
        }

        if (!isAtScreen)
        {
            Debug.LogWarning("[PCCameraController] Non allo schermo, ignoro");
            return;
        }

        StartCoroutine(TransitionBackCoroutine());
    }

    private IEnumerator TransitionToScreenCoroutine()
    {
        isTransitioning = true;
        Debug.Log("[PCCameraController] Transizione verso PC AVVIATA");

        // Disabilita i controlli del player
        SetPlayerControlsEnabled(false);

        // Nascondi la UI principale (HUD, inventario, ecc)
        if (mainUICanvas != null)
        {
            mainUICanvas.SetActive(false);
            Debug.Log("[PCCameraController] UI principale nascosta");
        }

        // Piccola pausa per effetto (opzionale)
        if (useFade)
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // Scambia le camere
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (pcCamera != null)
            pcCamera.gameObject.SetActive(true);

        // Imposta la Event Camera del Canvas per permettere l'interazione
        if (pcCanvas != null && pcCamera != null)
        {
            pcCanvas.worldCamera = pcCamera;
            Debug.Log("[PCCameraController] Event Camera impostata su PC Camera");
        }

        if (useFade)
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        isTransitioning = false;
        isAtScreen = true;

        Debug.Log("[PCCameraController] Transizione verso PC COMPLETATA");
        OnTransitionToScreenComplete?.Invoke();
    }

    private IEnumerator TransitionBackCoroutine()
    {
        isTransitioning = true;
        Debug.Log("[PCCameraController] Transizione verso Player AVVIATA");

        if (useFade)
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // Scambia le camere
        if (pcCamera != null)
            pcCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        // Ripristina la Event Camera (opzionale, ma più pulito)
        if (pcCanvas != null && playerCamera != null)
        {
            pcCanvas.worldCamera = playerCamera;
        }

        if (useFade)
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // Riabilita i controlli del player
        SetPlayerControlsEnabled(true);

        // Rimostra la UI principale
        if (mainUICanvas != null)
        {
            mainUICanvas.SetActive(true);
            Debug.Log("[PCCameraController] UI principale riattivata");
        }

        isTransitioning = false;
        isAtScreen = false;

        Debug.Log("[PCCameraController] Transizione verso Player COMPLETATA");
        OnTransitionBackComplete?.Invoke();
    }

    /// <summary>
    /// Abilita/disabilita i controlli del player
    /// </summary>
    private void SetPlayerControlsEnabled(bool enabled)
    {
        Debug.Log($"[PCCameraController] SetPlayerControlsEnabled({enabled})");

        // Disabilita FirstPersonController
        if (firstPersonController != null)
        {
            firstPersonController.enabled = enabled;
            Debug.Log($"[PCCameraController] Controller.enabled = {enabled}");
        }

        // Disabilita PlayerInput (per il nuovo Input System)
        if (playerInput != null)
        {
            playerInput.enabled = enabled;
            Debug.Log($"[PCCameraController] PlayerInput.enabled = {enabled}");
        }

        // Disabilita PlayerInteractor (per evitare che E chiuda il PC)
        if (playerInteractor != null)
        {
            playerInteractor.enabled = enabled;
            Debug.Log($"[PCCameraController] PlayerInteractor.enabled = {enabled}");
        }
        // Disabilita SmartphoneInput (per evitare che P apra il telefono mentre sei al PC)
        if (smartphoneInput != null)
        {
            smartphoneInput.enabled = enabled;
            Debug.Log($"[PCCameraController] SmartphoneInput.enabled = {enabled}");
        }

        // Gestisci cursore
        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Forza il ritorno immediato (senza transizione) - per emergenze
    /// </summary>
    public void ForceReturnToPlayer()
    {
        Debug.Log("[PCCameraController] ForceReturnToPlayer()");

        StopAllCoroutines();

        if (pcCamera != null)
            pcCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        SetPlayerControlsEnabled(true);

        isTransitioning = false;
        isAtScreen = false;
    }

    // Properties pubbliche
    public bool IsAtScreen => isAtScreen;
    public bool IsTransitioning => isTransitioning;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pcCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pcCamera.transform.position, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pcCamera.transform.position, pcCamera.transform.forward * 0.5f);
        }
    }
#endif
}