using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manager principale dell'interfaccia PC.
/// Gestisce la navigazione tra le diverse schermate, lo stato generale,
/// la transizione della camera e il blocco dei controlli del player.
/// </summary>
public class PCInterfaceManager : MonoBehaviour
{
    public enum PCState
    {
        Inactive,
        TransitioningIn,
        Login,
        ChangePassword,
        TwoFactorAuth,
        Desktop,
        TransitioningOut
    }

    [Header("Schermate")]
    [SerializeField] private GameObject loginScreenObject;
    [SerializeField] private GameObject changePasswordScreenObject;
    [SerializeField] private GameObject twoFactorAuthScreenObject;
    [SerializeField] private GameObject desktopScreenObject;

    [Header("Riferimenti UI")]
    [SerializeField] private Canvas pcCanvas;
    [SerializeField] private GameObject screenContainer;

    [Header("Camera e Controlli")]
    [SerializeField] private PCCameraController cameraController;

    [Header("Riferimento Computer")]
    [SerializeField] private ComputerInteractable computerInteractable;

    [Header("Configurazione Missione")]
    [SerializeField] private bool requireLogin = true;
    [SerializeField] private bool requirePasswordChange = true;
    [SerializeField] private bool require2FA = true;

    [Header("Messaggio Fine Missione")]
    [SerializeField] private float closeDelay = 2f;
    [SerializeField] private string missionCompleteMessage = "Setup account completato! Il PC si chiuderà automaticamente.";
    [SerializeField] private string missionIncompleteMessage = "Setup parziale. Il PC si chiuderà automaticamente.";

    [Header("Eventi")]
    public UnityEvent OnLoginCompleted;
    public UnityEvent OnPasswordChanged;
    public UnityEvent OnPasswordSkipped;
    public UnityEvent On2FAActivated;
    public UnityEvent On2FASkipped;
    public UnityEvent OnAllTasksCompleted;
    public UnityEvent OnInterfaceOpened;
    public UnityEvent OnInterfaceClosed;
    public UnityEvent OnMission1Finished;  // NUOVO: triggerato quando la missione 1 è finita (PC si chiude e diventa non interagibile)

    // Componenti delle schermate (cache)
    private LoginScreen loginScreen;
    private ChangePasswordScreen changePasswordScreen;
    private TwoFactorAuthScreen twoFactorAuthScreen;

    // Stato completamento
    private PCState currentState = PCState.Inactive;
    private PCState pendingState = PCState.Inactive;
    private bool isOpen = false;
    private bool loginCompleted = false;
    private bool passwordChanged = false;
    private bool twoFactorActivated = false;

    // Stato skip (NUOVO)
    private bool passwordSkipped = false;
    private bool twoFactorSkipped = false;

    // Riferimento al PlayerInteractor
    private PlayerInteractor playerInteractor;

    // Properties pubbliche per lo stato
    public PCState CurrentState => currentState;
    public bool IsOpen => isOpen;
    public bool LoginCompleted => loginCompleted;
    public bool PasswordChanged => passwordChanged;
    public bool PasswordSkipped => passwordSkipped;           // NUOVO
    public bool TwoFactorActivated => twoFactorActivated;
    public bool TwoFactorSkipped => twoFactorSkipped;         // NUOVO

    private void Awake()
    {
        // Cache dei componenti delle schermate
        if (loginScreenObject != null)
            loginScreen = loginScreenObject.GetComponent<LoginScreen>();

        if (changePasswordScreenObject != null)
            changePasswordScreen = changePasswordScreenObject.GetComponent<ChangePasswordScreen>();

        if (twoFactorAuthScreenObject != null)
            twoFactorAuthScreen = twoFactorAuthScreenObject.GetComponent<TwoFactorAuthScreen>();

        // Assicura riferimento al ComputerInteractable (serve per disabilitare il PC a fine missione)
        if (computerInteractable == null)
        {
            computerInteractable = GetComponentInParent<ComputerInteractable>();
            if (computerInteractable == null)
                computerInteractable = GetComponentInChildren<ComputerInteractable>(true);
        }

        // Setup camera controller
        SetupCameraController();

        // Inizializza le schermate
        InitializeScreens();

        // Nascondi tutto all'avvio
        HideAllScreens();

        if (screenContainer != null)
            screenContainer.SetActive(false);
    }

