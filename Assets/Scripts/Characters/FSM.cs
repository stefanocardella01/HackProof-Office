using System;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine<T>
{
    private T _owner;
    private State _currentState;
    private Dictionary<string, List<Transition>> _transitions = new Dictionary<string, List<Transition>>();
    private List<Transition> _currentTransitions = new List<Transition>();

    public FiniteStateMachine(T owner)
    {
        _owner = owner;
    }

    public void Tik()
    {
        State nextState = GetNextState();
        if (nextState != null)
            SetState(nextState);

        _currentState?.Tik();
    }

    public void SetState(State state)
    {
        if (state == _currentState)
            return;

        _currentState?.Exit();
        
        _currentState = state;

        _transitions.TryGetValue(_currentState.Name, out _currentTransitions);

        _currentState.Enter();
    }

    public void AddTransition(State fromState, State toState, Func<bool> transitionCondition)
    {
        if (!_transitions.TryGetValue(fromState.Name, out var stateTransitions))
        {
            stateTransitions = new List<Transition>();
            _transitions[fromState.Name] = stateTransitions;
        }

        stateTransitions.Add(new Transition(toState, transitionCondition));
    }

    private State GetNextState()
    {
        foreach (Transition transition in _currentTransitions)
        {
            if (transition.Condition())
                return transition.NextState;
        }

        return null;
    }

    public string CurrentStateName => _currentState?.Name;
}

