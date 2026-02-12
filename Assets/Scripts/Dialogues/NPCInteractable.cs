using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

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

    [Header("Optional Facing")]
    [SerializeField] private bool rotateTowardsPlayerBeforeTalking = false;
    [SerializeField] private float faceDuration = 0.15f;
    [SerializeField] private float faceTurnSpeed = 12f;



    private SeatedCharacter seated;
    private Transform npcTransform;
    private Animator npcAnimator;

    private NpcPatrolBrain patrolBrain;
    private NpcMovement movement;

    private DialogueUI dialogueUI; // cache

    [SerializeField] private ManagerAudio mixer;

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

        if (rotateTowardsPlayerBeforeTalking)
            StartCoroutine(InteractFaceThenTalk(interactor));
        else
            StartConversationNow(interactor);
    }

    private void StartConversationNow(PlayerInteractor interactor)
    {
        bool lookLeft = IsPlayerOnLeft(interactor.transform);

        // Audio
        if (mixer != null) mixer.SetDialog();

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

            if (patrolBrain != null)
                patrolBrain.StopTalking();

            var mm = MissionManager.Instance;

            if (!string.IsNullOrWhiteSpace(completeObjectiveIdOnDialogueEnd) &&
                !mm.IsObjectiveCompleted(completeObjectiveIdOnDialogueEnd))
            {
                mm.CompleteObjective(completeObjectiveIdOnDialogueEnd);
            }

            // Disabilita interazione dopo dialogo (opzionale)
            if (disableAfterDialogue)
                SetEnabled(false);

            if (mixer != null) mixer.SetNormal();
        });
    }

    private IEnumerator InteractFaceThenTalk(PlayerInteractor interactor)
    {
        // fermalo subito così non “lotta” con la rotazione di movimento
        if (movement != null) movement.StopMovement();

        float t = 0f;
        while (t < faceDuration)
        {
            Vector3 dir = interactor.transform.position - npcTransform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                npcTransform.rotation = Quaternion.Slerp(
                    npcTransform.rotation,
                    targetRot,
                    Time.deltaTime * faceTurnSpeed
                );
            }

            t += Time.deltaTime;
            yield return null;
        }

        StartConversationNow(interactor);
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
