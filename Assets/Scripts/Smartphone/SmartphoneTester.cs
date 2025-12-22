using UnityEngine;

/// <summary>
/// Script di test per inviare messaggi allo smartphone.
/// Usalo per testare il sistema durante lo sviluppo.
/// RIMUOVI O DISABILITA IN PRODUZIONE.
/// </summary>
public class SmartphoneTester : MonoBehaviour
{
    [Header("Test Messages")]
    [SerializeField] private string testSender = "Filippo Giorgi";
    [SerializeField][TextArea] private string testMessage = "Ciao! Ho bisogno del tuo aiuto con un problema di sicurezza. Vieni alla mia postazione quando puoi.";

    [Header("Target da evidenziare (opzionale)")]
    [SerializeField] private GameObject targetNPC;

    [Header("Messaggi Predefiniti")]
    [SerializeField] private SmartphoneMessage[] predefinedMessages;

    private SmartphoneManager manager;

    private void Start()
    {
        manager = SmartphoneManager.Instance;
    }

    private void Update()
    {
        if (manager == null) return;

        // Premi M per inviare un messaggio di test
        if (Input.GetKeyDown(KeyCode.M))
        {
            SendTestMessage();
        }

        // Premi N per inviare un messaggio predefinito random
        if (Input.GetKeyDown(KeyCode.N))
        {
            SendRandomPredefinedMessage();
        }
    }

    /// <summary>
    /// Invia il messaggio di test configurato nell'Inspector.
    /// </summary>
    public void SendTestMessage()
    {
        if (manager == null)
        {
            Debug.LogError("[SmartphoneTester] SmartphoneManager non trovato!");
            return;
        }

        manager.ReceiveMessage(testSender, testMessage, targetNPC);
        Debug.Log($"[SmartphoneTester] Messaggio inviato da {testSender}");
    }

    /// <summary>
    /// Invia un messaggio predefinito casuale.
    /// </summary>
    public void SendRandomPredefinedMessage()
    {
        if (manager == null || predefinedMessages == null || predefinedMessages.Length == 0)
        {
            Debug.LogWarning("[SmartphoneTester] Nessun messaggio predefinito disponibile");
            return;
        }

        int randomIndex = Random.Range(0, predefinedMessages.Length);
        var message = predefinedMessages[randomIndex];

        // Crea una copia per non modificare l'originale
        var messageCopy = new SmartphoneMessage
        {
            senderName = message.senderName,
            messageText = message.messageText,
            senderIcon = message.senderIcon,
            targetToHighlight = message.targetToHighlight
        };

        manager.ReceiveMessage(messageCopy);
        Debug.Log($"[SmartphoneTester] Messaggio predefinito inviato da {messageCopy.senderName}");
    }

    /// <summary>
    /// Metodo pubblico per inviare messaggi da altri script o eventi Unity.
    /// </summary>
    public void SendMessage(string sender, string text)
    {
        manager?.ReceiveMessage(sender, text);
    }
}