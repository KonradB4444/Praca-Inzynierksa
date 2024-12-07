using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private IPlayerState currentState;

        public Rigidbody playerRigidbody;
        public Camera playerCamera;
        public float moveSpeed = 10f;
        public float jumpForce = 5f;
        public float decelerationRate = 8f;  // New deceleration factor
        public bool IsGrounded = true;
        public Vector3 moveDirection { get; set; }

        public LayerMask collisionMask;

        private void Start()
        {
            SetState(new NormalState());
            playerRigidbody.freezeRotation = true;

            if (playerCamera == null)
            {
                CameraController cameraController = FindObjectOfType<CameraController>();
                if (cameraController != null)
                {
                    playerCamera = cameraController.GetComponent<Camera>();
                    cameraController.target = this.transform;
                }
                else
                {
                    Debug.LogError("CameraController not found in the scene!");
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Update current state logic
            currentState.UpdateState(this);

            // Update move direction based on velocity
            if (playerRigidbody.velocity.magnitude > 0.1f)
            {
                Vector3 horizontalVelocity = playerRigidbody.velocity;
                horizontalVelocity.y = 0;
                moveDirection = horizontalVelocity.normalized;
            }
        }

        public void SetState(IPlayerState newState)
        {
            if (currentState != null)
            {
                currentState.ExitState(this);
            }

            currentState = newState;
            currentState.EnterState(this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                IsGrounded = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                IsGrounded = false;
            }
        }
    }
}