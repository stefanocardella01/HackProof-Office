using System.Collections.Generic;
using UnityEngine;

public class MissionTracker : MonoBehaviour
{
    public static MissionTracker Instance { get; private set; }

    private Dictionary<ReportCheck, bool> results = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Set(ReportCheck check, bool value)
    {
        results[check] = value;
    }

    public bool Get(ReportCheck check)
    {
        return results.TryGetValue(check, out bool v) && v;
    }

    public void ResetAll()
    {
        results.Clear();
    }
}
