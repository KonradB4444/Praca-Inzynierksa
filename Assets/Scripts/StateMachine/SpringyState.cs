using System.Collections.Generic;
using UnityEngine;

public class SpringyState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates>
        {
            PlayerStates.Default,
            PlayerStates.Hurt,
            PlayerStates.Iced
        };

    private PlayerMovement playerMovement;

    private float windUpMaxTime = 2f;
    private float windUpTimer = 0f;

    private float jumpCooldown = 0.5f;
    private float maxJumpMultiplier = 2f;
    private float jumpMultiplier = 1f;

    private bool isKeyReleased = true;

    private Vector3 trueVelocity;
    private Vector3 lastPositon = Vector3.zero;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        playerMovement = player.GetComponent<PlayerMovement>();
        playerStateMachine = player.GetComponent<PlayerStateMachine>();
        groundCheck = player.GetComponentInChildren<GroundCheck>();

        Debug.Log("Entered Springy State");
    }

    public override void UpdateState()
    {
        playerMovement.HandleGravity();
        HandleSpringyJump();

        trueVelocity = (playerMovement.transform.position - lastPositon) / Time.deltaTime;
        lastPositon = playerMovement.transform.position;

        
        if (!groundCheck.isGrounded)
        {
            playerMovement.HandleMovement();
        }
        else if (trueVelocity.magnitude > 0)
        {
            var playerVelocity = playerMovement.GetCurrentVelocity();
            var newVelocity = new Vector3(0f, playerVelocity.y, 0f);
            playerMovement.SetCurrentVelocity(newVelocity);
        }

        if (Physics.Raycast(playerMovement.transform.position, playerMovement.transform.up, out RaycastHit hit, 1.1f))
        {
            if(!hit.transform.tag.Equals("Player") && !hit.transform.tag.Equals("Trigger"))
            {
                playerStateMachine.SwitchState(PlayerStates.Default);
            }
        }
        playerMovement.ApplyCharacterMove();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Springy State");
        playerMovement.SetCurrentVelocity(Vector3.zero);
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
                Debug.Log("Out");
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
                Debug.Log("Premature");
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
