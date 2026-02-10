using System;
using UnityEngine;

public class SeatedCharacter : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float idleTime = 6f;
    [SerializeField] private float writingTime = 8f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    // Eventi (opzionali, se ti servono per altri script)
    public event Action OnIdleStarted;
    public event Action OnWritingStarted;

    private float _timer;
    private bool _isWriting = false;

    private bool _isTalking = false;


    // Nomi dei parametri dell'Animator per evitare errori di battitura
    private static readonly int IsWritingHash = Animator.StringToHash("isWriting");
    private static readonly int TalkingHash = Animator.StringToHash("Talking");
    private static readonly int LeftHash = Animator.StringToHash("Left");

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        SetIdle();
    }

    private void Update()
    {
        if (_isTalking) return; // mentre parla non alterna idle/writing

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            if (_isWriting)
                SetIdle();
            else
                SetWriting();
        }
    }


    private void SetIdle()
    {
        _isWriting = false;
        _timer = idleTime;

        // Aggiorna l'animator
        animator.SetBool(IsWritingHash, false);

        OnIdleStarted?.Invoke();
    }

    private void SetWriting()
    {
        _isWriting = true;
        _timer = writingTime;

        // Aggiorna l'animator
        animator.SetBool(IsWritingHash, true);

        OnWritingStarted?.Invoke();
    }

    // Metodo pubblico per gestire il dialogo via script
    public void SetTalking(bool isTalking, bool lookLeft = true)
    {
        _isTalking = isTalking;

        animator.SetBool(TalkingHash, isTalking);
        animator.SetBool(LeftHash, lookLeft);

        if (isTalking)
        {
            // mentre parla: forzo non-scrittura + stop audio tramite evento Idle
            if (_isWriting)
                SetIdle();
            else
                OnIdleStarted?.Invoke(); // sicurezza: ferma audio anche se era rimasto attivo

            // opzionale: reset timer così quando finisce torna a idle per un po'
            _timer = idleTime;
        }
    }

}

