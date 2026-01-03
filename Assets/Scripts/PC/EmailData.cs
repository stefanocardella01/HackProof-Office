using UnityEngine;

/// <summary>
/// Enum per il tipo di email
/// </summary>
public enum EmailType
{
    Legitimate,
    Phishing
}

/// <summary>
/// Contiene tutti i dati di una singola email.
/// Usato per definire le email hardcoded nella missione.
/// </summary>
[System.Serializable]
public class EmailData
{
    [Header("Mittente")]
    [Tooltip("Nome visualizzato del mittente")]
    public string senderName;

    [Tooltip("Indirizzo email del mittente")]
    public string senderEmail;

    [Header("Contenuto")]
    [Tooltip("Oggetto dell'email")]
    public string subject;

    [Tooltip("Corpo del messaggio")]
    [TextArea(5, 15)]
    public string body;

    [Header("URL")]
    [Tooltip("URL del link (può essere sospetto per email di phishing)")]
    public string url;

    [Header("Classificazione")]
    [Tooltip("Il tipo corretto di questa email")]
    public EmailType correctType;

    [Header("Feedback Educativo")]
    [Tooltip("Spiegazione del perché questa email è phishing/legittima")]
    [TextArea(3, 8)]
    public string explanation;

    [Tooltip("Indizi che il giocatore dovrebbe notare")]
    public string[] hints;

    /// <summary>
    /// Verifica se la scelta del giocatore è corretta
    /// </summary>
    public bool IsChoiceCorrect(EmailType playerChoice)
    {
        return playerChoice == correctType;
    }
}

/// <summary>
/// Contiene il risultato della scelta del giocatore per una singola email
/// </summary>
[System.Serializable]
public class EmailChoice
{
    public int emailIndex;
    public EmailType playerChoice;
    public EmailType correctAnswer;
    public bool isCorrect;
    public string emailSubject;
    public string explanation;

    public EmailChoice(int index, EmailType choice, EmailData emailData)
    {
        emailIndex = index;
        playerChoice = choice;
        correctAnswer = emailData.correctType;
        isCorrect = emailData.IsChoiceCorrect(choice);
        emailSubject = emailData.subject;
        explanation = emailData.explanation;
    }
}