    private void SetupCameraController()
    {
        // Se non c'è un camera controller assegnato, cercalo
        if (cameraController == null)
        {
            cameraController = GetComponent<PCCameraController>();

            if (cameraController == null)
            {
                Debug.LogError("[PCInterfaceManager] PCCameraController non trovato! Aggiungilo manualmente.");
                return;
            }
        }

        // Registra callback
        cameraController.OnTransitionToScreenComplete = OnCameraArrivedAtScreen;
        cameraController.OnTransitionBackComplete = OnCameraReturnedToPlayer;
    }

    private void InitializeScreens()
    {
        if (loginScreen != null)
            loginScreen.Initialize(this);

        if (changePasswordScreen != null)
            changePasswordScreen.Initialize(this);

        if (twoFactorAuthScreen != null)
            twoFactorAuthScreen.Initialize(this);
    }

    private void Update()
    {
        // Gestisce la chiusura con ESC quando l'interfaccia è aperta
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            // Non permettere di chiudere durante le transizioni
            if (currentState != PCState.TransitioningIn && currentState != PCState.TransitioningOut)
            {
                Close();
            }
        }
    }

    /// <summary>
    /// Apre l'interfaccia PC con transizione camera
    /// </summary>
    public void Open(PlayerInteractor interactor)
    {
        Debug.Log("[PCInterfaceManager] Open() chiamato");

        if (isOpen)
        {
            Debug.LogWarning("[PCInterfaceManager] Già aperto, ignoro");
            return;
        }

        playerInteractor = interactor;
        isOpen = true;
        currentState = PCState.TransitioningIn;

        // Determina quale schermata mostrare dopo la transizione
        DeterminePendingState();
        Debug.Log($"[PCInterfaceManager] Schermata pendente: {pendingState}");

        // Avvia transizione camera
        if (cameraController != null)
        {
            Debug.Log("[PCInterfaceManager] Avvio transizione camera");
            cameraController.TransitionToScreen();
        }
        else
        {
            Debug.LogWarning("[PCInterfaceManager] CameraController non trovato, vado diretto");
            // Se non c'è camera controller, vai direttamente
            OnCameraArrivedAtScreen();
        }

        OnInterfaceOpened?.Invoke();
    }

    /// <summary>
    /// Chiamato quando la camera arriva davanti allo schermo
    /// </summary>
    private void OnCameraArrivedAtScreen()
    {
        // Attiva il container UI
        if (screenContainer != null)
            screenContainer.SetActive(true);

        // Mostra la schermata appropriata
        GoToState(pendingState);
    }

    /// <summary>
    /// Chiude l'interfaccia PC con transizione camera
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;

        // Prima chiama Hide() sulla schermata corrente per salvare lo stato
        // (prima di cambiare currentState!)
        HideCurrentScreen();

        // Ora cambia lo stato
        currentState = PCState.TransitioningOut;

        // Poi nascondi tutte le schermate
        HideAllScreens();

        if (screenContainer != null)
            screenContainer.SetActive(false);

        // Avvia transizione camera indietro
        if (cameraController != null)
        {
            cameraController.TransitionBack();
        }
        else
        {
            // Se non c'è camera controller, completa direttamente
            OnCameraReturnedToPlayer();
        }
    }

    /// <summary>
    /// Chiamato quando la camera torna al player
    /// </summary>
    private void OnCameraReturnedToPlayer()
    {
        isOpen = false;
        currentState = PCState.Inactive;

        OnInterfaceClosed?.Invoke();
    }

    /// <summary>
    /// Determina quale schermata mostrare in base ai progressi
    /// </summary>
    private void DeterminePendingState()
    {
        if (requireLogin && !loginCompleted)
        {
            pendingState = PCState.Login;
        }
        else if (requirePasswordChange && !passwordChanged && !passwordSkipped)
        {
            pendingState = PCState.ChangePassword;
        }
        else if (require2FA && !twoFactorActivated && !twoFactorSkipped)
        {
            pendingState = PCState.TwoFactorAuth;
        }
        else
        {
            pendingState = PCState.Desktop;
        }
    }

    /// <summary>
    /// Cambia lo stato/schermata corrente
    /// </summary>
    public void GoToState(PCState newState)
    {
        // Nascondi la schermata corrente
        HideCurrentScreen();

        currentState = newState;

        // Mostra la nuova schermata
        switch (newState)
        {
            case PCState.Login:
                ShowScreen(loginScreenObject, loginScreen);
                break;
            case PCState.ChangePassword:
                ShowScreen(changePasswordScreenObject, changePasswordScreen);
                break;
            case PCState.TwoFactorAuth:
                ShowScreen(twoFactorAuthScreenObject, twoFactorAuthScreen);
                break;
            case PCState.Desktop:
                if (desktopScreenObject != null)
                    desktopScreenObject.SetActive(true);
                break;
        }
    }

    private void ShowScreen(GameObject screenObj, IPCScreen screen)
    {
        if (screenObj != null)
        {
            screenObj.SetActive(true);
            screen?.Show();
        }
    }

    private void HideCurrentScreen()
    {
        switch (currentState)
        {
            case PCState.Login:
                HideScreen(loginScreenObject, loginScreen);
                break;
            case PCState.ChangePassword:
                HideScreen(changePasswordScreenObject, changePasswordScreen);
                break;
            case PCState.TwoFactorAuth:
                HideScreen(twoFactorAuthScreenObject, twoFactorAuthScreen);
                break;
            case PCState.Desktop:
                if (desktopScreenObject != null)
                    desktopScreenObject.SetActive(false);
                break;
        }
    }

    private void HideScreen(GameObject screenObj, IPCScreen screen)
    {
        if (screenObj != null)
        {
            screen?.Hide();
            screenObj.SetActive(false);
        }
    }

    private void HideAllScreens()
    {
        if (loginScreenObject != null) loginScreenObject.SetActive(false);
        if (changePasswordScreenObject != null) changePasswordScreenObject.SetActive(false);
        if (twoFactorAuthScreenObject != null) twoFactorAuthScreenObject.SetActive(false);
        if (desktopScreenObject != null) desktopScreenObject.SetActive(false);
    }

    #region Callback dalle schermate

    /// <summary>
    /// Chiamato quando il login è completato con successo
    /// </summary>
    public void OnLoginSuccess()
    {
        loginCompleted = true;
        OnLoginCompleted?.Invoke();

        // Passa alla prossima schermata
        if (requirePasswordChange && !passwordChanged)
        {
            GoToState(PCState.ChangePassword);
        }
        else if (require2FA && !twoFactorActivated)
        {
            GoToState(PCState.TwoFactorAuth);
        }
        else
        {
            CheckAllTasksCompleted();
            GoToState(PCState.Desktop);
        }
    }

    /// <summary>
    /// Chiamato quando la password è stata cambiata con successo
    /// </summary>
    public void OnPasswordChangeSuccess()
    {
        passwordChanged = true;
        passwordSkipped = false;
        OnPasswordChanged?.Invoke();

        // Passa alla prossima schermata o chiudi
        if (require2FA && !twoFactorActivated && !twoFactorSkipped)
        {
            GoToState(PCState.TwoFactorAuth);
        }
        else
        {
            // Non c'è 2FA da fare, chiudi il PC
            CheckAllTasksCompleted();
            StartCoroutine(ClosePCAfterDelay(missionCompleteMessage));
        }
    }

    /// <summary>
    /// Chiamato quando l'utente salta il cambio password
    /// </summary>
    public void OnPasswordChangeSkipped()
    {
        passwordSkipped = true;
        passwordChanged = false;
        OnPasswordSkipped?.Invoke();

        Debug.Log("[PCInterface] Cambio password saltato dall'utente");

        // Passa alla prossima schermata o chiudi
        if (require2FA && !twoFactorActivated && !twoFactorSkipped)
        {
            GoToState(PCState.TwoFactorAuth);
        }
        else
        {
            // Non c'è 2FA da fare, chiudi il PC
            CheckAllTasksCompleted();
            StartCoroutine(ClosePCAfterDelay(missionIncompleteMessage));
        }
    }

    /// <summary>
    /// Chiamato quando il 2FA è stato attivato con successo
    /// </summary>
    public void On2FASuccess()
    {
        twoFactorActivated = true;
        twoFactorSkipped = false;
        On2FAActivated?.Invoke();

        CheckAllTasksCompleted();

        // Avvia chiusura automatica con messaggio
        StartCoroutine(ClosePCAfterDelay(missionCompleteMessage));
    }

    /// <summary>
    /// Chiamato quando l'utente salta l'attivazione 2FA
    /// </summary>
    public void On2FASkippedByUser()
    {
        twoFactorSkipped = true;
        twoFactorActivated = false;
        On2FASkipped?.Invoke();

        Debug.Log("[PCInterface] Attivazione 2FA saltata dall'utente");

        CheckAllTasksCompleted();

        // Avvia chiusura automatica con messaggio
        StartCoroutine(ClosePCAfterDelay(missionIncompleteMessage));
    }

    /// <summary>
    /// Coroutine che mostra un messaggio e poi chiude il PC automaticamente
    /// </summary>
    private System.Collections.IEnumerator ClosePCAfterDelay(string message)
    {
        // Mostra messaggio nella schermata corrente (se c'è un feedbackText)
        // Nascondi tutte le schermate e mostra solo il messaggio
        HideAllScreens();

        // Attiva il container se non lo è già
        if (screenContainer != null)
            screenContainer.SetActive(true);

        // Crea un messaggio temporaneo (o usa un panel dedicato)
        ShowTemporaryMessage(message);

        Debug.Log($"[PCInterface] {message}");

        // Aspetta
        yield return new WaitForSeconds(closeDelay);

        // Chiudi il PC
        Close();

        // Disabilita SUBITO l'interazione con il computer (non dipendere dalla durata transizione)
        DisableComputerInteraction();

        // Triggera evento fine missione
        OnMission1Finished?.Invoke();

        Debug.Log("[PCInterface] Missione 1 terminata. PC non più interagibile.");
    }

    /// <summary>
    /// Mostra un messaggio temporaneo a schermo
    /// </summary>
    private void ShowTemporaryMessage(string message)
    {
        if (twoFactorAuthScreen != null && twoFactorAuthScreenObject != null)
        {
            twoFactorAuthScreenObject.SetActive(true);
            twoFactorAuthScreen.ShowFinalMessage(message);
        }
        else
        {
            Debug.Log($"[PCInterface] Messaggio finale: {message}");
        }
    }

    /// <summary>
    /// Disabilita l'interazione con il computer
    /// </summary>
    private void DisableComputerInteraction()
    {
        if (computerInteractable != null)
        {
            computerInteractable.SetEnabled(false);
        }
        else
        {
            // Prova a trovarlo se non è assegnato
            computerInteractable = GetComponentInParent<ComputerInteractable>();
            if (computerInteractable == null)
            {
                computerInteractable = transform.root.GetComponentInChildren<ComputerInteractable>();
            }

            if (computerInteractable != null)
            {
                computerInteractable.SetEnabled(false);
            }
            else
            {
                Debug.LogWarning("[PCInterface] ComputerInteractable non trovato. Impossibile disabilitare l'interazione.");
            }
        }
    }

    private void CheckAllTasksCompleted()
    {
        bool allCompleted = true;

        if (requireLogin && !loginCompleted) allCompleted = false;
        if (requirePasswordChange && !passwordChanged) allCompleted = false;
        if (require2FA && !twoFactorActivated) allCompleted = false;

        if (allCompleted)
        {
            OnAllTasksCompleted?.Invoke();
            Debug.Log("[PCInterface] Tutte le attività della missione 1 completate!");
        }
    }

    #endregion

    #region Metodi pubblici per query stato

    /// <summary>
    /// Verifica se tutte le attività richieste sono completate
    /// </summary>
    public bool AreAllTasksCompleted()
    {
        if (requireLogin && !loginCompleted) return false;
        if (requirePasswordChange && !passwordChanged) return false;
        if (require2FA && !twoFactorActivated) return false;
        return true;
    }

    /// <summary>
    /// Verifica se tutte le attività sono state completate O saltate
    /// </summary>
    public bool AreAllTasksHandled()
    {
        if (requireLogin && !loginCompleted) return false;
        if (requirePasswordChange && !passwordChanged && !passwordSkipped) return false;
        if (require2FA && !twoFactorActivated && !twoFactorSkipped) return false;
        return true;
    }

    /// <summary>
    /// Ottiene il report della missione 1 per la valutazione finale
    /// </summary>
    public Mission1Report GetMission1Report()
    {
        return new Mission1Report
        {
            loginCompleted = this.loginCompleted,
            passwordChanged = this.passwordChanged,
            passwordSkipped = this.passwordSkipped,
            twoFactorActivated = this.twoFactorActivated,
            twoFactorSkipped = this.twoFactorSkipped
        };
    }

    /// <summary>
    /// Resetta lo stato (utile per testing)
    /// </summary>
    public void ResetState()
    {
        loginCompleted = false;
        passwordChanged = false;
        passwordSkipped = false;
        twoFactorActivated = false;
        twoFactorSkipped = false;
        currentState = PCState.Inactive;
    }

    /// <summary>
    /// Forza la chiusura immediata (per emergenze/debug)
    /// </summary>
    public void ForceClose()
    {
        if (cameraController != null)
        {
            cameraController.ForceReturnToPlayer();
        }

        HideAllScreens();
        if (screenContainer != null)
            screenContainer.SetActive(false);

        isOpen = false;
        currentState = PCState.Inactive;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnInterfaceClosed?.Invoke();
    }

    #endregion
}

