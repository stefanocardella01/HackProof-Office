using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager centrale delle missioni. Singleton persistente.
/// Gestisce la progressione delle missioni, il completamento degli obiettivi
/// e comunica con la ChecklistUI tramite eventi.
/// 
/// USO DA ALTRI SCRIPT:
///   MissionManager.Instance.CompleteObjective("m1_talk_receptionist");
///   MissionManager.Instance.AdvanceObjective("m3_inspect_relax", 1);
///   MissionManager.Instance.RevealObjective("m2_deliver_items");
/// </summary>
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Missioni (in ordine)")]
    [SerializeField] private List<MissionData> missions = new();

    // ── Eventi ──────────────────────────────────────────────
    /// <summary>Una nuova missione è iniziata.</summary>
    public event Action<int, MissionData> OnMissionStarted;

    /// <summary>Un obiettivo è diventato visibile nella checklist.</summary>
    public event Action<MissionObjective> OnObjectiveRevealed;

    /// <summary>Un obiettivo è stato aggiornato (progresso/completamento).</summary>
    public event Action<MissionObjective> OnObjectiveUpdated;

    /// <summary>Un obiettivo è stato completato.</summary>
    public event Action<MissionObjective> OnObjectiveCompleted;

    /// <summary>Tutti gli obiettivi della missione corrente sono completati.</summary>
    public event Action<int, MissionData> OnMissionCompleted;

    /// <summary>Tutte le missioni sono state completate (fine gioco).</summary>
    public event Action OnAllMissionsCompleted;

    // ── Stato Runtime ───────────────────────────────────────
    private int currentMissionIndex = -1;
    private List<MissionObjective> currentObjectives = new();
    private bool missionActive = false;

    // ── Properties ──────────────────────────────────────────
    public int CurrentMissionIndex => currentMissionIndex;
    public MissionData CurrentMissionData => (currentMissionIndex >= 0 && currentMissionIndex < missions.Count)
        ? missions[currentMissionIndex] : null;
    public IReadOnlyList<MissionObjective> CurrentObjectives => currentObjectives.AsReadOnly();
    public bool IsMissionActive => missionActive;

    // ─────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[MissionManager] Awake -> missions.Count = {missions.Count}");

        if (missions.Count > 0)
        {
            Debug.Log($"[MissionManager] First mission autoStart = {missions[0].startsAutomatically}");
        }

        // Avvia automaticamente la prima missione se configurata
        if (missions.Count > 0 && missions[0].startsAutomatically)
        {
            Debug.Log("[MissionManager] Starting Mission 0");
            StartMission(0);
        }
    }


    #endregion

    // ─────────────────────────────────────────────────────────
    #region API Pubblica — Missioni

    /// <summary>
    /// Avvia una missione specifica per indice.
    /// </summary>
    public void StartMission(int index)
    {
        if (index < 0 || index >= missions.Count)
        {
            Debug.LogError($"[MissionManager] Indice missione non valido: {index}");
            return;
        }

        currentMissionIndex = index;
        missionActive = true;
        var missionData = missions[index];

        // Crea gli obiettivi runtime
        currentObjectives.Clear();
        foreach (var objData in missionData.objectives)
        {
            bool visible = objData.visibleAtStart;
            var objective = new MissionObjective(objData, visible);
            objective.OnStateChanged += HandleObjectiveStateChanged;
            currentObjectives.Add(objective);
        }

        Debug.Log($"[MissionManager] ══════ Missione {index + 1} iniziata: {missionData.missionTitle} ══════");
        OnMissionStarted?.Invoke(index, missionData);

        // Rivela gli obiettivi visibili all'inizio
        foreach (var obj in currentObjectives)
        {
            if (obj.IsVisible)
            {
                OnObjectiveRevealed?.Invoke(obj);
                Debug.Log($"[MissionManager]   ☐ {obj.GetDisplayText()}");
            }
        }
    }

    /// <summary>
    /// Avvia la prossima missione. Chiamato tipicamente dopo la chiusura del report.
    /// </summary>
    public void StartNextMission()
    {
        int nextIndex = currentMissionIndex + 1;
        if (nextIndex < missions.Count)
        {
            StartMission(nextIndex);
        }
        else
        {
            Debug.Log("[MissionManager] ══════ TUTTE LE MISSIONI COMPLETATE — FINE GIOCO ══════");
            OnAllMissionsCompleted?.Invoke();
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────
    #region API Pubblica — Obiettivi

    /// <summary>
    /// Completa un obiettivo immediatamente.
    /// Usato per obiettivi semplici (parla con X, vai a Y).
    /// </summary>
    public void CompleteObjective(string objectiveId)
    {
        var objective = FindObjective(objectiveId);
        if (objective == null)
        {
            Debug.LogWarning($"[MissionManager] Obiettivo non trovato: {objectiveId}");
            return;
        }

        if (objective.IsCompleted)
        {
            Debug.LogWarning($"[MissionManager] Obiettivo già completato: {objectiveId}");
            return;
        }

        if (!objective.IsVisible)
        {
            Debug.LogWarning($"[MissionManager] Obiettivo non ancora visibile: {objectiveId}. Rivelalo prima.");
            return;
        }

        objective.Complete();
        Debug.Log($"[MissionManager]   ☑ {objective.GetDisplayText()}");
    }

    /// <summary>
    /// Avanza il contatore di un obiettivo (es: sala relax 0/3 → 1/3).
    /// </summary>
    public void AdvanceObjective(string objectiveId, int amount = 1)
    {
        var objective = FindObjective(objectiveId);
        if (objective == null)
        {
            Debug.LogWarning($"[MissionManager] Obiettivo non trovato: {objectiveId}");
            return;
        }

        if (objective.IsCompleted)
        {
            Debug.LogWarning($"[MissionManager] Obiettivo già completato: {objectiveId}");
            return;
        }

        objective.Advance(amount);
        Debug.Log($"[MissionManager]   ◉ {objective.GetDisplayText()}" +
                  (objective.IsCompleted ? " ← COMPLETATO" : ""));
    }

    /// <summary>
    /// Rende visibile un obiettivo nella checklist (sblocco manuale).
    /// Usato quando un obiettivo non ha unlockedAfter configurato
    /// e va sbloccato da un evento specifico (es: lettura messaggio smartphone).
    /// </summary>
    public void RevealObjective(string objectiveId)
    {
        var objective = FindObjective(objectiveId);
        if (objective == null)
        {
            Debug.LogWarning($"[MissionManager] Obiettivo non trovato per reveal: {objectiveId}");
            return;
        }

        if (objective.IsVisible)
        {
            return; // già visibile, niente da fare
        }

        objective.Reveal();
        Debug.Log($"[MissionManager]   → Sbloccato: {objective.GetDisplayText()}");
        OnObjectiveRevealed?.Invoke(objective);
    }

    /// <summary>
    /// Controlla se un obiettivo è completato.
    /// </summary>
    public bool IsObjectiveCompleted(string objectiveId)
    {
        var objective = FindObjective(objectiveId);
        return objective != null && objective.IsCompleted;
    }

    /// <summary>
    /// Controlla se un obiettivo è visibile.
    /// </summary>
    public bool IsObjectiveVisible(string objectiveId)
    {
        var objective = FindObjective(objectiveId);
        return objective != null && objective.IsVisible;
    }

    #endregion

    // ─────────────────────────────────────────────────────────
    #region Logica Interna

    private MissionObjective FindObjective(string objectiveId)
    {
        return currentObjectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
    }

    /// <summary>
    /// Callback invocato quando un obiettivo cambia stato.
    /// Gestisce la catena: aggiornamento → sblocco obiettivi dipendenti → check completamento missione.
    /// </summary>
    private void HandleObjectiveStateChanged(MissionObjective objective)
    {
        // 1. Notifica aggiornamento
        OnObjectiveUpdated?.Invoke(objective);

        // 2. Se appena completato, notifica e controlla sblocchi
        if (objective.IsCompleted)
        {
            OnObjectiveCompleted?.Invoke(objective);
            CheckUnlocks(objective.ObjectiveId);
            CheckMissionCompletion();
        }
    }

    /// <summary>
    /// Verifica se il completamento di un obiettivo sblocca altri obiettivi.
    /// Un obiettivo si sblocca quando TUTTI gli objectiveId in unlockedAfter sono completati.
    /// </summary>
    private void CheckUnlocks(string completedObjectiveId)
    {
        foreach (var objective in currentObjectives)
        {
            // Salta se già visibile o se non ha dipendenze
            if (objective.IsVisible || objective.UnlockedAfter.Length == 0)
                continue;

            // Controlla se questo obiettivo dipende da quello appena completato
            bool dependsOnCompleted = objective.UnlockedAfter.Contains(completedObjectiveId);
            if (!dependsOnCompleted) continue;

            // Controlla se TUTTE le dipendenze sono soddisfatte
            bool allDependenciesMet = objective.UnlockedAfter.All(
                depId => currentObjectives.Any(o => o.ObjectiveId == depId && o.IsCompleted)
            );

            if (allDependenciesMet)
            {
                objective.Reveal();
                Debug.Log($"[MissionManager]   → Auto-sbloccato: {objective.GetDisplayText()}");
                OnObjectiveRevealed?.Invoke(objective);
            }
        }
    }

    /// <summary>
    /// Verifica se tutti gli obiettivi della missione corrente sono completati.
    /// </summary>
    private void CheckMissionCompletion()
    {
        if (!missionActive) return;

        bool allCompleted = currentObjectives.All(o => o.IsCompleted);
        if (allCompleted)
        {
            missionActive = false;
            Debug.Log($"[MissionManager] ══════ Missione {currentMissionIndex + 1} COMPLETATA! ══════");
            OnMissionCompleted?.Invoke(currentMissionIndex, missions[currentMissionIndex]);
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────
    #region Debug

    /// <summary>
    /// Stampa lo stato corrente di tutti gli obiettivi (per debug).
    /// </summary>
    [ContextMenu("Debug: Stampa Stato Obiettivi")]
    public void DebugPrintObjectives()
    {
        if (currentObjectives.Count == 0)
        {
            Debug.Log("[MissionManager] Nessun obiettivo attivo.");
            return;
        }

        var missionData = CurrentMissionData;
        string header = missionData != null ? missionData.missionTitle : $"Missione {currentMissionIndex + 1}";

        string log = $"[MissionManager] ── {header} ──\n";
        foreach (var obj in currentObjectives)
        {
            string status = obj.IsCompleted ? "☑" : (obj.IsVisible ? "☐" : "🔒");
            log += $"  {status} {obj.GetDisplayText()} [{obj.ObjectiveId}]\n";
        }
        Debug.Log(log);
    }

    #endregion

    public void DebugCompleteCurrentMission()
    {
        if (!missionActive || currentObjectives == null || currentObjectives.Count == 0)
        {
            Debug.LogWarning("[MissionManager] DebugCompleteCurrentMission: nessuna missione attiva.");
            return;
        }

        Debug.Log($"[MissionManager] DEBUG: completo tutti gli obiettivi della missione {currentMissionIndex + 1}");

        // 1) rendi visibili tutti (così CompleteObjective non fallisce per IsVisible)
        foreach (var o in currentObjectives)
        {
            if (o == null) continue;
            if (!o.IsVisible) o.Reveal(); // scatena OnStateChanged -> OnObjectiveUpdated
            OnObjectiveRevealed?.Invoke(o); // assicura UI aggiornata anche se qualcuno non ascolta Update
        }

        // 2) completa tutto (anche quelli con counter)
        foreach (var o in currentObjectives)
        {
            if (o == null) continue;
            if (!o.IsCompleted) o.Complete(); // scatena OnStateChanged -> Completed + unlock + mission complete
        }

        StartNextMission();

        // Nota: CheckMissionCompletion viene chiamato dentro HandleObjectiveStateChanged
    }
}
