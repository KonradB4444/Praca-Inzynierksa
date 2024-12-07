using UnityEngine;

namespace Player
{
    public class SlidingOnSlimeState : PlayerStateBase
    {
        private SlimeDecal currentSlime;
        private Vector3 direction;
        private float speed = 5f;

        public SlidingOnSlimeState(SlimeDecal slime)
        {
            currentSlime = slime;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            player.playerRigidbody.isKinematic = true;  // Disable physics while sliding
            CenterPlayerOnSlime(player);
            direction = (currentSlime.endPoint - currentSlime.startPoint).normalized;
        }

        public override void UpdateState(PlayerController player)
        {
            base.UpdateState(player);
            SlideAlongSlime(player);

            // Check if the player reaches the end of the slime
            if (Vector3.Distance(player.transform.position, currentSlime.endPoint) < 0.1f)
            {
                ExitSlime(player);
            }
        }

        private void SlideAlongSlime(PlayerController player)
        {
            player.transform.position += direction * speed * Time.deltaTime;
        }

        private void CenterPlayerOnSlime(PlayerController player)
        {
            Vector3 slimeCenter = (currentSlime.startPoint + currentSlime.endPoint) / 2;
            player.transform.position = new Vector3(slimeCenter.x, player.transform.position.y, slimeCenter.z);
        }

        private void ExitSlime(PlayerController player)
        {
            // Return control to the player after the slide finishes
            player.playerRigidbody.isKinematic = false;
            player.SetState(new NormalState());
        }
    }
}
