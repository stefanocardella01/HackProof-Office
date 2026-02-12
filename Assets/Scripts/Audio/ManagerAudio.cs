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
    private AudioMixerSnapshot _dialog;


    private void Awake()
    {
        if (_mainMixer == null)
        {
            Debug.LogError("[ManagerAudio] MainMixer non assegnato!");
            return;
        }

        _soundtrack = _mainMixer.FindSnapshot("Normal");
        _dialog = _mainMixer.FindSnapshot("Dialog");

        if (_soundtrack == null) Debug.LogError("[ManagerAudio] Snapshot Normal non trovato!");
        if (_dialog == null) Debug.LogError("[ManagerAudio] Snapshot Dialog non trovato!");
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
