using UnityEngine;

public class DialogueChoiceEventListener : MonoBehaviour
{
    [SerializeField] private DialogueChoiceEventChannelSO channel;

    private void OnEnable()
    {
        channel.OnEventRaised += Handle;
    }

    private void OnDisable()
    {
        channel.OnEventRaised -= Handle;
    }

    private void Handle(string eventId)
    {
        Debug.Log("Evento dialogo: " + eventId);

        if (eventId == "OpenEmailMinigame")
        {
            // Qui fai partire il tuo minigioco
        }
    }
}
