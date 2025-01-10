using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerStateMachine playerStateMachine;

    public abstract HashSet<PlayerStates> AllowedTransitions { get; }
    public virtual void EnterState(PlayerStateMachine player)
    {
        playerStateMachine = player;
    }

    public virtual void UpdateState() { }

    public virtual void ExitState() { }
}

