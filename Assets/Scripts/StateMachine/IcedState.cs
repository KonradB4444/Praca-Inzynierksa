using UnityEngine;

public class IcedState : PlayerBaseState
{
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

        //slidingVelocity = playerMovement.GetCurrentVelocity();
        //slidingVelocity = playerMovement.transform.forward * Mathf.Max(slidingSpeed, 0.1f); // debugging
        /*if (slidingVelocity.magnitude < slidingSpeed)
        {
            slidingVelocity = playerMovement.transform.forward * slidingSpeed;
        }*/

        playerMovement.SetCurrentVelocity(slidingVelocity);
    }
    public override void UpdateState()
    {
        //base.UpdateState();
        playerMovement.HandleGravity();
        if (playerMovement.GetCurrentVelocity().magnitude < exitThresholdSpeed || HitFlatWall())
        {
            playerStateMachine.SwitchState(PlayerStates.Default);
            return;
        }

        HandleSliding();
        if (playerMovement.IsWallConnectedToSlope())
        {
            PerformLaunch();
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        playerMovement.SetCurrentVelocity(Vector3.zero);
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
        Debug.Log($"Slope Direction: {slopeDirection}"); // debugging

        //slidingVelocity = SnapToCardinalDirection(slidingVelocity.normalized) * slidingVelocity.magnitude;

        //slidingVelocity = playerMovement.AdjustSlidingVelocity(slidingVelocity, slopeDirection, slidingSpeed);

        playerMovement.ApplyCharacterMove(slidingVelocity);

        //Debug.Log($"Applying Move: {slidingVelocity}"); // debugging

    }

    private void PerformLaunch()
    {
        float currentSpeed = slidingSpeed = slidingVelocity.magnitude;
        Vector3 launchVelocity = playerMovement.CalculateLaunchVelocity(currentSpeed);

        playerMovement.SetCurrentVelocity(launchVelocity);
        playerStateMachine.SwitchState(PlayerStates.Default);
    }

    private bool HitFlatWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerMovement.transform.position, playerMovement.transform.forward, out hit, 1f))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return Mathf.Abs(angle - 90f) < 5f;
        }
        return false;
    }
}
