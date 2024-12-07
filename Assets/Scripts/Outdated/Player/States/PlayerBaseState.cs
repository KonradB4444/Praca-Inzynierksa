using UnityEngine;

namespace Player
{
    public abstract class PlayerStateBase : IPlayerState
    {
        public float initialJumpForce = 10f;     // Initial force applied when jumping
        public float maxUpwardVelocity = 7f;     // Max upward velocity before gravity takes over
        public float minJumpTime = 0.2f;         // Minimum time jump force is applied (in seconds)
        public float maxJumpTime = 0.5f;         // Max time jump force can be applied
        public float gravityMultiplier = 3f;     // Stronger gravity for falling
        public float lowJumpMultiplier = 2f;     // Extra gravity for short jumps
        private float jumpTime;                  // Timer for jump force application
        private bool isJumping;
        private bool minJumpAchieved;            // Ensure minimum jump time is reached before falling

        public virtual void EnterState(PlayerController player) { }

        public virtual void UpdateState(PlayerController player)
        {
            HandleMovement(player);
            HandleJump(player);
        }

        public virtual void ExitState(PlayerController player) { }

        protected virtual void HandleMovement(PlayerController player)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward = cameraForward.normalized;
            cameraRight = cameraRight.normalized;

            Vector3 movement = cameraRight * moveHorizontal + cameraForward * moveVertical;

            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }

            Vector3 moveDirection = movement * player.moveSpeed;

            if (movement == Vector3.zero && player.IsGrounded)
            {
                player.playerRigidbody.velocity = Vector3.Lerp(player.playerRigidbody.velocity, Vector3.zero, 8f * Time.deltaTime);
            }
            else
            {
                player.playerRigidbody.velocity = new Vector3(moveDirection.x, player.playerRigidbody.velocity.y, moveDirection.z);
            }

            if (movement != Vector3.zero)
            {
                player.moveDirection = movement.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(player.moveDirection);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }

        // Refined jump logic for minimum jump time, faster fall, and time limit
        protected virtual void HandleJump(PlayerController player)
        {
            if (Input.GetButtonDown("Jump") && player.IsGrounded)
            {
                StartJump(player);
            }

            if (isJumping)
            {
                if (jumpTime < maxJumpTime)
                {
                    ContinueJump(player);
                }
                else
                {
                    EndJump(player); // Stop applying upward force after maxJumpTime or if button is not pressed
                }
            }

            ApplyDynamicGravity(player); // Ensure smooth gravity during fall
        }

        private void StartJump(PlayerController player)
        {
            player.playerRigidbody.velocity = new Vector3(player.playerRigidbody.velocity.x, 0f, player.playerRigidbody.velocity.z);
            isJumping = true;
            jumpTime = 0f;  // Reset jump timer
            minJumpAchieved = false;  // Ensure minimum jump time is not yet achieved
            player.IsGrounded = false;
        }

        private void ContinueJump(PlayerController player)
        {
            jumpTime += Time.deltaTime;

            // Apply upward force as long as the jumpTime is below maxUpwardVelocity
            if (player.playerRigidbody.velocity.y < maxUpwardVelocity)
            {
                // Apply diminishing jump force over time (stronger at start, weaker toward end)
                float jumpForce = Mathf.Lerp(initialJumpForce, 0f, jumpTime / maxJumpTime);
                player.playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
            }

            // If minimum jump time hasn't been achieved, keep applying force regardless of button state
            if (jumpTime < minJumpTime)
            {
                return;  // Force continues to be applied until the minimum jump time is reached
            }

            // If the jump button is no longer pressed after the minimum jump time, stop jumping
            if (!Input.GetButton("Jump"))
            {
                EndJump(player);
            }
        }

        private void EndJump(PlayerController player)
        {
            if (player.playerRigidbody.velocity.y > 0)
            {
                // Stop applying upward force once the jump ends
                isJumping = false;
            }
        }

        private void ApplyDynamicGravity(PlayerController player)
        {
            if (!player.IsGrounded)
            {
                if (player.playerRigidbody.velocity.y < 0) // When falling
                {
                    // Apply stronger gravity for a faster fall
                    player.playerRigidbody.velocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.deltaTime;
                }
                else if (!Input.GetButton("Jump") && player.playerRigidbody.velocity.y > 0) // If short jump
                {
                    // Apply extra gravity to end the jump early when the button is released (short jump)
                    player.playerRigidbody.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                }
            }
        }
    }
}
