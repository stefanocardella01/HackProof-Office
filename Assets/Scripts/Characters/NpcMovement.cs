using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcMovement : MonoBehaviour
{
    [SerializeField] private List<Transform> _targets;  // Lista dei waypoint
    [SerializeField] private float _arrivalThreshold = 0.1f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _currentTargetIndex = 0;

    public bool HasReachedDestination =>
        !_agent.pathPending &&
        _agent.remainingDistance <= _arrivalThreshold;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false; // blocca rotazioni automatiche
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (_targets == null || _targets.Count == 0)
        {
            Debug.LogWarning("NpcMovement: nessun target assegnato a " + gameObject.name);
            enabled = false;
            return;
        }

        MoveToNextTarget();
    }

    private void Update()
    {
        if (_targets.Count == 0) return;

        if (!HasReachedDestination)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_targets[_currentTargetIndex].position);
        }

        UpdateAnimation();
    }

    public void MoveToNextTarget()
    {
        if (_targets.Count == 0) return;

        _agent.isStopped = false;
        _agent.SetDestination(_targets[_currentTargetIndex].position);
    }

    public void GoToNextWaypoint()
    {
        _currentTargetIndex = (_currentTargetIndex + 1) % _targets.Count;
        MoveToNextTarget();
    }

    public void StopMovement()
    {
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
        _agent.ResetPath();
    }

    // Alias per compatibilità con vecchi stati
    public void Stop()
    {
        StopMovement();
    }

    public void PlayInteractionAnimation(string animationName)
    {
        if (_animator != null)
            _animator.Play(animationName); // all’inizio può essere ""
    }

    private void UpdateAnimation()
    {
        if (_animator == null) return;

        float speed = !_agent.isStopped ? _agent.velocity.magnitude : 0f;
        _animator.SetFloat("Speed", speed);
    }
}

