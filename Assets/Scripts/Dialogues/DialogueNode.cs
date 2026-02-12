using UnityEngine;
[System.Serializable]
public class DialogueNode
{

    [Tooltip("Un array di stringhe così in un solo nodo può dire più frasi (che vengono mostrate una alla volta)")]
    [TextArea(2, 4)]
    public string[] lines;

    [Tooltip("Voice-over associati 1:1 alle lines. Se un elemento è null, nessun audio per quella linea.")]
    public AudioClip[] lineVoiceOvers;

    [Tooltip("Scelte disponibili alla fine delle frasi dell'NPC")]
    public DialogueChoice[] choices;

#if UNITY_EDITOR
    public void Validate()
    {
        if (lines == null) lines = new string[0];
        if (lineVoiceOvers == null) lineVoiceOvers = new AudioClip[0];
    }
#endif
}
