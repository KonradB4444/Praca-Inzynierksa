using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushedState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates>
        {
            PlayerStates.Default
        };

    private bool isInSlimeTrigger = false;

    private float totalTime = 3f;
    private float currentTime;

    private PlayerMovement playerMovement;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        currentTime = totalTime;

        playerMovement = player.GetComponent<PlayerMovement>();

        playerMovement.SquishPlayer(0.5f);
        playerMovement.SetGravityModifier(0.5f);

        Debug.Log("Entered Crushed State");
    }

    public override void UpdateState()
    {
        playerMovement.HandleMovement();
        playerMovement.HandleGravity();
        playerMovement.HandleJump();
        playerMovement.ApplyCharacterMove();

        if (isInSlimeTrigger == true)
        {
            currentTime = totalTime;
        }
        else
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                playerStateMachine.SwitchState(PlayerStates.Default);
            }
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        playerMovement.ResetPlayerScale();
        playerMovement.ResetGravityModifier();
    }

    public void SetInSlimeTrigger(bool value)
    {
        isInSlimeTrigger = value;
    }
}