/// <summary>
/// Struttura che contiene i dati del report per la missione 1.
/// Usata per la valutazione finale e il feedback al giocatore.
/// </summary>
[System.Serializable]
public class Mission1Report
{
    public bool loginCompleted;
    public bool passwordChanged;
    public bool passwordSkipped;
    public bool twoFactorActivated;
    public bool twoFactorSkipped;

    /// <summary>
    /// Calcola il punteggio della missione (0-100)
    /// </summary>
    public int CalculateScore()
    {
        int score = 0;

        // Login obbligatorio: 20 punti
        if (loginCompleted) score += 20;

        // Password: 40 punti se cambiata, 0 se saltata
        if (passwordChanged) score += 40;

        // 2FA: 40 punti se attivato, 0 se saltato
        if (twoFactorActivated) score += 40;

        return score;
    }

    /// <summary>
    /// Verifica se la missione è stata completata correttamente (tutte le azioni fatte)
    /// </summary>
    public bool IsFullyCompleted()
    {
        return loginCompleted && passwordChanged && twoFactorActivated;
    }

    /// <summary>
    /// Verifica se ci sono stati errori di sicurezza (skip)
    /// </summary>
    public bool HasSecurityIssues()
    {
        return passwordSkipped || twoFactorSkipped;
    }

    /// <summary>
    /// Ottiene un elenco degli errori commessi
    /// </summary>
    public string[] GetSecurityIssues()
    {
        var issues = new System.Collections.Generic.List<string>();

        if (passwordSkipped)
            issues.Add("Non hai cambiato la password predefinita. Questo rende il tuo account vulnerabile.");

        if (twoFactorSkipped)
            issues.Add("Non hai attivato l'autenticazione a due fattori. Il tuo account è meno protetto.");

        return issues.ToArray();
    }

    /// <summary>
    /// Ottiene un elenco delle azioni corrette
    /// </summary>
    public string[] GetCorrectActions()
    {
        var actions = new System.Collections.Generic.List<string>();

        if (loginCompleted)
            actions.Add("Hai effettuato l'accesso al sistema.");

        if (passwordChanged)
            actions.Add("Hai cambiato la password con una più sicura.");

        if (twoFactorActivated)
            actions.Add("Hai attivato l'autenticazione a due fattori.");

        return actions.ToArray();
    }
}