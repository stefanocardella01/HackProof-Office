using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject che definisce una missione e i suoi obiettivi.
/// Creare un asset per ogni missione: Create > HackProof > Mission Data.
/// </summary>
[CreateAssetMenu(menuName = "HackProof/Mission Data", fileName = "NewMissionData")]
public class MissionData : ScriptableObject
{
    [Header("Informazioni Missione")]
    [Tooltip("ID univoco della missione (es: mission_1)")]
    public string missionId;

    [Tooltip("Titolo mostrato nella checklist (es: Missione 1 - Primo giorno)")]
    public string missionTitle;

    [Header("Obiettivi")]
    [Tooltip("Lista degli obiettivi di questa missione, in ordine")]
    public List<MissionObjectiveData> objectives = new();

    [Header("Trigger Iniziale")]
    [Tooltip("Se true, la missione inizia automaticamente (Missione 1). " +
             "Se false, viene triggerata da messaggio smartphone o altro evento.")]
    public bool startsAutomatically = false;
}
