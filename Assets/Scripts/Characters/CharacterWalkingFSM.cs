using UnityEngine;

public class NpcBrain : MonoBehaviour
{
    [SerializeField] private float _interactionDuration = 2f;

    private NpcMovement _movement;
    private FiniteStateMachine<NpcBrain> _fsm;
    private InteractionState _interactionState;

    void Start()
    {
        _movement = GetComponent<NpcMovement>();
        if (_movement == null)
        {
            Debug.LogError("NpcMovement component mancante!");
            return;
        }

        _fsm = new FiniteStateMachine<NpcBrain>(this);

        // Stati
        MoveToState moveState = new MoveToState("MoveTo", _movement);
        _interactionState = new InteractionState("Interaction", _movement, _interactionDuration);

        // Transizioni
        _fsm.AddTransition(moveState, _interactionState, () => _movement.HasReachedDestination);
        _fsm.AddTransition(_interactionState, moveState, () => _interactionState.IsFinished());

        // Stato iniziale
        _fsm.SetState(moveState);
    }

    void Update()
    {
        _fsm.Tik();

        // Quando l’interaction finisce, passa al prossimo waypoint
        if (_interactionState.IsFinished() && _fsm.CurrentStateName == _interactionState.Name)
        {
            _movement.GoToNextWaypoint();
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
        if (_movement == null) return;
        _movement.MoveToNextTarget();
    }

    public override void Tik()
    {
        // Aggiornamento movimento gestito direttamente da NpcMovement
    }

    public override void Exit()
    {
        // opzionale
    }
}




public class InteractionState : State
{
    private NpcMovement _movement;
    private float _duration;
    private float _timer;

    public InteractionState(string name, NpcMovement movement, float duration) : base(name)
    {
        _movement = movement;
        _duration = duration;
    }

    public override void Enter()
    {
        if (_movement == null) return;

        _movement.StopMovement();
        _movement.PlayInteractionAnimation(""); // puoi sostituire con il nome dell’animazione
        _timer = 0f;
    }

    public override void Tik()
    {
        _timer += Time.deltaTime;
    }

    public bool IsFinished() => _timer >= _duration;

    public override void Exit()
    {
        // opzionale: reset animazione a camminata
    }
}

