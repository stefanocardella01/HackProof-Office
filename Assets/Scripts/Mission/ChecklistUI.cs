using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


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

    private IEnumerator WaitForMissionManager()
    {
        while (MissionManager.Instance == null)
            yield return null; // aspetta un frame

        missionManager = MissionManager.Instance;

        missionManager.OnMissionStarted += HandleMissionStarted;
        missionManager.OnObjectiveRevealed += HandleObjectiveRevealed;
        missionManager.OnObjectiveUpdated += HandleObjectiveUpdated;
        missionManager.OnObjectiveCompleted += HandleObjectiveCompleted;
        missionManager.OnMissionCompleted += HandleMissionCompleted;
        missionManager.OnAllMissionsCompleted += HandleAllMissionsCompleted;

        SyncFromCurrentState();
    }

    private MissionManager missionManager;

    private void OnEnable()
    {
        StartCoroutine(WaitForMissionManager());
    }



    private void OnDisable()
    {
        if (missionManager == null) return;

        missionManager.OnMissionStarted -= HandleMissionStarted;
        missionManager.OnObjectiveRevealed -= HandleObjectiveRevealed;
        missionManager.OnObjectiveUpdated -= HandleObjectiveUpdated;
        missionManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
        missionManager.OnMissionCompleted -= HandleMissionCompleted;
        missionManager.OnAllMissionsCompleted -= HandleAllMissionsCompleted;
    }

    private void SyncFromCurrentState()
    {
        if (missionManager == null) return;

        // se nessuna missione attiva -> nascondi e pulisci
        if (!missionManager.IsMissionActive || missionManager.CurrentMissionData == null)
        {
            ClearAllItems();
            if (checklistPanel != null) checklistPanel.SetActive(false);
            return;
        }

        // mostra pannello e titolo
        if (checklistPanel != null) checklistPanel.SetActive(true);
        if (missionTitleText != null)
            missionTitleText.text = missionManager.CurrentMissionData.missionTitle;

        // ricostruisci gli obiettivi visibili già creati a runtime
        ClearAllItems();

        foreach (var obj in missionManager.CurrentObjectives)
        {
            if (obj != null && obj.IsVisible)
                SpawnItemInstant(obj);
        }
    }

    private void SpawnItemInstant(MissionObjective objective)
    {
        if (checklistItemPrefab == null || objectivesContainer == null)
        {
            Debug.LogError("[ChecklistUI] Prefab o container non assegnato!");
            return;
        }

        if (spawnedItems.ContainsKey(objective.ObjectiveId))
            return;

        GameObject itemGO = Instantiate(checklistItemPrefab, objectivesContainer);
        var itemUI = itemGO.GetComponent<ChecklistItemUI>();

        if (itemUI == null)
        {
            Debug.LogError("[ChecklistUI] Il prefab non ha il componente ChecklistItemUI!");
            Destroy(itemGO);
            return;
        }

        itemUI.Setup(objective);
        spawnedItems[objective.ObjectiveId] = itemUI;

        // (opzionale) se vuoi essere sicuro che sia subito visibile
        var cg = itemGO.GetComponent<CanvasGroup>() ?? itemGO.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
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
