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

    private Vector3 slideDirection;
    private Vector3 slideVelocity;

    
    private void OnTriggerStay(Collider other)
    {
        if (isSliding && playerStateMachine.currentStateEnum == PlayerStates.Default)
        {
            //slideDirection = orientation.forward;
            slideVelocity = slideDirection * slideSpeed;

            Debug.Log($"Slide Direction: {slideDirection}");
            Debug.Log($"Slide Velocity: {slideVelocity}");

            playerMovement.MoveAlongSlide(slideVelocity);
            playerMovement.ApplyCharacterMove();

            if (inputManager.GetJumpInput())
            {
                isSliding = false;
                //StartCoroutine(DisableTriggerTemporarily());
                playerMovement.Jump(0f, 1f);
                //StartCoroutine(DisableTriggerTemporarily());
                //playerMovement.ResetMovementSpeed();
            }
        }
        else if (playerStateMachine.currentStateEnum == PlayerStates.Crushed)
        {
            playerMovement.SetCurrentVelocity(Vector3.zero);

            if (inputManager.GetJumpInput())
            {
                isSliding = false;
                playerStateMachine.SwitchState(PlayerStates.Default);
                playerMovement.Jump(0f, 1f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            playerMovement = other.GetComponent<PlayerMovement>();

            Vector3 playerVelocity = playerMovement.GetCurrentVelocity();

            if (playerVelocity == Vector3.zero)
            {
                slideDirection = orientation.forward;
            }
            else
            {
                slideDirection = playerVelocity.normalized;
            }

            isSliding = true;

            playerMovement.SetMovementSpeed(0);
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
    private IEnumerator DisableTriggerTemporarily()
    {
        // Disable the trigger collider
        GetComponent<Collider>().enabled = false;

        Debug.Log("Trigger temporarily disabled.");

        // Wait for a short duration (adjust as needed)
        yield return new WaitForSeconds(0.5f);

        // Re-enable the trigger collider
        GetComponent<Collider>().enabled = true;

        Debug.Log("Trigger re-enabled.");
    }

}
