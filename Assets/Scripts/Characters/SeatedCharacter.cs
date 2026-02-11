using System;
using UnityEngine;

public class SeatedCharacter : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float idleTime = 6f;
    [SerializeField] private float writingTime = 8f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    public event Action OnIdleStarted;
    public event Action OnWritingStarted;
    public event Action<bool> OnTalking; 

    private float _timer;
    private bool _isWriting = false;
    private bool _isTalking = false;

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
        if (_isTalking) return;

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

        animator.SetBool(IsWritingHash, false);

        OnIdleStarted?.Invoke();
    }

    private void SetWriting()
    {
        _isWriting = true;
        _timer = writingTime;

        animator.SetBool(IsWritingHash, true);

        OnWritingStarted?.Invoke();
    }

    public void SetTalking(bool isTalking, bool lookLeft = true)
    {
        if (_isTalking == isTalking) return; 

        _isTalking = isTalking;

        animator.SetBool(TalkingHash, isTalking);
        animator.SetBool(LeftHash, lookLeft);

        OnTalking?.Invoke(isTalking); 

        if (isTalking)
        {
            if (_isWriting)
                SetIdle();

            _timer = idleTime;
        }
    }

    
}


