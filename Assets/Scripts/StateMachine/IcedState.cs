using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private GameObject iceGameObject;
    private float maxSlidingSpeed = 10f;
    private bool isSlidingDisabled = false;
    private bool isLaunched = false;
    private bool isGroundCheckReady = false;

    private Vector3 lastVelocity = Vector3.zero;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);

        playerMovement = player.GetComponent<PlayerMovement>();
        groundCheck = player.GetComponentInChildren<GroundCheck>();
        iceGameObject = player.transform.Find("Ice").gameObject;

        iceGameObject.SetActive(true);

        isLaunched = false;

        Debug.Log("Entered Iced State");

        Vector3 initialDirection = playerMovement.GetCurrentVelocity().normalized;

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
        playerMovement.HandleGravity();
        playerMovement.ApplyCharacterMove();

        Debug.Log($"sliding disabled: {isSlidingDisabled}");
        Debug.Log($"ground check ready: {isGroundCheckReady}");
        Debug.Log($"is launched: {isLaunched}");

        if (slidingVelocity.magnitude < exitThresholdSpeed)
        {
            Debug.Log("Exiting Iced State due to low speed.");
            playerStateMachine.SwitchState(PlayerStates.Default);
        }

        if (isSlidingDisabled)
        {
            if (isGroundCheckReady && groundCheck.isGrounded)
            {
                Debug.Log("Player has landed, re-enabling sliding.");
                Debug.Log($"Dupa: {lastVelocity}");
                playerMovement.SetCurrentVelocity(lastVelocity);
                isGroundCheckReady = false;
                isSlidingDisabled = false;
                isLaunched = false;
            }
        }
        else 
            HandleSliding();
    }

    public override void ExitState()
    {
        base.ExitState();
        playerMovement.SetCurrentVelocity(Vector3.zero);
        iceGameObject.SetActive(false);
        slidingVelocity = Vector3.zero;
        slidingSpeed = 5f;
    }

    private static readonly Vector3[] cardinalDirections =
    {
        Vector3.forward, // North
        Vector3.right,   // East
        Vector3.back,    // South
        Vector3.left     // West
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
        Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        Vector3 slidingForce = slopeDirection * slidingSpeed * Time.deltaTime;

        float horizontalInput = playerMovement.inputManager.GetHorizontalInput();
        Vector3 lateralMovement = playerMovement.orientation.right * horizontalInput * slidingSpeed * Time.deltaTime;

        slidingVelocity += slidingForce;
        slidingVelocity += lateralMovement;

        if (slidingVelocity.magnitude > maxSlidingSpeed)
        {
            slidingVelocity = slidingVelocity.normalized * maxSlidingSpeed;
        }

        // Apply the combined velocity
        if (!isSlidingDisabled)
        {
            Debug.Log($"Dupa: Sliding Velocity {slidingVelocity}");
        }

        // Detect wall collisions
        if (Physics.Raycast(playerMovement.transform.position, playerMovement.transform.forward, out RaycastHit hitFirst, 1f))
        {
            if (!hitFirst.transform.tag.Equals("Player"))
                HandleWallCollision(hitFirst.collider);
        }
        else if (Physics.Raycast(playerMovement.transform.position, -playerMovement.transform.forward, out RaycastHit hitSecond, 1f))
        {
            if (!hitSecond.transform.tag.Equals("Player"))
                HandleWallCollision(hitSecond.collider);
        }
        else if (Physics.Raycast(playerMovement.transform.position, playerMovement.transform.right, out RaycastHit hitThird, 1f))
        {
            if (!hitThird.transform.tag.Equals("Player"))
                HandleWallCollision(hitThird.collider);
        }
        else if (Physics.Raycast(playerMovement.transform.position, -playerMovement.transform.right, out RaycastHit hitFourth, 1f))
        {
            if (!hitFourth.transform.tag.Equals("Player"))
                HandleWallCollision(hitFourth.collider);
        }
    }

    private void PerformLaunch()
    {
        Debug.Log("Performing Launch!");

        isSlidingDisabled = true;

        var velocity = playerMovement.GetCurrentVelocity();
        Debug.Log($"Dupa: Current Velocity {velocity}");
        lastVelocity = new Vector3(velocity.x, Mathf.Abs(velocity.y) , velocity.z) * -1f;

        // Allow some side-to-side control during the launch
        float horizontalInput = playerMovement.inputManager.GetHorizontalInput();
        Vector3 lateralMovement = playerMovement.orientation.right * horizontalInput * slidingSpeed;

        playerMovement.SetCurrentVelocity(new Vector3(lateralMovement.x, velocity.y, lateralMovement.z));

        playerMovement.Jump(0f, 1.5f);

        ResetWallCollisionFlag();

        DelayGroundCheck();
    }

    private async void DelayGroundCheck()
    {
        await Task.Delay(100);

        isGroundCheckReady = true;
    }

    public void HandleWallCollision(Collider collider)
    {
        if (collider.CompareTag("WallWithSlope"))
        {
            Debug.Log("Wall connected to slope detected. Launching player.");
            if (!isLaunched)
            {
                PerformLaunch();
                isLaunched = true;
            }

            Debug.Log($"velocity: {playerMovement.GetCurrentVelocity()}");
        }
        else
        {
            Debug.Log("Normal wall detected. Exiting Iced State.");
            playerStateMachine.SwitchState(PlayerStates.Default);
        }
    }

    public void ResetWallCollisionFlag()
    {
        playerMovement.ResetWallCollisionFlag();
    }

}
