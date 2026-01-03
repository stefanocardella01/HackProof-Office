using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Report della missione email.
/// Contiene tutte le scelte del giocatore e metodi per calcolare il punteggio.
/// Usato per il report finale delle missioni.
/// </summary>
[System.Serializable]
public class EmailMissionReport
{
    public List<EmailChoice> choices = new List<EmailChoice>();
    public int totalEmails;
    public int correctAnswers;
    public int wrongAnswers;

    /// <summary>
    /// Aggiunge una scelta al report
    /// </summary>
    public void AddChoice(EmailChoice choice)
    {
        choices.Add(choice);
        totalEmails = choices.Count;

        if (choice.isCorrect)
            correctAnswers++;
        else
            wrongAnswers++;
    }

    /// <summary>
    /// Calcola il punteggio della missione (0-100)
    /// </summary>
    public int CalculateScore()
    {
        if (totalEmails == 0) return 0;
        return Mathf.RoundToInt((float)correctAnswers / totalEmails * 100f);
    }

    /// <summary>
    /// Verifica se tutte le risposte sono corrette
    /// </summary>
    public bool IsPerfect()
    {
        return correctAnswers == totalEmails && totalEmails > 0;
    }

    /// <summary>
    /// Ottiene le email classificate correttamente
    /// </summary>
    public EmailChoice[] GetCorrectChoices()
    {
        return choices.FindAll(c => c.isCorrect).ToArray();
    }

    /// <summary>
    /// Ottiene le email classificate erroneamente
    /// </summary>
    public EmailChoice[] GetWrongChoices()
    {
        return choices.FindAll(c => !c.isCorrect).ToArray();
    }

    /// <summary>
    /// Ottiene un elenco delle azioni corrette per il report finale
    /// </summary>
    public string[] GetCorrectActions()
    {
        var actions = new List<string>();

        foreach (var choice in choices)
        {
            if (choice.isCorrect)
            {
                string typeStr = choice.correctAnswer == EmailType.Phishing ? "phishing" : "legittima";
                actions.Add($"Hai correttamente identificato \"{choice.emailSubject}\" come {typeStr}.");
            }
        }

        return actions.ToArray();
    }

    /// <summary>
    /// Ottiene un elenco degli errori commessi per il report finale
    /// </summary>
    public string[] GetSecurityIssues()
    {
        var issues = new List<string>();

        foreach (var choice in choices)
        {
            if (!choice.isCorrect)
            {
                string playerTypeStr = choice.playerChoice == EmailType.Phishing ? "phishing" : "legittima";
                string correctTypeStr = choice.correctAnswer == EmailType.Phishing ? "phishing" : "legittima";

                issues.Add($"Hai classificato \"{choice.emailSubject}\" come {playerTypeStr}, ma era {correctTypeStr}. {choice.explanation}");
            }
        }

        return issues.ToArray();
    }

    /// <summary>
    /// Ottiene un riepilogo testuale del report
    /// </summary>
    public string GetSummary()
    {
        return $"Hai identificato correttamente {correctAnswers} email su {totalEmails}. Punteggio: {CalculateScore()}%";
    }

    /// <summary>
    /// Verifica se ci sono stati errori
    /// </summary>
    public bool HasSecurityIssues()
    {
        return wrongAnswers > 0;
    }
}