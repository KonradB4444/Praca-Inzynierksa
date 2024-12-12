using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeTrigger : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private float slideSpeed = 7f;
    [SerializeField] private PlayerStateMachine playerStateMachine;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private InputManager inputManager;

    private bool isSliding;

    private void Update()
    {
        if (isSliding && playerStateMachine.currentStateEnum == PlayerStates.Default)
        {
            Vector3 slideDirection = orientation.forward;
            Vector3 slideVelocity = slideDirection * slideSpeed;

            playerMovement.MoveAlongSlide(slideVelocity);

            if (inputManager.GetJumpInput())
            {
                isSliding = false;
                playerMovement.ResetMovementSpeed();
            }
        }
        else if (playerStateMachine.currentStateEnum == PlayerStates.Crushed)
        {
            playerMovement.SetCurrentVelocity(Vector3.zero);

            if (inputManager.GetJumpInput())
            {
                isSliding = false;
                playerStateMachine.SwitchState(PlayerStates.Default);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            playerMovement = other.GetComponent<PlayerMovement>();
            isSliding = true;

            playerMovement.SetMovementSpeed(0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            isSliding = false;
            playerMovement.ResetMovementSpeed();
        }
    }
}
