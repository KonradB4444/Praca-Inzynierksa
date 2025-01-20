using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeTrigger : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private float slideSpeed = 7f;
    [SerializeField] private PlayerStateMachine playerStateMachine;
    private PlayerMovement playerMovement;

    private bool isSliding;

    private Vector3 slideDirection;
    private Vector3 slideVelocity;

    
    private void OnTriggerStay(Collider other)
    {
        if (isSliding && playerStateMachine.currentStateEnum == PlayerStates.Default)
        {
            //slideDirection = orientation.forward;
            slideVelocity = slideDirection * slideSpeed * 2f;

            Debug.Log($"Slide Direction: {slideDirection}");
            Debug.Log($"Slide Velocity: {slideVelocity}");

            playerMovement.MoveAlongSlide(slideVelocity);
            playerMovement.ApplyCharacterMove();

            if (playerMovement.inputManager.GetJumpInput())
            {
                playerMovement.StartDelayIsMidAir();
                playerMovement.jumpAngle = slideDirection;
                playerMovement.moveSpeedMidAir = slideSpeed * 2f;
                
                isSliding = false;
                playerMovement.Jump(0f, 1f);
            }
        }
        else if (playerStateMachine.currentStateEnum == PlayerStates.Crushed)
        {
            playerMovement.SetCurrentVelocity(Vector3.zero);

            if (playerMovement.inputManager.GetJumpInput())
            {
                isSliding = false;
                if(playerStateMachine.currentStateEnum != PlayerStates.Crushed) playerStateMachine.SwitchState(PlayerStates.Default);
                playerMovement.Jump(0f, 1f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            playerMovement = other.GetComponent<PlayerMovement>();

            playerMovement = playerStateMachine.GetComponent<PlayerMovement>();

            Vector3 playerVelocity = playerMovement.GetCurrentVelocity();

            Debug.Log($"Player Velocity: {playerVelocity}");

            if (playerVelocity.x == 0f && playerVelocity.z == 0f)
            {
                slideDirection = orientation.forward;
            }
            else
            {
                slideDirection = playerVelocity.normalized;
            }

            isSliding = true;
        }

        if (other.CompareTag("Player"))
        {
            PlayerStateMachine playerStateMachine = other.GetComponent<PlayerStateMachine>();
            if (playerStateMachine != null && playerStateMachine.currentStateEnum == PlayerStates.Crushed)
            {
                CrushedState crushedState = playerStateMachine.GetCurrentState() as CrushedState;
                if (crushedState != null)
                {
                    crushedState.SetInSlimeTrigger(true);
                    Debug.Log("Player entered slime trigger in Crushed State. Timer stopped.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            isSliding = false;
            playerMovement.ResetMovementSpeed();
            slideDirection = Vector3.zero;
            slideVelocity = Vector3.zero;
        }

        if (other.CompareTag("Player"))
        {
            PlayerStateMachine playerStateMachine = other.GetComponent<PlayerStateMachine>();
            if (playerStateMachine != null && playerStateMachine.currentStateEnum == PlayerStates.Crushed)
            {
                CrushedState crushedState = playerStateMachine.GetCurrentState() as CrushedState;
                if (crushedState != null)
                {
                    crushedState.SetInSlimeTrigger(false);
                    Debug.Log("Player exited slime trigger in Crushed State. Timer resumed.");
                }
            }
        }
    }

}
