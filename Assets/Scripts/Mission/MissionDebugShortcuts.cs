using UnityEngine;

public class MissionDebugShortcuts : MonoBehaviour
{
    [Header("Hotkeys")]
    [SerializeField] private KeyCode completeCurrentMissionKey = KeyCode.F4;

    private void Update()
    {
        if (Input.GetKeyDown(completeCurrentMissionKey))
        {
            var mm = MissionManager.Instance;
            if (mm == null) { Debug.LogWarning("[MissionDebug] MissionManager.Instance null"); return; }

            mm.DebugCompleteCurrentMission(); // metodo che aggiungiamo sotto
        }
    }
}