using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conversation")]
public class DialogueConversation : ScriptableObject
{

    public int startNodeIndex = 0;

    public DialogueNode[] nodes;

}
