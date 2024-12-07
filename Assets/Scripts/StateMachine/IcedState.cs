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

        slidingVelocity = playerMovement.GetCurrentVelocity();
        //slidingVelocity = playerMovement.transform.forward * Mathf.Max(slidingSpeed, 0.1f); // debugging
        if (slidingVelocity.magnitude < slidingSpeed)
        {
            slidingVelocity = playerMovement.transform.forward * slidingSpeed;
        }

        playerMovement.SetCurrentVelocity(slidingVelocity);
    }
    public override void UpdateState()
    {
        //base.UpdateState();
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
        //base.ExitState();
        playerMovement.SetCurrentVelocity(Vector3.zero);
    }

    private void HandleSliding()
    {
        Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, playerMovement.GetGroundNormal()).normalized;

        Debug.Log($"Slope Direction: {slopeDirection}"); // debugging

        slidingVelocity = playerMovement.AdjustSlidingVelocity(slidingVelocity, slopeDirection, slidingSpeed);

        playerMovement.MoveAlongSlide(slidingVelocity);

        Debug.Log($"Applying Move: {slidingVelocity}"); // debugging

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
