using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerStateMachine playerStateMachine = other.GetComponent<PlayerStateMachine>();
        if (playerStateMachine != null)
        {
            playerStateMachine.SwitchState(PlayerStates.Hurt);
            Debug.Log("Player hit spikes! Entering HurtState.");
        }
    }
}
