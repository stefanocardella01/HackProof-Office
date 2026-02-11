using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("NPC")]
    public string npcName = "Marco";
    public DialogueConversation conversation;

    [Header("Gating")]
    [SerializeField] private bool isEnabled = true;

    [Tooltip("Se valorizzato, completa questo obiettivo quando il dialogo finisce.")]
    [SerializeField] private string completeObjectiveIdOnDialogueEnd = "";

    [Tooltip("Se true, dopo il dialogo l'NPC non sarà più interagibile.")]
    [SerializeField] private bool disableAfterDialogue = false;

    [Tooltip("Se true, quando disabilitato spegne i collider (niente raycast).")]
    [SerializeField] private bool disableCollidersWhenDisabled = true;


    private SeatedCharacter seated;
    private Transform npcTransform;
    private Animator npcAnimator;

    private NpcPatrolBrain patrolBrain;
    private NpcMovement movement;

    private DialogueUI dialogueUI; // cache

    private void Awake()
    {
        seated = GetComponent<SeatedCharacter>();
        npcTransform = transform;
        npcAnimator = GetComponent<Animator>();

        patrolBrain = GetComponent<NpcPatrolBrain>();
        movement = GetComponent<NpcMovement>();

        // non è un problema se è null qui: lo ritroviamo al bisogno
        dialogueUI = FindFirstObjectByType<DialogueUI>();
    }

    public string GetInteractionText()
    {
        if (!isEnabled) return "";
        if (conversation == null) return "";
        return $"Parla con {npcName}";
    }

    public void SetConversation(DialogueConversation newConversation, string completeObjectiveOnEnd = "", bool disableAfter = false)
    {
        conversation = newConversation;
        completeObjectiveIdOnDialogueEnd = completeObjectiveOnEnd;
        disableAfterDialogue = disableAfter;
        SetEnabled(newConversation != null); // se vuoi disabilitare quando non ha conversazione
    }


    public void Interact(PlayerInteractor interactor)
    {
        if (!isEnabled) return;
        if (conversation == null) return;

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI == null) return;

        bool lookLeft = IsPlayerOnLeft(interactor.transform);

        // Attiva talking + direzione
        if (seated != null)
            seated.SetTalking(true, lookLeft);

        var audio = GetComponentInChildren<SeatedCharacterAudio>();
        if (audio != null) audio.ForceStop();

        // blocca movimento/brain
        if (movement != null) movement.StopMovement();
        if (patrolBrain != null) patrolBrain.StartTalking();

        dialogueUI.StartConversation(conversation, npcAnimator, onFinished: () =>
        {
            if (seated != null)
                seated.SetTalking(false, false);

            if (patrolBrain != null) patrolBrain.StopTalking();

            var mm = MissionManager.Instance;

            if (!string.IsNullOrWhiteSpace(completeObjectiveIdOnDialogueEnd) &&
                !mm.IsObjectiveCompleted(completeObjectiveIdOnDialogueEnd))
            {
                mm.CompleteObjective(completeObjectiveIdOnDialogueEnd);
            }

            // Disabilita interazione dopo dialogo (opzionale)
            if (disableAfterDialogue)
                SetEnabled(false);
        });
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;

        if (!disableCollidersWhenDisabled) return;

        foreach (var col in GetComponentsInChildren<Collider>(true))
            col.enabled = enabled;
    }

    private bool IsPlayerOnLeft(Transform player)
    {
        Vector3 local = npcTransform.InverseTransformPoint(player.position);
        return local.x < 0f;
    }
}
