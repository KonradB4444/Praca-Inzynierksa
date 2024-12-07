using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float verticalVelocity;
    [SerializeField] private Transform orientation;

    [SerializeField] private float maxCheckDistance = 2.0f;
    [SerializeField] private float wallConnectionTolerance = 5f; // Tolerance for wall angle (degrees)

    [SerializeField] private float baseLaunchForce = 5f;
    [SerializeField] private float speedMultiplier = 0.7f;
    [SerializeField] private float minLaunchHeight = 5f;
    [SerializeField] private float maxLaunchHeight = 15f;

    [SerializeField] private float minimumSlidingSpeed = 2f;

    private Vector3 currentVelocity = Vector3.zero;



    private float jumpCooldownTimer = 0f;

    public CharacterController characterController;
    private Vector3 velocity;
    [SerializeField] private bool isGrounded;

    [SerializeField] public InputManager inputManager;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void ApplyCharacterMove(Vector3 newVelocity)
    {
        characterController.Move(newVelocity * Time.deltaTime);
    }
    public void ApplyCharacterMove()
    {
        characterController.Move(velocity * Time.deltaTime);
    }

    public void HandleMovement()
    {
        Vector2 input = inputManager.GetMovementInput();

        //Vector3 move = new Vector3(input.x, 0, input.y);

        Vector3 move = orientation.right * input.x + orientation.forward * input.y;
        //characterController.Move(move * walkSpeed * Time.deltaTime);

        //move = transform.TransformDirection(move);

        velocity.x = move.x * walkSpeed;
        velocity.z = move.z * walkSpeed;

        //return move * walkSpeed;
    }

    public void HandleJump(
    bool autoJump = false, 
    float jumpCooldown = 0f,
    float jumpMultiplier = 1f
    )
    {
        /*if (characterController.isGrounded)
        {
            verticalVelocity = 0;
            // Check for cooldown and apply jump multiplier
            if ((inputManager.GetJumpInputDown() || autoJump) && jumpCooldownTimer >= jumpCooldown)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * jumpMultiplier * gravity * 2);
                jumpCooldownTimer = 0f; // Reset cooldown
            }
        }

        jumpCooldownTimer += Time.deltaTime;*/

        if(characterController.isGrounded && CanJump())
        {
            if (autoJump)
            {
                Jump(jumpCooldown, jumpMultiplier);
            }
            else if (inputManager.GetJumpInputDown())
            {
                Jump(jumpCooldown, jumpMultiplier);
            }
        }
    }

    private void Jump(float jumpCooldown, float jumpMultiplier)
    {
        velocity.y = Mathf.Sqrt(jumpHeight * jumpMultiplier * 2f * gravity);
        jumpCooldownTimer = jumpCooldown;
    }

    private bool CanJump()
    {
        if (jumpCooldownTimer <= 0f) return true;

        jumpCooldownTimer -= Time.deltaTime;
        return false;
    }


    public void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            Vector3 groundNormal = GetGroundNormal();
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            if (slopeAngle > characterController.slopeLimit)
            {
                velocity.y = 0f;
            }
            else
            {
                velocity.y = -1f;
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        /*    verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }*/
    }

    public void SetCurrentVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
    }
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public Vector3 AdjustSlidingVelocity(Vector3 currentVelocity, Vector3 slopeDirection, float speed)
    {
        Vector3 slidingVelocity = currentVelocity + (slopeDirection * speed * Time.deltaTime);

        if (slidingVelocity.magnitude < minimumSlidingSpeed)
        {
            slidingVelocity = slopeDirection.normalized * minimumSlidingSpeed;
        }

        return slidingVelocity;
    }

    public void SetHitboxSize(Vector3 newSize)
    {
        characterController.center = new Vector3(0, newSize.y / 2, 0);
        characterController.height = newSize.y;
    }
    public void MoveAlongSlide(Vector3 slidingVelocity) //Vector3 direction, float speed
    {
        velocity = slidingVelocity;
    }

    public void SetMovementSpeed(float newSpeed)
    {
        walkSpeed = newSpeed;
    }

    public void ResetMovementSpeed()
    {
        walkSpeed = 5f; // Reset to default speed or adjust as needed
    }

    public void SetJumpHeight(float newHeight)
    {
        jumpHeight = newHeight;
    }

    public void ResetJumpHeight()
    {
        jumpHeight = 2f; // Default jump height
    }

    public void ModifyGravity(float newGravity)
    {
        gravity = newGravity;
    }

    public void ResetGravity()
    {
        gravity = 9.81f; // Default gravity
    }
    public bool CanUseAction(ref float cooldownTimer, float cooldownDuration)
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= cooldownDuration)
        {
            cooldownTimer = 0f;
            return true;
        }

        return false;
    }

    public Vector3 GetGroundNormal()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height / 2 + 0.2f))
        {
            Debug.Log($"Ground Normal: {hit.normal}"); // debugging
            return hit.normal;
        }

        Debug.DrawRay(transform.position, Vector3.down * (characterController.height / 2 + 0.2f), Color.red); // debugging

        Debug.Log("No raycast hit?");
        return Vector3.up;
    }

    public float GetSlopeAngle()
    {
        Vector3 groundNormal = GetGroundNormal();
        return Vector3.Angle(groundNormal, Vector3.up);
    }

    public bool IsWallConnectedToSlope() // need a better name for this
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxCheckDistance))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return Mathf.Abs(angle - 90f) < wallConnectionTolerance;
        }
        return false;
    }

    public Vector3 CalculateLaunchVelocity(float speed)
    {
        float launchHeight = Mathf.Clamp(baseLaunchForce + (speed * speedMultiplier), minLaunchHeight, maxLaunchHeight);
        return new Vector3(currentVelocity.x, launchHeight, currentVelocity.z);
    }

    /*public Vector3 AdjustSlidingVelocity(Vector3 currentVelocity, Vector3 slopeDirection, float speed)
    {
        // Calculate new sliding velocity
        Vector3 slidingVelocity = currentVelocity + (slopeDirection * speed * Time.deltaTime);

        // Enforce minimum sliding speed
        if (slidingVelocity.magnitude < minimumSlidingSpeed)
        {
            slidingVelocity = slopeDirection.normalized * minimumSlidingSpeed;
        }

        return slidingVelocity;
    }*/
}
