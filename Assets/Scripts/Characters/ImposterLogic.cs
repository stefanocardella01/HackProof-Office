using UnityEngine;

public class NpcPatrolBrain : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _inspectDuration = 4f;
    [SerializeField] private float _initialIdleWait = 2f;

    private NpcMovement _movement;
    private Animator _animator;
    private FiniteStateMachine<NpcPatrolBrain> _fsm;

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

        // --- LOGICA TRANSIZIONI ---

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

        // Gestione Speed/Walking costante durante il movimento
        // come richiesto per replicare il comportamento di NpcBrain
        UpdateAnimationParameters();

        _fsm.Tik();
    }

    private void UpdateAnimationParameters()
    {
        if (_animator == null || _movement == null) return;

        // Se siamo nello stato "Walking", lasciamo che i parametri riflettano il movimento
        // Altrimenti (Idle, Looking, Inspecting) forziamo a zero
        if (_fsm.CurrentStateName == "Walking")
        {
            // Reclutiamo la logica di calcolo velocità dall'agente
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
    private const float ANIM_DURATION = 2.5f; // Durata stimata della clip LookBehind

    public LookingBehindState(string name, Animator animator, NpcMovement movement) : base(name)
    {
        _animator = animator;
        _movement = movement;
    }

    public override void Enter()
    {
        _movement.StopMovement(); // Forza lo stop e resetta i parametri walking nell'animator
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
        // In questo stato l'animator è in Idle (Walking=false, Looking=false)
        // come da transizioni del tuo schema.
    }

    public override void Tik() => _timer += Time.deltaTime;

    public bool IsFinished() => _timer >= _duration;

    public override void Exit()
    {
        // Prepariamo il prossimo obiettivo prima di uscire
        if (_movement != null)
            _movement.GoToNextWaypoint();
    }
}