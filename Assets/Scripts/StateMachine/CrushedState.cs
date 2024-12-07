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

        Debug.Log("Entered Crushed State");
    }

    public override void UpdateState()
    {
        playerMovement.HandleMovement();

        currentTime -= Time.deltaTime;
        if(currentTime <= 0)
        {
            //playerMovement.ChangeSize(1f);
            playerStateMachine.SwitchState(PlayerStates.Default);
        }
    }
}
