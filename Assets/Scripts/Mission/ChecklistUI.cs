using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// UI della checklist missione (HUD in alto a sinistra).
/// Si iscrive agli eventi del MissionManager e aggiorna la lista degli obiettivi.
/// 
/// SETUP IN UNITY:
///   Canvas (Screen Space - Overlay)
///     └── ChecklistPanel (ancorato in alto a sinistra)
///           ├── MissionTitleText (TMP)
///           └── ObjectivesContainer (VerticalLayoutGroup)
///                 └── (gli item vengono istanziati qui)
/// 
/// Assegnare:
///   - checklistPanel: il pannello contenitore
///   - missionTitleText: il testo del titolo missione
///   - objectivesContainer: il parent con VerticalLayoutGroup
///   - checklistItemPrefab: il prefab di ChecklistItemUI
/// </summary>
public class ChecklistUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private GameObject checklistPanel;
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private Transform objectivesContainer;

    [Header("Prefab")]
    [SerializeField] private GameObject checklistItemPrefab;

    [Header("Animazione")]
    [SerializeField] private float itemRevealDelay = 0.3f;
    [SerializeField] private float itemFadeDuration = 0.2f;

    // Mappa objectiveId → ChecklistItemUI istanziato
    private Dictionary<string, ChecklistItemUI> spawnedItems = new();

    private MissionManager missionManager;

    private void Start()
    {
        missionManager = MissionManager.Instance;

        if (missionManager == null)
        {
            Debug.LogError("[ChecklistUI] MissionManager.Instance non trovato!");
            return;
        }

        // Iscrizione agli eventi
        missionManager.OnMissionStarted += HandleMissionStarted;
        missionManager.OnObjectiveRevealed += HandleObjectiveRevealed;
        missionManager.OnObjectiveUpdated += HandleObjectiveUpdated;
        missionManager.OnObjectiveCompleted += HandleObjectiveCompleted;
        missionManager.OnMissionCompleted += HandleMissionCompleted;
        missionManager.OnAllMissionsCompleted += HandleAllMissionsCompleted;

        // Nascondi all'inizio se non c'è una missione attiva
        if (!missionManager.IsMissionActive && checklistPanel != null)
            checklistPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (missionManager == null) return;

        missionManager.OnMissionStarted -= HandleMissionStarted;
        missionManager.OnObjectiveRevealed -= HandleObjectiveRevealed;
        missionManager.OnObjectiveUpdated -= HandleObjectiveUpdated;
        missionManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
        missionManager.OnMissionCompleted -= HandleMissionCompleted;
        missionManager.OnAllMissionsCompleted -= HandleAllMissionsCompleted;
    }

    // ─────────────────────────────────────────────────────────
    #region Event Handlers

    private void HandleMissionStarted(int index, MissionData data)
    {
        // Pulisci gli item precedenti
        ClearAllItems();

        // Mostra il pannello
        if (checklistPanel != null)
            checklistPanel.SetActive(true);

        // Titolo missione
        if (missionTitleText != null)
            missionTitleText.text = data.missionTitle;

        Debug.Log($"[ChecklistUI] Checklist aggiornata per: {data.missionTitle}");
    }

    private void HandleObjectiveRevealed(MissionObjective objective)
    {
        if (spawnedItems.ContainsKey(objective.ObjectiveId))
        {
            // Già presente, aggiorna solo
            spawnedItems[objective.ObjectiveId].UpdateVisual();
            return;
        }

        // Crea un nuovo item nella checklist
        StartCoroutine(SpawnItemAnimated(objective));
    }

    private void HandleObjectiveUpdated(MissionObjective objective)
    {
        if (spawnedItems.TryGetValue(objective.ObjectiveId, out var item))
        {
            item.UpdateVisual();
        }
    }

    private void HandleObjectiveCompleted(MissionObjective objective)
    {
        if (spawnedItems.TryGetValue(objective.ObjectiveId, out var item))
        {
            item.UpdateVisual();
        }
    }

    private void HandleMissionCompleted(int index, MissionData data)
    {
        // Opzionale: nascondi la checklist o mostra un feedback "Missione Completata"
        Debug.Log($"[ChecklistUI] Missione {index + 1} completata!");

        // Nascondi dopo un breve delay (il report si aprirà)
        StartCoroutine(HideAfterDelay(1f));
    }

    private void HandleAllMissionsCompleted()
    {
        Debug.Log("[ChecklistUI] Tutte le missioni completate!");
        if (checklistPanel != null)
            checklistPanel.SetActive(false);
    }

    #endregion

    // ─────────────────────────────────────────────────────────
    #region UI Management

    /// <summary>
    /// Istanzia un item nella checklist con animazione di fade-in.
    /// </summary>
    private IEnumerator SpawnItemAnimated(MissionObjective objective)
    {
        if (checklistItemPrefab == null || objectivesContainer == null)
        {
            Debug.LogError("[ChecklistUI] Prefab o container non assegnato!");
            yield break;
        }

        yield return new WaitForSeconds(itemRevealDelay);

        GameObject itemGO = Instantiate(checklistItemPrefab, objectivesContainer);
        var itemUI = itemGO.GetComponent<ChecklistItemUI>();

        if (itemUI == null)
        {
            Debug.LogError("[ChecklistUI] Il prefab non ha il componente ChecklistItemUI!");
            Destroy(itemGO);
            yield break;
        }

        itemUI.Setup(objective);
        spawnedItems[objective.ObjectiveId] = itemUI;

        // Animazione fade-in
        var canvasGroup = itemGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = itemGO.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < itemFadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / itemFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Rimuove tutti gli item dalla checklist.
    /// </summary>
    private void ClearAllItems()
    {
        foreach (var kvp in spawnedItems)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        spawnedItems.Clear();

        // Pulisci anche eventuali figli rimasti
        if (objectivesContainer != null)
        {
            foreach (Transform child in objectivesContainer)
                Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Nasconde il pannello dopo un delay.
    /// </summary>
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (checklistPanel != null)
            checklistPanel.SetActive(false);
    }

    #endregion
}
