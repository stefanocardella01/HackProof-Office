using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    //testo che viene mostrato 
    public string text;

    //-1 significa chiudi ui dialogo
    public int nextNodeIndex = -1;

    public bool endsDialogue = false;

    [Header("Uso singolo (opzionale)")]
    public bool singleUse = false;  // se true: dopo averla scelta non la mostro più
    public string customId;         // opzionale: ID leggibile, altrimenti ne generiamo uno
}
