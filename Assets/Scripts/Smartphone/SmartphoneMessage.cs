using UnityEngine;
// rappresenta il singolo messaggio ricevuto dallo smartphone in-game
[System.Serializable]
public class SmartphoneMessage
{
    public string id;              // ID univoco del messaggio (es: "msg_mission2_collega1")
    public string senderName;      // Nome del mittente (es: "Marco Rossi")
    public Sprite senderIcon;      // Icona/avatar del mittente (opzionale)

    [TextArea(2, 5)]
    public string messageText;     // Contenuto del messaggio

    public bool isRead;            // Se il messaggio è stato letto
    public string timestamp;       // Orario del messaggio (es: "09:32")

    // Riferimento opzionale a un GameObject da evidenziare dopo la lettura
    // (es: l'NPC collega da andare a trovare)
    public GameObject targetToHighlight;

    public SmartphoneMessage()
    {
        id = System.Guid.NewGuid().ToString();
        isRead = false;
        timestamp = System.DateTime.Now.ToString("HH:mm");
    }

    // Costruttore con parametri per mittente e testo(prima chiama il construttre senza parametri this())
    public SmartphoneMessage(string sender, string text) : this()
    {
        senderName = sender;
        messageText = text;
    }
}

