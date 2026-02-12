using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using StarterAssets;

/// <summary>
/// Manager principale dell'interfaccia email.
/// Gestisce il flusso delle email, le scelte del giocatore e genera il report.
/// </summary>
public class EmailInterfaceManager : MonoBehaviour
{
    public enum EmailMissionState
    {
        Inactive,
        TransitioningIn,
        ShowingEmail,
        TransitioningOut,
        Completed
    }

    [Header("Email da Classificare")]
    [SerializeField] private EmailData[] emails = new EmailData[4];

    [Header("Riferimenti UI")]
    [SerializeField] private Canvas emailCanvas;
    [SerializeField] private GameObject screenContainer;
    [SerializeField] private EmailScreen emailScreen;

    [SerializeField] private ReportUI reportUI;

    [Header("Camera e Controlli")]
    [SerializeField] private PCCameraController cameraController;

    [Header("Riferimento Computer")]
    [SerializeField] private EmailComputerInteractable computerInteractable;

    [Header("Configurazione")]
    [SerializeField] private float closeDelay = 0.5f;  // Breve pausa prima di chiudere

    [Header("Eventi")]
    public UnityEvent OnEmailMissionStarted;
    public UnityEvent OnEmailAnswered;
    public UnityEvent OnEmailMissionCompleted;
    public UnityEvent OnInterfaceOpened;
    public UnityEvent OnInterfaceClosed;

    // Stato
    private EmailMissionState currentState = EmailMissionState.Inactive;
    private int currentEmailIndex = 0;
    private bool isOpen = false;
    private EmailMissionReport report;

    // Riferimento al PlayerInteractor
    private PlayerInteractor playerInteractor;

    // Properties pubbliche
    public EmailMissionState CurrentState => currentState;
    public bool IsOpen => isOpen;
    public int CurrentEmailIndex => currentEmailIndex;
    public int TotalEmails => emails.Length;
    public EmailMissionReport Report => report;

    // Audio
    [SerializeField] private ManagerAudio mixer;

    private void Awake()
    {
        // Setup camera controller
        SetupCameraController();

        // Inizializza il report
        report = new EmailMissionReport();

        // Assicura riferimento al ComputerInteractable
        if (computerInteractable == null)
        {
            computerInteractable = GetComponentInParent<EmailComputerInteractable>();
            if (computerInteractable == null)
                computerInteractable = GetComponentInChildren<EmailComputerInteractable>(true);
        }

        // Nascondi tutto all'avvio
        if (screenContainer != null)
            screenContainer.SetActive(false);

        // Inizializza la schermata email
        if (emailScreen != null)
            emailScreen.Initialize(this);

        if (reportUI == null) reportUI = FindFirstObjectByType<ReportUI>();

        // Popola le email di default se non sono state configurate
        /* if (emails == null || emails.Length == 0 || emails[0] == null || string.IsNullOrEmpty(emails[0].senderName))
         {
             PopulateDefaultEmails();
         }*/
    }

    private void SetupCameraController()
    {
        if (cameraController == null)
        {
            cameraController = GetComponent<PCCameraController>();

            if (cameraController == null)
            {
                Debug.LogError("[EmailInterfaceManager] PCCameraController non trovato!");
                return;
            }
        }

        // Registra callback
        cameraController.OnTransitionToScreenComplete = OnCameraArrivedAtScreen;
        cameraController.OnTransitionBackComplete = OnCameraReturnedToPlayer;
    }

