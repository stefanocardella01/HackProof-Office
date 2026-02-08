using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcMovement : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> _targets;
    [SerializeField] private float _arrivalThreshold = 0.2f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 8f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _currentTargetIndex = 0;

    public bool HasReachedDestination =>
        !_agent.pathPending &&
        _agent.remainingDistance <= _arrivalThreshold;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false; // rotazione manuale = più fluida
        _agent.updatePosition = true;

        _animator = GetComponentInChildren<Animator>();

        // sicurezza: disattiva root motion
        if (_animator != null)
            _animator.applyRootMotion = false;
    }

    private void Update()
    {
        UpdateAnimation();
        RotateTowardsMovement();
    }

    #region Movement API (usata dalla FSM)

    public void MoveToNextTarget()
    {
        if (_targets == null || _targets.Count == 0) return;

        _agent.isStopped = false;
        _agent.SetDestination(_targets[_currentTargetIndex].position);
    }

    public void GoToNextWaypoint()
    {
        if (_targets == null || _targets.Count == 0) return;

        _currentTargetIndex = (_currentTargetIndex + 1) % _targets.Count;
        MoveToNextTarget();
    }

    public void StopMovement()
    {
        if (_agent == null) return;

        _agent.isStopped = true;
        _agent.velocity = Vector3.zero; // Azzera l'inerzia immediata
        _agent.ResetPath();             // Pulisce il calcolo del percorso

        // Forza l'animator a fermarsi se lo script viene chiamato in Idle
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            _animator.SetBool("Walking", false);
            _animator.SetFloat("Speed", 0f);
        }
    }

    #endregion

    #region Animation

    private void UpdateAnimation()
    {
        if (_animator == null) return;

        // Calcoliamo la velocità relativa (0 se fermo, 1 se corre alla velocità massima)
        float currentSpeed = _agent.velocity.magnitude;
        float normalizedSpeed = currentSpeed / _agent.speed;

        // Applichiamo una piccola soglia per evitare micro-movimenti
        if (currentSpeed < 0.1f) normalizedSpeed = 0f;

        _animator.SetFloat("Speed", normalizedSpeed);
        _animator.SetBool("Walking", normalizedSpeed > 0.01f);
    }

    #endregion

    #region Rotation (smooth e naturale)

    private void RotateTowardsMovement()
    {
        // Aumenta la soglia per evitare micro-rotazioni nervose
        if (_agent.velocity.sqrMagnitude < 0.1f)
            return;

        Vector3 direction = _agent.velocity.normalized;
        direction.y = 0; // Mantieni l'NPC dritto, evita che si inclini verso l'alto/basso

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * _rotationSpeed
        );
    }

    #endregion

    #region Anti Root Motion Drift (fix definitivo)

    private void OnAnimatorMove()
    {
        if (_agent != null)
            transform.position = _agent.nextPosition;
    }

    #endregion
}

