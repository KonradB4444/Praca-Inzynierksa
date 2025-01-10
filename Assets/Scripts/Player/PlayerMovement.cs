using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float gravityModifier = 1f;
    [SerializeField] private float verticalVelocity;
    [SerializeField] private Transform orientation;

    [SerializeField] private float maxCheckDistance = 2.0f;
    //[SerializeField] private float wallConnectionTolerance = 5f; // Tolerance for wall angle (degrees)

    [SerializeField] private float baseLaunchForce = 5f;
    [SerializeField] private float speedMultiplier = 0.7f;
    [SerializeField] private float minLaunchHeight = 5f;
    [SerializeField] private float maxLaunchHeight = 15f;

    [SerializeField] private float minimumSlidingSpeed = 2f;
    public float MinimumSlidingSpeed => minimumSlidingSpeed;

    public float Gravity => gravity;

    public float WalkSpeed => walkSpeed;

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

    public void Jump(float jumpCooldown, float jumpMultiplier)
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
            velocity.y -= gravity * gravityModifier * Time.deltaTime;
        }

        /*    verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }*/
    }

    public void SetGravityModifier(float modifier)
    {
        gravityModifier = modifier;
    }

    public void ResetGravityModifier()
    {
        gravityModifier = 1f;
    }

    public void SetCurrentVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
    }
    public Vector3 GetCurrentVelocity()
    {
        return characterController.velocity;
    }

    public Vector3 AdjustSlidingVelocity(Vector3 slopeDirection, float speed)
    {
        velocity += slopeDirection * speed * Time.deltaTime;

        if (velocity.magnitude < minimumSlidingSpeed)
        {
            velocity = slopeDirection.normalized * minimumSlidingSpeed;
        }

        return velocity;
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
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height / 2 + 0.3f))
        {
            //Debug.Log($"Ground Normal: {hit.normal}"); // debugging
            return hit.normal;
        }

        Debug.DrawRay(transform.position, Vector3.down * (characterController.height / 2 + 0.3f), Color.red); // debugging

        Debug.Log("No raycast hit?");
        return Vector3.up;
    }

    public float GetSlopeAngle()
    {
        Vector3 groundNormal = GetGroundNormal();
        return Vector3.Angle(groundNormal, Vector3.up);
    }

    /*public bool IsWallConnectedToSlope(float wallConnectionTolerance = 5f) // need a better name for this
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, orientation.forward, out hit, maxCheckDistance))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return Mathf.Abs(angle - 90f) < wallConnectionTolerance;
        }
        return false;
    }*/

    public Vector3 CalculateLaunchVelocity(float speed)
    {
        float launchHeight = Mathf.Clamp(baseLaunchForce + (speed * speedMultiplier), minLaunchHeight, maxLaunchHeight);
        return new Vector3(velocity.x, launchHeight, velocity.z);
    }

    public void SquishPlayer(float squishFactor)
    {
        squishFactor = Mathf.Clamp(squishFactor, 0.1f, 1f);

        transform.localScale = new Vector3(
        transform.localScale.x,
        squishFactor,
        transform.localScale.z
        );
    }
    public void ResetPlayerScale()
    {
        transform.localScale = new Vector3(
            transform.localScale.x,
            1f,
            transform.localScale.z
        );
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
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // angle between collision normal and "up"
        float angle = Vector3.Angle(hit.normal, Vector3.up);

        // If angle is near 90‹, itfs a wall
        // Tweak the thresholds (80, 100, etc.) as you need
        if (angle > 80f && angle < 100f)
        {
            // This is pretty close to a vertical surface
            Debug.Log("We hit a WALL.");
            IsWallConnectedToSlope();
            Debug.Log($"Does it work?: {IsWallConnectedToSlope()}");
        }
    }

    public bool IsWallConnectedToSlope()
    {
        // We do a raycast straight down from the characterfs position
        // (or from the base, offset by a small amount).
        RaycastHit groundHit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // slight offset to avoid self-intersection
        
        // The maximum distance should be enough to definitely hit the ground
        float maxDistance = 1.4f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out groundHit, maxDistance))
        {
            // Calculate the angle between the ground normal and up
            float angle = Vector3.Angle(groundHit.normal, Vector3.up);

            if (angle > 25f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false; //no ground below player
        }
    }

}
