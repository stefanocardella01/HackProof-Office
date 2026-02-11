using UnityEngine;

public class NpcWalkingAudio : MonoBehaviour
{
    [SerializeField] private AudioClip walkingClip;
    private AudioSource audioSource;
    private NpcMovement controller;

    void Start()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        controller = GetComponent<NpcMovement>();
        if (controller == null || audioSource == null)
            return;
        controller.OnWalking += HandleWalking;
    }

    private void HandleWalking(bool isWalking)
    {       
        Debug.Log($"NPC walking state changed: isWalking={isWalking}");
        if (isWalking)
            Play(walkingClip);
        else
            StopSound();
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        Debug.Log($"Playing clip: {clip.name}");

        if (audioSource.isPlaying && audioSource.clip == clip)
            return;
        

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopSound()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}