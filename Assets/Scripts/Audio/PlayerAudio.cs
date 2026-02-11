using UnityEngine;
using StarterAssets;
public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip walkingClip;
    private AudioSource audioSource;
    private FirstPersonController controller;

    void Start()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        controller = GetComponent<FirstPersonController>();
        if (controller == null || audioSource == null)
            return;
        controller.OnWalking += HandleWalking;
    }

    private void HandleWalking(bool isWalking)
    {
        if (isWalking)
            Play(walkingClip);
        else
            StopSound();
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;


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
