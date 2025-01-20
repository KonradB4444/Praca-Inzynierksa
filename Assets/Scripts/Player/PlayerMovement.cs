using System.Collections;
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
    [SerializeField] public Transform orientation;

    [SerializeField] private float maxCheckDistance = 2.0f;

    [SerializeField] private float baseLaunchForce = 5f;
    [SerializeField] private float speedMultiplier = 0.7f;
    [SerializeField] private float minLaunchHeight = 5f;
    [SerializeField] private float maxLaunchHeight = 15f;

    [SerializeField] private float minimumSlidingSpeed = 2f;
    public float MinimumSlidingSpeed => minimumSlidingSpeed;

    public float Gravity => gravity;

    public float WalkSpeed => walkSpeed;

    private bool isMidAir = false;
    public float moveSpeedMidAir = 1f;
    public Vector3 jumpAngle = Vector3.zero;
    [SerializeField] private float allowedMidAirDirectionChange = 45f;

    private Vector3 currentVelocity = Vector3.zero;


    private float jumpCooldownTimer = 0f;

    public CharacterController characterController;
    private Vector3 velocity;
    [SerializeField] private bool isGrounded;

    [SerializeField] public InputManager inputManager;

    private Vector3 force;
    [SerializeField] private Renderer playerRenderer;
    private Color defaultColor;

    [SerializeField] private GroundCheck groundCheck;

    [SerializeField] private bool isInWater = false;
    [SerializeField] public float waterSurfaceY = 0f;

    public delegate void CollisionHandler(ControllerColliderHit hit);
    public event CollisionHandler OnPlayerCollide;

    [SerializeField] PlayerStateMachine playerStateMachine;

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
        Vector2 input = inputManager.GetMovementInput().normalized;
        Vector3 move = orientation.right * input.x + orientation.forward * input.y;
        
        if(isMidAir)
        {
            velocity.x = move.x * moveSpeedMidAir;
            velocity.z = move.z * moveSpeedMidAir;

            if(groundCheck.isGrounded || !IsDirectionSame(jumpAngle, move, allowedMidAirDirectionChange))
            {
                isMidAir = false;
            }
        }
        else
        {
            velocity.x = move.x * walkSpeed;
            velocity.z = move.z * walkSpeed;
        }
    }

    public void StartDelayIsMidAir()
    {
        StartCoroutine(DelayIsMidAir());
    }

    private IEnumerator DelayIsMidAir()
    {
        yield return new WaitForSeconds(0.1f);

        isMidAir = true;
    }

    private bool IsDirectionSame(Vector3 original, Vector3 current, float maxAngleDifference)
    {
        // Normalize the vectors to get only the direction
        Vector3 originalNormalized = original.normalized;
        Vector3 currentNormalized = current.normalized;

        // Calculate the dot product
        float dotProduct = Vector3.Dot(originalNormalized, currentNormalized);

        // Convert the dot product to an angle (in degrees)
        float angleDifference = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        // Check if the angle difference is within the acceptable range
        return angleDifference <= maxAngleDifference;
    }

    public void HandleJump(
    bool autoJump = false,
    float jumpCooldown = 0f,
    float jumpMultiplier = 1f,
    float? jumpForce = null // optional
)
    {
        if (groundCheck.isGrounded && (CanJump() || jumpCooldown == 0f))
        {
            if (autoJump)
            {
                Jump(jumpCooldown, jumpMultiplier, jumpForce);
            }
            else if (inputManager.GetJumpInputDown())
            {
                Jump(jumpCooldown, jumpMultiplier, jumpForce);
            }
        }
    }

    public void Jump(float jumpCooldown, float jumpMultiplier, float? jumpForce = null)
    {
        if (jumpForce.HasValue)
        {
            velocity.y = jumpForce.Value;
        }
        else
        {
            velocity.y = Mathf.Sqrt(jumpHeight * jumpMultiplier * 2f * gravity);
        }
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
        if (groundCheck.isGrounded)
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
                velocity.y += -0.1f * Time.deltaTime;
            }
        }
        else
        {
            velocity.y -= gravity * gravityModifier * Time.deltaTime;
        }
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
        this.velocity = velocity;
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
    public void MoveAlongSlide(Vector3 slidingVelocity)
    {
        velocity = slidingVelocity;
    }

    public void SetMovementSpeed(float newSpeed)
    {
        walkSpeed = newSpeed;
    }

    public void ResetMovementSpeed()
    {
        walkSpeed = 5f;
    }

    public void SetJumpHeight(float newHeight)
    {
        jumpHeight = newHeight;
    }

    public void ResetJumpHeight()
    {
        jumpHeight = 2f;
    }

    public void ModifyGravity(float newGravity)
    {
        gravity = newGravity;
    }

    public void ResetGravity()
    {
        gravity = 9.81f;
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

    private bool hasCollidedWithWall = false;

    public void ResetWallCollisionFlag()
    {
        hasCollidedWithWall = false;
    }
    public void AddForce(Vector3 direction)
    {
        force += direction;
    }

    public Vector3 GetForce()
    {
        return force;
    }

    public void ResetForce()
    {
        force = Vector3.zero;
    }

    public void SetPlayerColor(Color color)
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = color;
        }
    }

    public void ResetPlayerColor()
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = defaultColor;
        }
    }
    public bool IsInWater
    {
        get => isInWater;
        set => isInWater = value;
    }

    public void SetWaterSurfaceY(float surfaceY)
    {
        waterSurfaceY = surfaceY;
    }
    public void MoveVertical(float verticalSpeed)
    {
        velocity = new Vector3(velocity.x, verticalSpeed, velocity.z);
    }

    public void StartSmoothMove(Vector3 prevSpike, Vector3 nextSpike, float time)
    {
        StartCoroutine(SmoothMove(prevSpike, nextSpike, time));
    }

    private IEnumerator SmoothMove(Vector3 prevSpike, Vector3 nextSpike, float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(prevSpike, nextSpike, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = nextSpike;

        Debug.Log("Smooth move complete.");
    }

}
