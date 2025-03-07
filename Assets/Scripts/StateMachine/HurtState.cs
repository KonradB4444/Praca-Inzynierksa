using System.Collections.Generic;
using UnityEngine;

public class HurtState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates>
        {
            PlayerStates.Default
        };

    private PlayerMovement playerMovement;
    private InputManager inputManager;

    private Color defaultColor = Color.white;
    private Color hurtColor = Color.red;
    private Color recoveryColor = Color.blue;

    private float recoveryWindow = 0.5f;
    private float recoveryTimer = 0f;
    private bool recoveryAvailable = false;
    private bool recoveryTriggeredOnce = false;

    private float hurtDuration = 5f;
    private float hurtTimer = 0f;

    private float recoveryDelay = 1f;
    private bool recoveryWindowReady = false;
    private bool recoveryBlocked = false;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        playerMovement = player.GetComponent<PlayerMovement>();
        inputManager = player.GetComponent<InputManager>();
        groundCheck = player.GetComponentInChildren<GroundCheck>();

        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck component is missing from the player.");
            return;
        }

        inputManager.CanJump = false;

        Vector3 lastInput = inputManager.GetMovementInput();
        Vector3 pushDirection;

        if (lastInput != Vector3.zero)
        {
            pushDirection = new Vector3(-lastInput.x, -lastInput.z, -lastInput.y).normalized;
        }
        else
        {
            pushDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }

        playerMovement.AddForce(pushDirection * 20f);

        playerMovement.SetPlayerColor(hurtColor);

        recoveryTimer = 0f;
        hurtTimer = 0f;
        recoveryAvailable = false;
        recoveryWindowReady = false;
        recoveryTriggeredOnce = false;
        recoveryBlocked = false;

        Debug.Log("Entered Hurt State");
    }

    public override void UpdateState()
    {
        hurtTimer += Time.deltaTime;
        playerMovement.HandleGravity();
        playerMovement.ApplyCharacterMove();

        if (groundCheck.isGrounded)
        {
            playerMovement.SetCurrentVelocity(new Vector3(0f, 0f, 0f));
        }

        // Check for spamming before recovery window
        if (!recoveryWindowReady && inputManager.GetJumpInputDown())
        {
            recoveryBlocked = true;
            Debug.Log("Recovery blocked due to spamming.");
        }

        if (hurtTimer >= recoveryDelay && !recoveryBlocked)
        {
            recoveryWindowReady = true;
        }

        if (groundCheck.isGrounded && !recoveryAvailable && recoveryWindowReady && !recoveryTriggeredOnce)
        {
            recoveryAvailable = true;
            recoveryTriggeredOnce = true;
            recoveryTimer = 0f;
            playerMovement.SetPlayerColor(recoveryColor);
            Debug.Log("Player landed: Recovery window started.");
        }

        if (recoveryAvailable)
        {
            recoveryTimer += Time.deltaTime;

            if (inputManager.GetJumpInputDown() && recoveryTimer <= recoveryWindow)
            {
                TriggerRecovery();
                return;
            }

            if (recoveryTimer > recoveryWindow)
            {
                recoveryAvailable = false;
                playerMovement.SetPlayerColor(hurtColor);
                Debug.Log("Recovery window ended.");
            }
        }

        if (hurtTimer >= hurtDuration)
        {
            playerStateMachine.SwitchState(PlayerStates.Default);
        }

        Vector3 currentForce = playerMovement.GetForce();
        playerMovement.characterController.Move(currentForce * Time.deltaTime);

        playerMovement.AddForce(-currentForce * 5f * Time.deltaTime);
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Hurt State");

        inputManager.CanJump = true;
        playerMovement.SetPlayerColor(defaultColor);
        playerMovement.ResetForce();
    }

    private void TriggerRecovery()
    {
        Debug.Log("Recovery Triggered!");
        recoveryAvailable = false;
        playerMovement.SetPlayerColor(defaultColor);

        float jumpForce = 10f;
        playerMovement.Jump(1f, 1f, jumpForce);

        playerStateMachine.SwitchState(PlayerStates.Default);
    }
}
