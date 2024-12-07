using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        public Transform target;  // Reference to the player's transform
        public float distance = 7.0f;  // Distance between the camera and the player
        public float height = 3.0f;  // Height above the player
        public float smoothSpeed = 0.125f;  // Smooth movement speed of the camera
        public float rotationSmoothSpeed = 5.0f;  // Smooth rotation speed of the camera
        public float maxVerticalAngle = 45.0f; // Maximum angle to look up or down at the player

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
            // Calculate the camera's target position
            Vector3 targetDirection = target.forward;
            targetDirection.y = 0; // Keep only the horizontal component of the player's forward direction
            targetDirection = targetDirection.normalized;

            // Calculate the target position
            targetPosition = target.position - targetDirection * distance + Vector3.up * height;

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

            HandleBackwardMovement(lookDirection);
        }



        // TODO: fix this shit
        private void HandleBackwardMovement(Vector3 lookDirection)
        {
            // Check if the player is moving toward the camera (using the "S" key)
            if (Input.GetAxis("Vertical") < 0)
            {
                // Adjust the camera's position if the player is moving toward it
                Vector3 cameraBackward = -transform.forward; // The backward direction relative to the camera
                Vector3 newTargetPosition = target.position + cameraBackward * distance + Vector3.up * height;

                // Smoothly move the camera to the new position
                transform.position = Vector3.SmoothDamp(transform.position, newTargetPosition, ref currentVelocity, smoothSpeed);

                // Rotate the camera to stay focused on the player
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), rotationSmoothSpeed * Time.deltaTime);
            }
        }
    }
}
