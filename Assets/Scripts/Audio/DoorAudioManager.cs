using UnityEngine;

public class DoorAudio : MonoBehaviour
{
    [SerializeField] private AudioClip _openingSound;
    [SerializeField] private AudioClip _closingSound;

    private Door _door;
    private AudioSource _audioSource;

    private void Start()
    {
        _door = GetComponentInChildren<Door>();
        _audioSource = GetComponentInChildren<AudioSource>();

        if (_door == null || _audioSource == null)
            return;

        _door.DoorOpening += PlayOpeningSound;
        _door.DoorClosing += PlayClosingSound;
    }

    private void PlayOpeningSound()
    {
        Play(_openingSound);
    }

    private void PlayClosingSound()
    {
        Play(_closingSound);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    private void OnDestroy()
    {
        if (_door != null)
        {
            _door.DoorOpening -= PlayOpeningSound;
            _door.DoorClosing -= PlayClosingSound;
        }
    }
}
