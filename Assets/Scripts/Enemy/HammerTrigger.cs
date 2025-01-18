using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerTrigger : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private PlayerStateMachine playerStateMachine;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.isPlayerInHammerTrigger = true;
            playerStateMachine.SwitchState(PlayerStates.Springy);
            Debug.Log("Player entered hammer trigger.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.isPlayerInHammerTrigger = false;
            Debug.Log("Player exited hammer trigger.");
        }
    }
}
