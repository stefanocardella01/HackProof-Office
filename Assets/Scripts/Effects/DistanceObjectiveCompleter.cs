using UnityEngine;

public class DistanceObjectiveCompleter : MonoBehaviour
{
    [Header("Mission")]
    [SerializeField] private string objectiveId = "m1_go_to_desk";

    [Header("Distanza")]
    [SerializeField] private float activationDistance = 2.2f;

    private Transform player;
    private bool done = false;

    private void Start()
    {
        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor != null)
            player = interactor.transform;
    }

    private void Update()
    {
        if (done || player == null) return;

        var mm = MissionManager.Instance;
        if (mm == null) return;

        if(objectiveId != "")
        {
            // Deve essere visibile e non completato
            if (!mm.IsObjectiveVisible(objectiveId) || mm.IsObjectiveCompleted(objectiveId))
                return;
        }

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= activationDistance)
        {
            mm.CompleteObjective(objectiveId);
            done = true;

            // Spegne l'effetto
            gameObject.SetActive(false);
        }
    }
}
