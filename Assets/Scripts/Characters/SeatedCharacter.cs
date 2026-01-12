using System;
using UnityEngine;

public class SeatedCharacter : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float idleTime = 6f;
    [SerializeField] private float writingTime = 6f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    public event Action OnIdleStarted;
    public event Action OnWritingStarted;

    private float _timer;
    private bool _isWriting = false;

    private void Start()
    {
        SetIdle();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
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

        animator.SetBool("isWriting", false);

        OnIdleStarted?.Invoke();
    }

    private void SetWriting()
    {
        _isWriting = true;
        _timer = writingTime;

        animator.SetBool("isWriting", true);

        OnWritingStarted?.Invoke();
    }

    public bool IsWriting => _isWriting;
}

