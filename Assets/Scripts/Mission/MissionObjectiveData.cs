using UnityEngine;

/// <summary>
/// Dati di un singolo obiettivo di missione.
/// Configurabile da Inspector tramite MissionData ScriptableObject.
/// </summary>
[System.Serializable]
public class MissionObjectiveData
{
    [Tooltip("ID univoco dell'obiettivo (es: m1_talk_receptionist)")]
    public string objectiveId;

    [Tooltip("Testo mostrato nella checklist")]
    public string displayText;

    [Tooltip("Quante volte va completato (1 = normale, 3 = es. sala relax 0/3)")]
    public int requiredCount = 1;

    [Tooltip("Mostra il contatore nella checklist (es: 0/3)")]
    public bool showCounter = false;

    [Tooltip("Visibile all'inizio della missione o sbloccato dopo?")]
    public bool visibleAtStart = false;

    [Tooltip("Lista di objectiveId che devono essere completati per rendere visibile questo obiettivo. " +
             "Se vuoto e visibleAtStart=false, va sbloccato manualmente con RevealObjective().")]
    public string[] unlockedAfter = new string[0];
}