    /// <summary>
    /// Popola le email con contenuti di default (4 email hardcoded)
    /// </summary>
   /* private void PopulateDefaultEmails()
    {
        emails = new EmailData[4];

        // EMAIL 1: Phishing - Finta fattura PayPal
        emails[0] = new EmailData
        {
            senderName = "Reparto Fatturazione",
            senderEmail = "service@paypa1.com",
            subject = "Ecco la tua fattura",
            body = "Il reparto fatturazione ti ha inviato una fattura di 600,00 $\n\n" +
                   "Scadenza alla ricezione.\n\n" +
                   "Dettagli fattura\n" +
                   "Importo richiesto: 600,00 $\n\n" +
                   "Nota del venditore:\n" +
                   "Ci sono prove di un accesso non autorizzato al tuo conto PayPal. " +
                   "Sono stati addebitati 600,00 $ sul tuo conto per l'acquisto di una carta regalo elettronica. " +
                   "La transazione verrà visualizzata nella tua attività PayPal entro 24 ore. " +
                   "Se non riconosci questa transazione, contattaci subito al numero verde +1 (858-555-7823). " +
                   "Siamo disponibili dal lunedì al venerdì dalle 06:00 alle 18:00 (fuso orario del Pacifico).\n\n" +
                   "Numero fattura: 1031",
            url = "http://paypa1-secure.billing-check.com/invoice",
            correctType = EmailType.Phishing,
            explanation = "Questa è un'email di phishing. L'indirizzo del mittente 'paypa1.com' usa il numero 1 invece della lettera 'l'. " +
                         "PayPal non invia mai fatture non richieste e non chiede di chiamare numeri di telefono sconosciuti.",
            hints = new string[] {
                "Controlla l'indirizzo email: paypa1.com invece di paypal.com",
                "Urgenza e paura sono tattiche comuni di phishing",
                "Il numero di telefono non è quello ufficiale di PayPal"
            }
        };

        // EMAIL 2: Legittima - Email dal reparto IT aziendale
        emails[1] = new EmailData
        {
            senderName = "Marco Bianchi - IT Support",
            senderEmail = "m.bianchi@hackproof-office.it",
            subject = "Aggiornamento software obbligatorio - Entro venerdì",
            body = "Ciao,\n\n" +
                   "Ti ricordo che entro venerdì è necessario aggiornare il client VPN aziendale alla versione 3.2.1.\n\n" +
                   "L'aggiornamento è già disponibile nel Software Center del tuo PC. " +
                   "Se hai bisogno di assistenza, passa pure in ufficio IT (stanza 204) o chiamami all'interno 4521.\n\n" +
                   "Grazie per la collaborazione!\n\n" +
                   "Marco Bianchi\n" +
                   "IT Support - HackProof Office\n" +
                   "Interno: 4521",
            url = "software-center://updates",
            correctType = EmailType.Legitimate,
            explanation = "Questa è un'email legittima. Il mittente usa il dominio aziendale corretto (@hackproof-office.it), " +
                         "fornisce un contatto verificabile (interno 4521, stanza 204) e non richiede azioni urgenti o dati sensibili.",
            hints = new string[] {
                "Il dominio email corrisponde a quello aziendale",
                "Il mittente fornisce contatti verificabili",
                "Non chiede password o dati sensibili"
            }
        };

        // EMAIL 3: Phishing - Finto reset password Microsoft
        emails[2] = new EmailData
        {
            senderName = "Microsoft Account Team",
            senderEmail = "security@microsoft-account-verify.com",
            subject = "Azione richiesta: La tua password scadrà tra 24 ore",
            body = "Gentile utente,\n\n" +
                   "Abbiamo rilevato che la password del tuo account Microsoft Office 365 scadrà tra 24 ore.\n\n" +
                   "Per evitare l'interruzione dei servizi, è necessario aggiornare immediatamente la password " +
                   "cliccando sul pulsante sottostante.\n\n" +
                   "ATTENZIONE: Se non aggiorni la password entro 24 ore, il tuo account verrà sospeso " +
                   "e perderai l'accesso a tutti i documenti e le email.\n\n" +
                   "Cordiali saluti,\n" +
                   "Microsoft Security Team",
            url = "http://microsoft-account-verify.com/password/reset",
            correctType = EmailType.Phishing,
            explanation = "Questa è un'email di phishing. Microsoft non usa domini come 'microsoft-account-verify.com'. " +
                         "Le email legittime di Microsoft provengono da @microsoft.com. L'urgenza estrema e le minacce sono segnali di phishing.",
            hints = new string[] {
                "Il dominio 'microsoft-account-verify.com' non è di Microsoft",
                "Microsoft non minaccia mai la sospensione immediata dell'account",
                "L'uso di emoji nel subject è insolito per email aziendali"
            }
        };

        // EMAIL 4: Legittima - Conferma riunione da un collega
        emails[3] = new EmailData
        {
            senderName = "Laura Rossi",
            senderEmail = "l.rossi@hackproof-office.it",
            subject = "Re: Riunione progetto Q1 - Conferma partecipazione",
            body = "Ciao!\n\n" +
                   "Confermo la mia partecipazione alla riunione di domani alle 14:30 in sala Meeting B.\n\n" +
                   "Ho già preparato la presentazione sui risultati del Q1 come richiesto. " +
                   "Se hai bisogno che aggiunga qualcosa, fammi sapere entro oggi pomeriggio.\n\n" +
                   "A domani!\n\n" +
                   "Laura\n\n" +
                   "---\n" +
                   "Laura Rossi\n" +
                   "Project Manager\n" +
                   "HackProof Office S.r.l.",
            url = "calendar://add?event=meeting-q1",
            correctType = EmailType.Legitimate,
            explanation = "Questa è un'email legittima. Proviene dal dominio aziendale corretto, " +
                         "fa riferimento a un contesto lavorativo reale (riunione Q1) e non richiede azioni sospette o dati sensibili.",
            hints = new string[] {
                "Il dominio email è quello aziendale corretto",
                "Il contesto è coerente con l'ambiente lavorativo",
                "Non ci sono richieste urgenti o minacce"
            }
        };

        Debug.Log("[EmailInterfaceManager] Email di default caricate");
    }
   */

