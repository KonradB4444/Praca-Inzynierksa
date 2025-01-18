using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerStateMachine playerStateMachine = other.GetComponent<PlayerStateMachine>();
        if (playerStateMachine != null)
        {
            if (playerStateMachine.currentStateEnum == PlayerStates.Bubble)
            {
                // Transition to SpikeState and wait to assign currentSpike
                StartCoroutine(WaitForStateSwitch(playerStateMachine));
            }
            else
            {
                playerStateMachine.SwitchState(PlayerStates.Hurt);
                Debug.Log("Player hit spikes! Entering HurtState.");
            }
        }
    }

    private System.Collections.IEnumerator WaitForStateSwitch(PlayerStateMachine playerStateMachine)
    {
        // Switch to SpikeState
        playerStateMachine.SwitchState(PlayerStates.Spike);

        // Wait for one frame to ensure the state is fully switched
        yield return null;

        // Get the SpikeState instance and assign currentSpike
        SpikeState spikeState = playerStateMachine.GetCurrentState() as SpikeState;
        if (spikeState != null)
        {
            spikeState.AssignCurrentSpike(transform);
            Debug.Log($"Spike assigned to SpikeState: {transform.position}");
        }
        else
        {
            Debug.LogWarning("SpikeState not active after state switch!");
        }
    }
}
