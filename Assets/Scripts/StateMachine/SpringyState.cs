using System.Collections.Generic;
using UnityEngine;

public class SpringyState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates>
        {
            PlayerStates.Default
        };

    private PlayerMovement playerMovement;

    private float windUpMaxTime = 2f;
    private float windUpTimer = 0f;

    private float jumpCooldown = 0.5f;
    private float maxJumpMultiplier = 1.5f;
    private float jumpMultiplier = 1f;

    private bool isKeyReleased = true;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        playerMovement = player.GetComponent<PlayerMovement>();

        Debug.Log("Entered Springy State");
    }

    public override void UpdateState()
    {
        playerMovement.HandleGravity();
        HandleSpringyJump();
        if(!playerMovement.characterController.isGrounded)
        {
            playerMovement.HandleMovement();
        }
        playerMovement.ApplyCharacterMove();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Springy State");
        playerMovement.ResetMovementSpeed();
        playerMovement.ResetJumpHeight();
        playerMovement.ResetGravity();
    }

    private void HandleSpringyJump()
    {
        if(playerMovement.inputManager.GetJumpInputUp())
        {
            isKeyReleased = true;
        }

        if (isKeyReleased && playerMovement.inputManager.GetJumpInput())
        {
            windUpTimer += Time.deltaTime;

            if (windUpMaxTime <= windUpTimer)
            {
                var value = Mathf.Clamp01(windUpTimer / windUpMaxTime);
                jumpMultiplier = Mathf.Lerp(1f, maxJumpMultiplier, value);

                playerMovement.HandleJump(true, 0f, jumpMultiplier);

                isKeyReleased = false;
                windUpTimer = 0;
            }
        }
        else
        {

            if (windUpTimer > 0)
            {
                var value = Mathf.Clamp01(windUpTimer / windUpMaxTime);
                jumpMultiplier = Mathf.Lerp(1f, maxJumpMultiplier, value);

                playerMovement.HandleJump(true, 0f, jumpMultiplier);

                windUpTimer = 0;
            }
            else
            {
                playerMovement.HandleJump(true, 1f);
            }
        }

    }
}