    /// <summary>
    /// Apre l'interfaccia email con transizione camera
    /// </summary>
    public void Open(PlayerInteractor interactor)
    {
        Debug.Log("[EmailInterfaceManager] Open() chiamato");
        mixer.SetDialog();

        if (isOpen)
        {
            Debug.LogWarning("[EmailInterfaceManager] Già aperto, ignoro");
            return;
        }

        if (currentState == EmailMissionState.Completed)
        {
            Debug.LogWarning("[EmailInterfaceManager] Missione già completata");
            return;
        }

        playerInteractor = interactor;
        isOpen = true;
        currentState = EmailMissionState.TransitioningIn;
        currentEmailIndex = 0;
        report = new EmailMissionReport();

        // Avvia transizione camera
        if (cameraController != null)
        {
            Debug.Log("[EmailInterfaceManager] Avvio transizione camera");
            cameraController.TransitionToScreen();
        }
        else
        {
            Debug.LogWarning("[EmailInterfaceManager] CameraController non trovato, vado diretto");
            OnCameraArrivedAtScreen();
        }

        OnInterfaceOpened?.Invoke();
        OnEmailMissionStarted?.Invoke();
    }

    /// <summary>
    /// Chiamato quando la camera arriva davanti allo schermo
    /// </summary>
    private void OnCameraArrivedAtScreen()
    {
        Debug.Log("[EmailInterfaceManager] Camera arrivata allo schermo");

        // Attiva il container UI
        if (screenContainer != null)
            screenContainer.SetActive(true);

        // Mostra la prima email
        currentState = EmailMissionState.ShowingEmail;
        ShowCurrentEmail();
    }

    /// <summary>
    /// Mostra l'email corrente
    /// </summary>
    private void ShowCurrentEmail()
    {
        if (currentEmailIndex >= emails.Length)
        {
            Debug.LogWarning("[EmailInterfaceManager] Indice email fuori range");
            return;
        }

        EmailData currentEmail = emails[currentEmailIndex];

        if (emailScreen != null)
        {
            emailScreen.ShowEmail(currentEmail, currentEmailIndex + 1, emails.Length);
        }

        Debug.Log($"[EmailInterfaceManager] Mostrando email {currentEmailIndex + 1}/{emails.Length}: {currentEmail.subject}");
    }

