using UnityEngine;

public class DoorAudio : MonoBehaviour
{
    [SerializeField] private AudioClip _openingSound;
    [SerializeField] private AudioClip _closingSound;
    [SerializeField] private AudioClip _UnlockDoor;

    private Door _door;
    private DoorOpener _d;
    private AudioSource _audioSource;

    private void Start()
    {
        _door = GetComponentInChildren<Door>();
        _d = GetComponentInChildren<DoorOpener>();
        _audioSource = GetComponentInChildren<AudioSource>();

        if (_door == null || _audioSource == null)
            return;

        _door.DoorOpening += PlayOpeningSound;
        _door.DoorClosing += PlayClosingSound;
    }

    private void PlayOpeningSound()
    {
        bool hasBadge = _d.Badge();
        Debug.Log("Badge: " + hasBadge);

        Play(_openingSound);
        if (hasBadge)
            Play(_UnlockDoor);
    }


    private void PlayClosingSound()
    {
        Play(_closingSound);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        
        _audioSource.PlayOneShot(clip);
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
