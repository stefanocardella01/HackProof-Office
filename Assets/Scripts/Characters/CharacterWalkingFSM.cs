using UnityEngine;

public class NpcBrain : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _minWait = 3f;
    [SerializeField] private float _maxWait = 6f;

    private NpcMovement _movement;
    private Animator _animator;
    private FiniteStateMachine<NpcBrain> _fsm;

    void Start()
    {
        _movement = GetComponent<NpcMovement>();
        _animator = GetComponentInChildren<Animator>();

        _fsm = new FiniteStateMachine<NpcBrain>(this);

        var moveState = new MoveToState("MoveTo", _movement);
        var interactState = new InteractionState("Interaction", _movement, _minWait, _maxWait);

        _fsm.AddTransition(moveState, interactState, () => _movement != null && _movement.HasReachedDestination);
        _fsm.AddTransition(interactState, moveState, () => interactState.IsFinished());

        _fsm.SetState(moveState);
    }

    void Update()
    {
        // Se l'oggetto è nullo o la FSM è stata pulita, interrompi tutto
        if (this == null || _fsm == null) return;
        _fsm.Tik();
    }

    private void OnDestroy()
    {
        // Pulizia della FSM per evitare MissingReferenceException
        _fsm = null;
    }

    public void StartSocialCheck()
    {
        if (_movement == null || _movement.CurrentTarget == null) return;

        GameObject target = _movement.CurrentTarget;
        SeatedCharacter seated = target.GetComponent<SeatedCharacter>();

        if (seated != null)
        {
            
            Vector3 lookPos = seated.transform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            
            float dot = Vector3.Dot(seated.transform.right, (transform.position - seated.transform.position).normalized);
            seated.SetTalking(true, dot < 0);

            
            if (_animator != null) _animator.SetBool("Talking", true);
        }
    }

    public void EndSocialCheck()
    {
        if (_animator != null) _animator.SetBool("Talking", false);

        if (_movement != null && _movement.CurrentTarget != null)
        {
            SeatedCharacter seated = _movement.CurrentTarget.GetComponent<SeatedCharacter>();
            if (seated != null) seated.SetTalking(false);
        }
    }
}



public class IdleState : State
{
    private NpcMovement _movement;

    public IdleState(string name, NpcMovement movement)
        : base(name)
    {
        _movement = movement;
    }

    public override void Enter()
    {
        _movement.StopMovement();
    }

    public override void Tik() { }

    public override void Exit() { }
}




public class MoveToState : State
{
    private NpcMovement _movement;

    public MoveToState(string name, NpcMovement movement) : base(name)
    {
        _movement = movement;
    }

    public override void Enter()
    {
        _movement.MoveToNextTarget();
    }

    public override void Tik() { }

    public override void Exit() { }
}



public class InteractionState : State
{
    private NpcMovement _movement;
    private NpcBrain _brain;
    private float _duration, _timer, _min, _max;

    public InteractionState(string name, NpcMovement movement, float min, float max) : base(name)
    {
        _movement = movement;
        _brain = movement.GetComponent<NpcBrain>();
        _min = min; _max = max;
    }

    public override void Enter()
    {
        if (_brain == null || _movement == null) return;
        _movement.StopMovement();
        _timer = 0f;
        _duration = Random.Range(_min, _max);
        _brain.StartSocialCheck();
    }

    public override void Tik() => _timer += Time.deltaTime;
    public bool IsFinished() => _timer >= _duration;

    public override void Exit()
    {
        if (_brain != null) _brain.EndSocialCheck();
        if (_movement != null) _movement.GoToNextWaypoint();
    }
}