    /// <summary>
    /// Chiamato quando il giocatore fa una scelta (da EmailScreen)
    /// </summary>
    /// 
    private void RestorePlayerControls()
    {
        var inputs = FindFirstObjectByType<StarterAssetsInputs>();
        var fps = FindFirstObjectByType<StarterAssets.FirstPersonController>();

        if (fps != null) fps.enabled = true;

        if (inputs != null)
        {
            inputs.cursorInputForLook = true;
            inputs.move = Vector2.zero;
            inputs.look = Vector2.zero;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void OnPlayerChoice(EmailType choice)
    {
        if (currentState != EmailMissionState.ShowingEmail) return;

        EmailData currentEmail = emails[currentEmailIndex];
        bool isCorrect = currentEmail.IsChoiceCorrect(choice);

        // salva nel tuo report interno (già lo fai)
        EmailChoice emailChoice = new EmailChoice(currentEmailIndex, choice, currentEmail);
        report.AddChoice(emailChoice);

        // ✅ salva anche nel MissionTracker (così il report finale missione 4 può leggerlo)
        if (MissionTracker.Instance != null)
        {
            var check = (ReportCheck)((int)ReportCheck.Email1Correct + currentEmailIndex);
            MissionTracker.Instance.Set(check, isCorrect);
        }

        // ✅ apri micro-report e aspetta "continua"
        currentState = EmailMissionState.TransitioningOut;

        if (reportUI != null)
        {
            reportUI.OpenSingleFeedback(
                title: $"Email {currentEmailIndex + 1}/{emails.Length}",
                label: currentEmail.subject,
                ok: isCorrect,
                explanation: currentEmail.explanation,
                onClosed: () =>
                {
                    // ora vai avanti
                    currentEmailIndex++;

                    if (currentEmailIndex < emails.Length)
                    {
                        currentState = EmailMissionState.ShowingEmail;
                        ShowCurrentEmail();
                    }
                    else
                    {
                        StartCoroutine(CompleteMission());
                    }
                }
            );
        }
        else
        {
            // fallback: se non c'è reportUI vai avanti senza feedback
            currentEmailIndex++;
            if (currentEmailIndex < emails.Length) ShowCurrentEmail();
            else StartCoroutine(CompleteMission());
        }
    }

    /// <summary>
    /// Completa la missione e chiude il PC
    /// </summary>
    private IEnumerator CompleteMission()
    {
        Debug.Log("[EmailInterfaceManager] Missione email completata!");
        Debug.Log($"[EmailInterfaceManager] {report.GetSummary()}");

        currentState = EmailMissionState.Completed;

        // Breve pausa prima di chiudere
        yield return new WaitForSeconds(closeDelay);

        // Chiudi il PC
        Close();

        RestorePlayerControls();

        // Disabilita l'interazione col computer
        DisableComputerInteraction();

        int correct = 0;

        for (int i = 0; i < emails.Length; i++)
        {
            var check = (ReportCheck)((int)ReportCheck.Email1Correct + i);
            if (MissionTracker.Instance.Get(check))
                correct++;
        }

        // salva il punteggio in una variabile globale
        EmailFinalScore.LastScore = correct;
        EmailFinalScore.LastTotal = emails.Length;

        // segna il check come true così la riga appare nel report
        MissionTracker.Instance.Set(ReportCheck.EmailScore, true);

        // Triggera evento fine missione (il sistema missioni userà il report)
        OnEmailMissionCompleted?.Invoke();

        Debug.Log($"[Email] FinalScore={EmailFinalScore.LastScore}/{EmailFinalScore.LastTotal}");

        Debug.Log("[EmailInterfaceManager] Missione email terminata. PC non più interagibile.");

        var reportUI = FindFirstObjectByType<ReportUI>();
        if (reportUI != null)
            reportUI.OpenFinalReport();
    }

    /// <summary>
    /// Chiude l'interfaccia email
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;

        currentState = EmailMissionState.TransitioningOut;
        mixer.SetNormal();

        // Nascondi UI
        if (screenContainer != null)
            screenContainer.SetActive(false);

        // Avvia transizione camera indietro
        if (cameraController != null)
        {
            cameraController.TransitionBack();
        }
        else
        {
            OnCameraReturnedToPlayer();
        }
    }

    /// <summary>
    /// Chiamato quando la camera torna al player
    /// </summary>
    private void OnCameraReturnedToPlayer()
    {
        Debug.Log("[EmailInterfaceManager] Camera tornata al player");

        isOpen = false;

        // Non cambiare lo stato se la missione è completata
        if (currentState != EmailMissionState.Completed)
        {
            currentState = EmailMissionState.Inactive;
        }

        OnInterfaceClosed?.Invoke();
    }

    /// <summary>
    /// Disabilita l'interazione col computer
    /// </summary>
    private void DisableComputerInteraction()
    {
        if (computerInteractable != null)
        {
            computerInteractable.SetEnabled(false);
            Debug.Log("[EmailInterfaceManager] Computer disabilitato");
        }
        else
        {
            // Prova a trovarlo
            computerInteractable = GetComponentInParent<EmailComputerInteractable>();
            if (computerInteractable == null)
                computerInteractable = transform.root.GetComponentInChildren<EmailComputerInteractable>();

            if (computerInteractable != null)
            {
                computerInteractable.SetEnabled(false);
                Debug.Log("[EmailInterfaceManager] Computer disabilitato (trovato automaticamente)");
            }
            else
            {
                Debug.LogWarning("[EmailInterfaceManager] ComputerInteractable non trovato");
            }
        }
    }

    /// <summary>
    /// Ottiene il report della missione
    /// </summary>
    public EmailMissionReport GetEmailMissionReport()
    {
        return report;
    }
}