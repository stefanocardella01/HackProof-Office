using UnityEngine;

public class SeatedCharacterAudio : MonoBehaviour
{
    [SerializeField] private AudioClip writingClip;
    private AudioSource audioSource;

    private SeatedCharacter seatedCharacter;

    private void Start()
    {
        seatedCharacter = GetComponentInParent<SeatedCharacter>();
        audioSource = GetComponentInChildren<AudioSource>();

        if (seatedCharacter == null || audioSource == null)
            return;

        seatedCharacter.OnWritingStarted += PlayWritingSound;
        seatedCharacter.OnIdleStarted += StopWritingSound;
    }

    private void PlayWritingSound()
    {
        if (writingClip == null)
            return;

        audioSource.clip = writingClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopWritingSound()
    {
        audioSource.Stop();
    }

    private void OnDestroy()
    {
        if (seatedCharacter == null)
            return;

        seatedCharacter.OnWritingStarted -= PlayWritingSound;
        seatedCharacter.OnIdleStarted -= StopWritingSound;
    }
}

