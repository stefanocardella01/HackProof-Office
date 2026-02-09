using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HackProof/Mission Report Data")]
public class MissionReportData : ScriptableObject
{
    public string missionTitle;
    public List<ReportEntry> entries = new();
}
