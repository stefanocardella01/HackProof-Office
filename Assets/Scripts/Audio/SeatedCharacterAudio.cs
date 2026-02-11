using UnityEngine;

public class SeatedCharacterAudio : MonoBehaviour
{
    [SerializeField] private AudioClip writingClip;
    [SerializeField] private AudioClip talkingClip;

    private AudioSource audioSource;
    private SeatedCharacter seatedCharacter;

    private void Start()
    {
        seatedCharacter = GetComponentInParent<SeatedCharacter>();
        audioSource = GetComponentInChildren<AudioSource>();

        if (seatedCharacter == null || audioSource == null)
            return;

        seatedCharacter.OnWritingStarted += PlayWritingSound;
        seatedCharacter.OnIdleStarted += StopSound;
        seatedCharacter.OnTalking += HandleTalkingSound; 
    }

    private void PlayWritingSound()
    {
        Play(writingClip);
    }

    private void HandleTalkingSound(bool isTalking)
    {
        if (isTalking)
            Play(talkingClip);
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

    private void OnDestroy()
    {
        if (seatedCharacter == null)
            return;

        seatedCharacter.OnWritingStarted -= PlayWritingSound;
        seatedCharacter.OnIdleStarted -= StopSound;
        seatedCharacter.OnTalking -= HandleTalkingSound;
    }

    public void ForceStop()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}


