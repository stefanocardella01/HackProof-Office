using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalReportSectionUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private Transform boxesParent;
    [SerializeField] private Button detailsButton;

    public void SetTitle(string title)
    {
        if (missionTitleText != null)
            missionTitleText.text = title;
    }

    public void SetupDetailsButton(Action onClick)
    {
        if (detailsButton == null) return;

        detailsButton.onClick.RemoveAllListeners();
        if (onClick != null)
            detailsButton.onClick.AddListener(() => onClick());
    }

    /// <summary>
    /// Instanzia un SummaryBox dentro boxesParent e lo configura.
    /// </summary>
    public void AddSummaryBox(GameObject summaryBoxPrefab, string headline, string lines, SummaryStatus status)
    {
        if (summaryBoxPrefab == null)
        {
            Debug.LogError("[FinalReportSectionUI] summaryBoxPrefab NULL");
            return;
        }

        if (boxesParent == null)
        {
            Debug.LogError("[FinalReportSectionUI] boxesParent NULL");
            return;
        }

        var boxGO = Instantiate(summaryBoxPrefab, boxesParent);
        var ui = boxGO.GetComponent<SummaryBoxUI>();
        if (ui == null)
        {
            Debug.LogError("[FinalReportSectionUI] Il prefab SummaryBox non ha SummaryBoxUI attaccato.");
            return;
        }

        ui.Setup(headline, lines, status);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Non è obbligatorio, ma aiuta a capire subito se manca qualcosa
        if (detailsButton == null)
            detailsButton = GetComponentInChildren<Button>(true);
    }
#endif
}
