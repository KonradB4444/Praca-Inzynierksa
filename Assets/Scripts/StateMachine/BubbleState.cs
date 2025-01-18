using System.Collections.Generic;
using UnityEngine;

public class BubbleState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
    new HashSet<PlayerStates>
    {
            PlayerStates.Default,
            PlayerStates.Spike
    };
    private float buoyancyForce = 5f;
    private float maxBuoyancyForce = 30f;
    private PlayerMovement playerMovement;
    private float jumpForce = 5f;
    private float driftForce = 7f;

    private float sinkingForce = -5f;
    private float maxSinkingForce = -30f;

    private bool isSinking = false;

    private GameObject bubbleGameObject;


    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);

        bubbleGameObject = player.transform.Find("Bubble").gameObject;
        bubbleGameObject.SetActive(true);

        playerMovement = player.GetComponent<PlayerMovement>();
        groundCheck = player.GetComponentInChildren<GroundCheck>();
        
        buoyancyForce = 5f;
        sinkingForce = -5;

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement is missing in BubbleState!");
        }

        Debug.Log("Entered Bubble State");

        playerMovement.OnPlayerCollide += HandleCollision;
    }

    public override void UpdateState()
    {
        sinkingForce = Mathf.Clamp(sinkingForce + sinkingForce * Time.deltaTime, maxSinkingForce, sinkingForce);

        buoyancyForce = Mathf.Clamp(buoyancyForce + buoyancyForce * Time.deltaTime, buoyancyForce, maxBuoyancyForce);

        if (buoyancyForce > 15f && playerMovement.IsInWater)
        {
            playerMovement.MoveVertical(buoyancyForce);

            playerStateMachine.SwitchState(PlayerStates.Default);
            Debug.Log("Bubble Burst! Transitioning to Default State.");
            return;
        }

        if (groundCheck.isGrounded || !isSinking || !playerMovement.IsInWater)
        {
            sinkingForce = -5f;
        }

        Vector3 currentForce = playerMovement.GetForce();
        playerMovement.characterController.Move(currentForce * Time.deltaTime);

        playerMovement.AddForce(-currentForce * 5f * Time.deltaTime);

        playerMovement.HandleMovement();
        playerMovement.HandleGravity();

        HandleJump();

        // Raycast for checking if player is standing on water surface
        Vector3 rayOrigin = playerMovement.transform.position;
        RaycastHit hit;
        bool isTouchingWaterSurface = Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            playerMovement.characterController.height / 2f + 0.1f,
            LayerMask.GetMask("Water")
        );

        Debug.DrawRay(rayOrigin, Vector3.down * (playerMovement.characterController.height / 2f + 0.1f), Color.blue);

        if (isTouchingWaterSurface || !playerMovement.IsInWater || isSinking)
        {
            buoyancyForce = 5f;
        }

        if (isTouchingWaterSurface || playerMovement.IsInWater)
        {
            HandleFloating();
        }
        else
        {
            playerMovement.ApplyCharacterMove();
        }

        Debug.Log($"Is Sinking: {isSinking}");
        Debug.Log($"Sinking Force: {sinkingForce}");
        Debug.Log($"Buoyancy Force: {buoyancyForce}");
    }


    private void HandleFloating()
    {
        if (playerMovement.inputManager.GetCrouchInput())
        {
            isSinking = true;
        }
        else
        {
            isSinking = false;
        }

        float playerBottomY = playerMovement.transform.position.y - (playerMovement.characterController.height / 2f);
        float targetY = playerMovement.waterSurfaceY;
        float offset = targetY - playerBottomY;

        float force = isSinking
            ? 5f * sinkingForce // Apply sinking force if sinking
            : offset * buoyancyForce; // Apply buoyancy otherwise

        Debug.Log($"Applied Force: {force}");

        if (isSinking) force = Mathf.Clamp(force, sinkingForce, -sinkingForce);
        else force = Mathf.Clamp(force, -buoyancyForce, buoyancyForce);

        playerMovement.MoveVertical(force);

        playerMovement.ApplyCharacterMove();
    }

    private void HandleJump()
    {
        playerMovement.HandleJump(
            autoJump: false,
            jumpCooldown: 0.5f,
            jumpMultiplier: 1f,
            jumpForce: jumpForce
        );
    }


    public override void ExitState()
    {
        base.ExitState();
        Debug.Log("Exited Bubble State");
        bubbleGameObject.SetActive(false);
        playerMovement.ResetForce();
        playerMovement.OnPlayerCollide -= HandleCollision;
    }
    private void HandleCollision(ControllerColliderHit hit)
    {
        Vector3 pushDirection = hit.normal;
        pushDirection.y = 0;

        playerMovement.AddForce(pushDirection * driftForce);
        Debug.Log($"Drifting Applied: {pushDirection}, Force: {-pushDirection * driftForce}");

    }

}
