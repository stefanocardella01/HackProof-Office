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

    //Valore booleano che indica se questo nodo è gia stato visitato (ovvero se linee di dialogo sono già mostrate)
    //Utile perchè se il nostro NPC dice le frasi e noi scegliamo una scelta, se dopo quella scelta vogliamo tornare in questo nodo non vogliamo che vengano
    //mostrate nuovamente le frasi, ma solo le scelte (eventualmente solo quelle ancora non selezionate)
    private bool alreadyVisited = false;

#if UNITY_EDITOR
    // opzionale: ti avvisa se non sono allineati
    public void Validate()
    {
        if (lines == null) lines = new string[0];
        if (lineVoiceOvers == null) lineVoiceOvers = new AudioClip[0];
        // non forzo resize automatico (a volte vuoi meno clip), ma potresti farlo
    }
#endif
}
