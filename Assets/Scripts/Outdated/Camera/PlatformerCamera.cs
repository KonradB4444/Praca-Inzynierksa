using UnityEngine;

namespace Player
{
    public class PlatformerCamera : MonoBehaviour
    {
        public Transform target;  // Reference to the player's transform
        public float distance = 7.0f;  // Distance between the camera and the player
        public float height = 3.0f;  // Height above the player
        public float smoothSpeed = 0.125f;  // Smooth movement speed of the camera
        public float rotationSmoothSpeed = 5.0f;  // Smooth rotation speed of the camera
        public float maxVerticalAngle = 45.0f; // Maximum angle to look up or down at the player
        public float sideOffset = 2.0f; // Offset to move the player slightly off-center when moving sideways
        public float backwardDistance = 3.0f;  // Extra distance when moving backward towards the camera

        private Vector3 currentVelocity;
        private Vector3 targetPosition;

        void LateUpdate()
        {
            if (target == null)
                return;

            HandleCameraPositionAndRotation();
        }

        private void HandleCameraPositionAndRotation()
        {
            // Calculate player movement direction
            Vector3 playerMovement = target.InverseTransformDirection(target.forward);

            // Offset the camera to the side when moving left or right
            float horizontalOffset = Input.GetAxis("Horizontal") * sideOffset;
            Vector3 offsetPosition = target.right * horizontalOffset;

            // Calculate the camera's target position
            Vector3 targetDirection = target.forward;
            targetDirection.y = 0; // Keep only the horizontal component of the player's forward direction
            targetDirection = targetDirection.normalized;

            // Apply the offset to the camera target position
            targetPosition = target.position - targetDirection * distance + Vector3.up * height + offsetPosition;

            // Smoothly move the camera to the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothSpeed);

            // Calculate the direction the camera should look towards
            Vector3 lookDirection = target.position - transform.position;
            float angleToPlayer = Vector3.Angle(Vector3.forward, lookDirection);

            // Limit the vertical angle of the camera to avoid unnatural looking angles
            if (lookDirection.y > 0) // Player is above the camera
            {
                float clampedY = Mathf.Clamp(lookDirection.y, 0, Mathf.Tan(maxVerticalAngle * Mathf.Deg2Rad) * lookDirection.magnitude);
                lookDirection.y = clampedY;
            }

            // Smoothly rotate the camera to look at the player
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);

            // Handle backward movement
            HandleBackwardMovement(playerMovement);
        }

        // Handles camera behavior when the player moves backward towards the camera
        private void HandleBackwardMovement(Vector3 playerMovement)
        {
            // Check if the player is moving toward the camera (using the "S" key or backward input)
            if (Input.GetAxis("Vertical") < 0)
            {
                // Adjust the camera's distance if the player is moving toward it
                Vector3 cameraBackward = -transform.forward; // The backward direction relative to the camera
                Vector3 newTargetPosition = target.position + cameraBackward * backwardDistance + Vector3.up * height;

                // Smoothly move the camera to the new position with a slight backward offset
                transform.position = Vector3.SmoothDamp(transform.position, newTargetPosition, ref currentVelocity, smoothSpeed);

                // Rotate the camera to stay focused on the player
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), rotationSmoothSpeed * Time.deltaTime);
            }
        }
    }
}
