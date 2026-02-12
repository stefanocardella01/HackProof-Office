using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcMovement : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<GameObject> _targets;
    [SerializeField] private float _arrivalThreshold = 0.4f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _currentTargetIndex = 0;

    public event Action<bool> OnWalking;
    private bool _isWalking = false;
    private float speed;

    public GameObject CurrentTarget => (_targets != null && _currentTargetIndex < _targets.Count) ? _targets[_currentTargetIndex] : null;

    public bool HasReachedDestination
    {
        get
        {
            if (_agent == null || !_agent.isOnNavMesh) return false;
            return !_agent.pathPending && _agent.remainingDistance <= _arrivalThreshold;
        }
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (_agent == null || !_agent.isActiveAndEnabled) return;
        speed = _agent.velocity.magnitude / _agent.speed;
        UpdateAnimation();
        UpdateAudio();
        RotateTowardsMovement();
    }

    public void MoveToNextTarget()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        var target = CurrentTarget;
        if (target == null) return;

        _agent.isStopped = false;
        _agent.SetDestination(target.transform.position);
    }

    public void GoToNextWaypoint()
    {
        if (_targets == null || _targets.Count == 0) return;

        int nextIndex = GetNextAllowedIndex(_currentTargetIndex);
        _currentTargetIndex = nextIndex;

        MoveToNextTarget();
    }

    private int GetNextAllowedIndex(int fromIndex)
    {
        int count = _targets.Count;
        int start = (fromIndex + 1) % count;

        // sicurezza: massimo N tentativi, così non loopi infinito
        for (int i = 0; i < count; i++)
        {
            int idx = (start + i) % count;
            if (IsAllowedTargetIndex(idx))
                return idx;
        }

        // Se sono tutti "bloccati", resta dove sei (fallback)
        return fromIndex;
    }

    private bool IsAllowedTargetIndex(int idx)
    {
        if (_targets == null || idx < 0 || idx >= _targets.Count) return false;

        GameObject t = _targets[idx];
        if (t == null) return false;

        // Se non c'è MissionManager, non bloccare nulla
        var mm = MissionManager.Instance;
        if (mm == null || !mm.IsMissionActive) return true;

        // Se il waypoint non ha gate, è sempre ok
        var gate = t.GetComponent<WaypointMissionGate>();
        if (gate == null) return true;

        // Regola richiesta:
        // "Se il prossimo target appartiene alla missione corrente, passa al successivo."
        return gate.missionIndexOwner != mm.CurrentMissionIndex;
    }


    public void StopMovement()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        if (_animator != null)
        {
            _animator.SetBool("Walking", false);
            _animator.SetFloat("Speed", 0f);
        }
    }

    private void UpdateAnimation()
    {
        if (_animator == null || _agent == null) return;
        
        _animator.SetFloat("Speed", speed > 0.1f ? speed : 0f);
        _animator.SetBool("Walking", speed > 0.1f);
    }

    private void UpdateAudio()
    {   
        if (speed > 0.1f)
        {
            if (!_isWalking)
            {
                _isWalking = true;
                OnWalking?.Invoke(true);
            }
        }
        else
        {
            if (_isWalking)
            {
                _isWalking = false;
                OnWalking?.Invoke(false);
            }
        }
    }

    private void RotateTowardsMovement()
    {
        if (_agent == null || _agent.velocity.sqrMagnitude < 0.1f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_agent.velocity.normalized), Time.deltaTime * 8f);
    }
}