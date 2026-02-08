using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Dialogue Choice Event Channel")]
public class DialogueChoiceEventChannelSO : ScriptableObject
{
    public event Action<string> OnEventRaised;

    public void Raise(string eventId)
    {
        OnEventRaised?.Invoke(eventId);
    }
}
