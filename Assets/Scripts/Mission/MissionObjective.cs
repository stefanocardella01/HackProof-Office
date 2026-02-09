using System;

/// <summary>
/// Stato runtime di un singolo obiettivo.
/// Creato dal MissionManager a partire da MissionObjectiveData.
/// </summary>
public class MissionObjective
{
    public string ObjectiveId { get; private set; }
    public string DisplayText { get; private set; }
    public int RequiredCount { get; private set; }
    public int CurrentCount { get; private set; }
    public bool ShowCounter { get; private set; }
    public bool IsVisible { get; private set; }
    public bool IsCompleted => CurrentCount >= RequiredCount;
    public string[] UnlockedAfter { get; private set; }

    /// <summary>
    /// Evento invocato quando lo stato dell'obiettivo cambia
    /// (progresso, completamento, visibilità).
    /// </summary>
    public event Action<MissionObjective> OnStateChanged;

    public MissionObjective(MissionObjectiveData data, bool visible)
    {
        ObjectiveId = data.objectiveId;
        DisplayText = data.displayText;
        RequiredCount = data.requiredCount;
        ShowCounter = data.showCounter;
        UnlockedAfter = data.unlockedAfter ?? new string[0];
        CurrentCount = 0;
        IsVisible = visible;
    }

    /// <summary>
    /// Completa l'obiettivo immediatamente (count = required).
    /// </summary>
    public bool Complete()
    {
        if (IsCompleted) return false;

        CurrentCount = RequiredCount;
        OnStateChanged?.Invoke(this);
        return true;
    }

    /// <summary>
    /// Avanza il contatore di un valore (per obiettivi con counter, es: sala relax).
    /// </summary>
    public bool Advance(int amount = 1)
    {
        if (IsCompleted) return false;

        CurrentCount = Math.Min(CurrentCount + amount, RequiredCount);
        OnStateChanged?.Invoke(this);
        return true;
    }

    /// <summary>
    /// Rende l'obiettivo visibile nella checklist.
    /// </summary>
    public void Reveal()
    {
        if (IsVisible) return;

        IsVisible = true;
        OnStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Testo formattato per la checklist, con eventuale contatore.
    /// </summary>
    public string GetDisplayText()
    {
        if (ShowCounter && RequiredCount > 1)
            return $"{DisplayText} ({CurrentCount}/{RequiredCount})";

        return DisplayText;
    }
}
