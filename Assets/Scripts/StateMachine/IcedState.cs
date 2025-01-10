using System.Collections.Generic;
using UnityEngine;

public class IcedState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates>
        {
            PlayerStates.Default
        };

    private PlayerMovement playerMovement;
    private Vector3 slidingVelocity;

    private float slidingSpeed = 5f;
    private float exitThresholdSpeed = 1f;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);

        playerMovement = player.GetComponent<PlayerMovement>();

        Debug.Log("Entered Iced State");

        Vector3 initialDirection = playerMovement.GetCurrentVelocity().normalized;
        Debug.Log($"Current Velocity Before Entering Iced State: {playerMovement.GetCurrentVelocity()}");

        if (initialDirection == Vector3.zero) 
        {
            initialDirection = playerMovement.transform.forward;
        }

        float initialBoost = 1.5f;
        initialDirection = SnapToCardinalDirection(initialDirection);

        slidingVelocity = initialDirection * slidingSpeed * initialBoost;

        playerMovement.SetCurrentVelocity(slidingVelocity);
    }
    public override void UpdateState()
    {
        //base.UpdateState();
        playerMovement.HandleGravity();
        if (playerMovement.GetCurrentVelocity().magnitude < exitThresholdSpeed && playerMovement.IsWallConnectedToSlope() == false)
        {
            playerStateMachine.SwitchState(PlayerStates.Default);
            return;
        }

        Debug.Log($"MOST NIGGAS: {playerMovement.IsWallConnectedToSlope()}");
        if (playerMovement.IsWallConnectedToSlope() == true)
        {
            Debug.Log("Wall connected to slope.");
            PerformLaunch();
            //playerStateMachine.SwitchState(PlayerStates.Default);

        }
        else
        {
            HandleSliding();
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        playerMovement.SetCurrentVelocity(Vector3.zero);
        slidingVelocity = Vector3.zero;
        slidingSpeed = 5f;
    }

    private static readonly Vector3[] cardinalDirections =
        {
            Vector3.forward,// North
            Vector3.right,  // East
            Vector3.back,   // South
            Vector3.left    // West
        };
    private Vector3 SnapToCardinalDirection(Vector3 direction)
    {
        Vector3 closestDirection = Vector3.zero;
        float maxDot = float.MinValue;

        foreach (Vector3 cardinalDirection in cardinalDirections)
        {
            float dot = Vector3.Dot(direction, cardinalDirection);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = cardinalDirection;
            }
        }

        return closestDirection;
    }

    private void HandleSliding()
    {
        Vector3 groundNormal = playerMovement.GetGroundNormal();
        Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, playerMovement.GetGroundNormal()).normalized;

        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        if (slopeAngle > playerMovement.characterController.slopeLimit)
        {
            slidingVelocity.y = 0f;
        }
        else
        {
            slidingVelocity += slopeDirection * slidingSpeed * Time.deltaTime;
        }

        if (!playerMovement.characterController.isGrounded)
        {
            slidingVelocity += Vector3.down * playerMovement.Gravity * Time.deltaTime;
        }

        if(slidingVelocity.magnitude < playerMovement.MinimumSlidingSpeed)
        {
            slidingVelocity = slopeDirection.normalized * playerMovement.MinimumSlidingSpeed;
        }
        //Debug.Log($"Slope Direction: {slopeDirection}"); // debugging

        playerMovement.ApplyCharacterMove(slidingVelocity);
    }

    private void PerformLaunch()
    {
        float currentSpeed = slidingVelocity.magnitude;

        // Use a multiplier based on speed to make the jump stronger with more momentum
        float launchMultiplier = Mathf.Clamp(currentSpeed / playerMovement.MinimumSlidingSpeed, 1f, 2f);

        // Call the Jump function from playerMovement
        Debug.Log($"Launching player with speed: {currentSpeed} and multiplier: {launchMultiplier}");
        playerMovement.Jump(1f, launchMultiplier);
        playerMovement.ApplyCharacterMove();

        // Switch to Default State after the jump
        //playerStateMachine.SwitchState(PlayerStates.Default);
    }



/*    private bool HitFlatWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerMovement.transform.position, playerMovement.orientation.forward, out hit, 1f))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return Mathf.Abs(angle - 90f) < 5f;
        }
        Debug.Log($"Dupa: {playerMovement.orientation.forward}");
        return false;
    }*/
}
