using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    public Transform nextSpike;
    public Transform prevSpike;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"player entered spike trigger {other.transform.name}");
        PlayerStateMachine playerStateMachine = other.GetComponent<PlayerStateMachine>();
        if (playerStateMachine != null)
        {
            if (playerStateMachine.currentStateEnum == PlayerStates.Bubble)
            {
                playerStateMachine.SwitchState(PlayerStates.Spike);
                SpikeState spikeState = playerStateMachine.GetCurrentState() as SpikeState;
                spikeState.ResetTimer();
                if (spikeState != null)
                {
                    spikeState.SetCurrentSpikeTrigger(this);
                }
            }
            else if (playerStateMachine.currentStateEnum == PlayerStates.Spike)
            {
                SpikeState spikeState = playerStateMachine.GetCurrentState() as SpikeState;
                spikeState.ResetTimer();
            }
            else
            {
                playerStateMachine.SwitchState(PlayerStates.Hurt);
            }
        }
    }
}
