using UnityEngine;

public class DefaultState : PlayerBaseState
{
    private PlayerMovement playerMovement;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        playerMovement = player.GetComponent<PlayerMovement>();

        Debug.Log("Entered Default State");
    }

    public override void UpdateState()
    {
        playerMovement.HandleMovement();
        playerMovement.HandleGravity();
        playerMovement.HandleJump();

        playerMovement.ApplyCharacterMove();
    }
}