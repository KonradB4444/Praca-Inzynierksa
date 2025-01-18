using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeyserTrigger : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine playerStateMachine;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStateMachine.SwitchState(PlayerStates.Bubble);
        }
    }
}
