using UnityEngine;

public class NpcPatrolBrain : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _inspectDuration = 4f;
    [SerializeField] private float _initialIdleWait = 2f;


    private NpcMovement _movement;
    private Animator _animator;
    private FiniteStateMachine<NpcPatrolBrain> _fsm;

    [Header("Talking Override")]
    [SerializeField] private string talkBoolParam = "Talking"; // deve esistere nell'Animator
    private bool _isTalking;

    public void StartTalking()
    {
        _isTalking = true;

        if (_movement != null)
            _movement.StopMovement();

        if (_animator != null)
        {
            _animator.SetBool("Walking", false);
            _animator.SetFloat("Speed", 0f);
            _animator.SetBool("Looking", false);

            // attiva talking
            _animator.SetBool(talkBoolParam, true);
        }
    }

    public void StopTalking()
    {
        _isTalking = false;

        if (_animator != null)
            _animator.SetBool(talkBoolParam, false);
    }


    void Start()
    {
        _movement = GetComponent<NpcMovement>();
        _animator = GetComponentInChildren<Animator>();

        _fsm = new FiniteStateMachine<NpcPatrolBrain>(this);

        // Definizione degli stati
        var idleState = new IdleState("Idle", _movement);
        var moveState = new MoveToState("Walking", _movement);
        var lookBehindState = new LookingBehindState("LookingBehind", _animator, _movement);
        var inspectingState = new InspectingState("Inspecting", _animator, _movement, _inspectDuration);


        // Idle -> Walking (dopo il caricamento/attesa iniziale)
        _fsm.AddTransition(idleState, moveState, () => Time.timeSinceLevelLoad > _initialIdleWait);

        // Walking -> LookingBehind (quando arriva al punto)
        _fsm.AddTransition(moveState, lookBehindState, () => _movement.HasReachedDestination);

        // LookingBehind -> Inspecting (fine animazione guardata)
        _fsm.AddTransition(lookBehindState, inspectingState, () => lookBehindState.IsAnimationFinished());

        // Inspecting -> Walking (fine ispezione e prossimo punto)
        _fsm.AddTransition(inspectingState, moveState, () => inspectingState.IsFinished());

        _fsm.SetState(idleState);
    }

    void Update()
    {
        if (this == null || _fsm == null) return;

        if (_isTalking) return;  


        UpdateAnimationParameters();

        _fsm.Tik();
    }

    private void UpdateAnimationParameters()
    {
        if (_animator == null || _movement == null) return;

        if (_isTalking) return;


        if (_fsm.CurrentStateName == "Walking")
        {
            var agent = _movement.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                float speed = agent.velocity.magnitude / agent.speed;
                _animator.SetFloat("Speed", speed > 0.1f ? speed : 0f);
                _animator.SetBool("Walking", speed > 0.1f);
            }
        }
    }

    private void OnDestroy() => _fsm = null;
}


public class LookingBehindState : State
{
    private Animator _animator;
    private NpcMovement _movement;
    private bool _isDone;
    private float _timer;
    private const float ANIM_DURATION = 2.5f; 

    public LookingBehindState(string name, Animator animator, NpcMovement movement) : base(name)
    {
        _animator = animator;
        _movement = movement;
    }

    public override void Enter()
    {
        _movement.StopMovement(); 
        _isDone = false;
        _timer = 0f;

        if (_animator != null)
            _animator.SetBool("Looking", true);
    }

    public override void Tik()
    {
        _timer += Time.deltaTime;
        if (_timer >= ANIM_DURATION) _isDone = true;
    }

    public bool IsAnimationFinished() => _isDone;

    public override void Exit()
    {
        if (_animator != null)
            _animator.SetBool("Looking", false);
    }
}

public class InspectingState : State
{
    private Animator _animator;
    private NpcMovement _movement;
    private float _duration;
    private float _timer;

    public InspectingState(string name, Animator animator, NpcMovement movement, float duration) : base(name)
    {
        _animator = animator;
        _movement = movement;
        _duration = duration;
    }

    public override void Enter()
    {
        _timer = 0f;

    }

    public override void Tik() => _timer += Time.deltaTime;

    public bool IsFinished() => _timer >= _duration;

    public override void Exit()
    {
        if (_movement != null)
            _movement.GoToNextWaypoint();
    }
}