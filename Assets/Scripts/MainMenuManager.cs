using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;

    public void PlayGame()
    {
        // Distruggi singleton persistenti
        if (MissionManager.Instance != null)
            Destroy(MissionManager.Instance.gameObject);

        if (MissionTracker.Instance != null)
            Destroy(MissionTracker.Instance.gameObject);

        SceneManager.LoadScene("HackProof-Office");
    }

    public void OpenInfo()
    {
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();

        // Solo per test in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
