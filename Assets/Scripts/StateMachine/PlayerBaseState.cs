using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerStateMachine playerStateMachine;
    public virtual void EnterState(PlayerStateMachine player)
    {
        playerStateMachine = player;
    }

    public virtual void UpdateState() { }

    public virtual void ExitState() { }
}

