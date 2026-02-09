using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conversation")]
public class DialogueConversation : ScriptableObject
{

    [Header("Speaker")]
    public string speakerName;

    public int startNodeIndex = 0;

    public DialogueNode[] nodes;

}
