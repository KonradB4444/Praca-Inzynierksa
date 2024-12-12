using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushedState : PlayerBaseState
{
    private float totalTime = 3f;
    private float currentTime;

    private PlayerMovement playerMovement;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        currentTime = totalTime;

        playerMovement = player.GetComponent<PlayerMovement>();

        //playerMovement.ChangeSize(0.10f);
        playerMovement.SquishPlayer(0.5f);

        Debug.Log("Entered Crushed State");
    }

    public override void UpdateState()
    {
        playerMovement.HandleMovement();
        playerMovement.ApplyCharacterMove();

        if (!playerMovement.characterController.isGrounded)
        {
            playerMovement.SetCurrentVelocity(new Vector3(
            playerMovement.GetCurrentVelocity().x,
            playerMovement.GetCurrentVelocity().y * 0.5f, // Slow descent by 50%
            playerMovement.GetCurrentVelocity().z
            ));
        }
        currentTime -= Time.deltaTime;
        if(currentTime <= 0)
        {
            //playerMovement.ChangeSize(1f);
            playerStateMachine.SwitchState(PlayerStates.Default);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        playerMovement.ResetPlayerScale();
    }
}
