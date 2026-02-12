using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class ManagerAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer _mainMixer;

    private AudioMixerSnapshot _soundtrack;
    private AudioMixerSnapshot _environment;
    private AudioMixerSnapshot _dialog;


    void Start()
    {
        _soundtrack = _mainMixer.FindSnapshot("Normal");
        _dialog = _mainMixer.FindSnapshot("Dialog");
}

    public void SetDialog()
    {   
        Debug.Log("Dialog");
        _dialog.TransitionTo(4f);
    }

    public void SetNormal()
    {       
        Debug.Log("Normal");
        _soundtrack.TransitionTo(4f);
    }
}
