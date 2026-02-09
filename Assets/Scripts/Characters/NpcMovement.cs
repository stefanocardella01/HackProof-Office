using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcMovement : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<GameObject> _targets;
    [SerializeField] private float _arrivalThreshold = 0.4f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _currentTargetIndex = 0;

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
        UpdateAnimation();
        RotateTowardsMovement();
    }

    public void MoveToNextTarget()
    {
        if (_agent == null || !_agent.isOnNavMesh || CurrentTarget == null) return;
        _agent.isStopped = false;
        _agent.SetDestination(CurrentTarget.transform.position);
    }

    public void GoToNextWaypoint()
    {
        if (_targets == null || _targets.Count == 0) return;
        _currentTargetIndex = (_currentTargetIndex + 1) % _targets.Count;
        MoveToNextTarget();
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
        float speed = _agent.velocity.magnitude / _agent.speed;
        _animator.SetFloat("Speed", speed > 0.1f ? speed : 0f);
        _animator.SetBool("Walking", speed > 0.1f);
    }

    private void RotateTowardsMovement()
    {
        if (_agent == null || _agent.velocity.sqrMagnitude < 0.1f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_agent.velocity.normalized), Time.deltaTime * 8f);
    }
}