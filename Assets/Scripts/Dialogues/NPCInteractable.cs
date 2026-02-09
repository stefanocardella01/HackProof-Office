using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    public string npcName = "Marco";
    public DialogueConversation conversation;
    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>(); // Animator dell’NPC
    }


    public string GetInteractionText()
    {
        return $"Parla con {npcName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();

        // passiamo ANCHE l’animator dell’NPC
        dialogueUI.StartConversation(conversation, animator);
    }

}
