using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    public string npcName = "Marco";
    public DialogueConversation conversation;

    private SeatedCharacter seated;
    private Transform npcTransform;
    private Animator npcAnimator;

    private void Awake()
    {
        seated = GetComponent<SeatedCharacter>();
        npcTransform = transform;
        npcAnimator = GetComponent<Animator>();
    }

    public string GetInteractionText()
    {
        return $"Parla con {npcName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        var dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI == null) return;

        bool lookLeft = IsPlayerOnLeft(interactor.transform);

        // Attiva talking + direzione (gestito da SeatedCharacter)
        if (seated != null)
            seated.SetTalking(true, lookLeft);

        var audio = GetComponentInChildren<SeatedCharacterAudio>();
        if (audio != null) audio.ForceStop();


        // Avvia dialogo e quando finisce resetta (e torna al ciclo idle/writing)
        dialogueUI.StartConversation(conversation, npcAnimator, onFinished: () =>
        {
            if (seated != null)
                seated.SetTalking(false, false);
        });
    }

    private bool IsPlayerOnLeft(Transform player)
    {
        Vector3 local = npcTransform.InverseTransformPoint(player.position);
        return local.x < 0f;
    }
}
