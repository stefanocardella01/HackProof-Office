using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    public string npcName = "Marco";
    public DialogueConversation conversation;

    public string GetInteractionText()
    {
        return $"Parla con {npcName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
        dialogueUI.StartConversation(conversation);
    }

}
