using UnityEngine;
using UnityEngine.Audio;

public class ManagerAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer _mainMixer;

    private AudioMixerSnapshot _soundtrack;
    private AudioMixerSnapshot _environment;
    private AudioMixerSnapshot _dialog;


    void Start()
    {
        _soundtrack = _mainMixer.FindSnapshot("Soundtrack");
        _environment = _mainMixer.FindSnapshot("Enviroment");
        _dialog = _mainMixer.FindSnapshot("Dialog");
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
