using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UrlHoverArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string urlText;
    private GameObject tooltipPanel;
    private TextMeshProUGUI tooltipTextUI;

    // Configura i dati (chiamato da EmailScreen)
    public void Configure(string url, GameObject panel, TextMeshProUGUI textUI)
    {
        urlText = url;
        tooltipPanel = panel;
        tooltipTextUI = textUI;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        Debug.Log("ENTER " + gameObject.name);

        Debug.Log($"panel={(tooltipPanel ? tooltipPanel.name : "NULL")} url='{urlText}' text={(tooltipTextUI ? tooltipTextUI.name : "NULL")}");


        if (tooltipPanel != null && !string.IsNullOrEmpty(urlText))
        {
            tooltipPanel.SetActive(true);
            if (tooltipTextUI != null) tooltipTextUI.text = urlText;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}