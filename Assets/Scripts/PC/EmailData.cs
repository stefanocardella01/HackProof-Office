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
    [Header("Contenuto Visivo")]
    [Tooltip("Lo screenshot dell'email preso dal quiz di Google")]
    public Sprite emailImage;

    [Header("Dati per il Report")]
    [Tooltip("Nome breve per identificare l'email nel report finale")]
    public string subject;

    [Header("Dati per il Tooltip")]
    public string visibleUrl;      // Il testo da mostrare nel tooltip (es: "http://falso.com")

    [Header("Classificazione")]
    public EmailType correctType;

    [Header("Feedback")]
    [TextArea(3, 8)]
    public string explanation;

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