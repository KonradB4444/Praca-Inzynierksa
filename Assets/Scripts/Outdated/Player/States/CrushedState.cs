using UnityEngine;

namespace Player
{
    public class CrushedState : PlayerStateBase
    {
        private SlimeDecal currentSlime;

        public CrushedState(SlimeDecal slime)
        {
            currentSlime = slime;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            player.playerRigidbody.isKinematic = true; // Disable physics while stuck
            StickToSlime(player);
        }

        public override void UpdateState(PlayerController player)
        {
            base.UpdateState(player);
            HandleJumpOffSlime(player);
        }

        private void StickToSlime(PlayerController player)
        {
            // Stick the player to the slime's surface
            Vector3 slimeCenter = (currentSlime.startPoint + currentSlime.endPoint) / 2;
            player.transform.position = slimeCenter;
        }

        private void HandleJumpOffSlime(PlayerController player)
        {
            if (Input.GetButtonDown("Jump"))
            {
                player.playerRigidbody.isKinematic = false;  // Re-enable physics
                player.playerRigidbody.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
                player.SetState(new NormalState()); // Return to normal state after jumping off
            }
        }
    }
}
