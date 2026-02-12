using UnityEngine;

public class WaypointMissionGate : MonoBehaviour
{
    [Tooltip("Indice della missione a cui appartiene questo waypoint (0-based come MissionManager).")]
    public int missionIndexOwner = -1;
}
