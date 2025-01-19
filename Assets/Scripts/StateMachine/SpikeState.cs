using System.Collections.Generic;
using UnityEngine;

public class SpikeState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates> { PlayerStates.Default };

    private PlayerMovement playerMovement;
    private InputManager inputManager;
    private SpikeTrigger currentSpikeTrigger;
    private float baseTime = 0.5f;
    private float timer; 
    private bool hasJumped = false;
    private bool goingForward = true;

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        playerMovement = player.GetComponent<PlayerMovement>();
        inputManager = player.GetComponent<InputManager>();

        inputManager.CanMove = false;
        inputManager.CanJump = true;

        playerMovement.SetCurrentVelocity(Vector3.zero);

        timer = baseTime;
        hasJumped = false;
        goingForward = true;

        Debug.Log("Entered Spike State. Timer started.");
    }

    public void SetCurrentSpikeTrigger(SpikeTrigger spikeTrigger)
    {
        currentSpikeTrigger = spikeTrigger;
        StickPlayerToSpike();
    }

    public override void UpdateState()
    {
        timer -= Time.deltaTime;

        Debug.Log($"timer: {timer}");

        if (timer > 0 && inputManager.GetJumpInputDown() && !hasJumped)
        {
            hasJumped = true;
            JumpToNextSpike();
        }

        if (timer <= 0 && !hasJumped)
        {
            Debug.Log("Timer expired. Returning to default state.");
            playerStateMachine.SwitchState(PlayerStates.Default);
        }
    }

    private void JumpToNextSpike()
    {
        if (currentSpikeTrigger != null)
        {
            if (currentSpikeTrigger.nextSpike != null && currentSpikeTrigger.prevSpike == null)
            {
                goingForward = true;
            }
            else if (currentSpikeTrigger.nextSpike == null && currentSpikeTrigger.prevSpike != null)
            {
                goingForward = false;
            }
        }

        if (goingForward && currentSpikeTrigger != null && currentSpikeTrigger.nextSpike != null)
        {
            Debug.Log($"Jumping to next spike at {currentSpikeTrigger.nextSpike.position}");

            playerMovement.StartSmoothMove(
                playerMovement.transform.position,
                currentSpikeTrigger.nextSpike.position,
                0.5f 
            );

            SpikeTrigger nextSpikeTrigger = currentSpikeTrigger.nextSpike.GetComponent<SpikeTrigger>();
            if (nextSpikeTrigger != null)
            {
                SetCurrentSpikeTrigger(nextSpikeTrigger);
            }
            else
            {
                Debug.LogError("No SpikeTrigger found on the next spike!");
                playerStateMachine.SwitchState(PlayerStates.Default);
            }
        }
        else if (!goingForward && currentSpikeTrigger != null && currentSpikeTrigger.prevSpike != null)
        {
            Debug.Log($"Jumping to next spike at {currentSpikeTrigger.prevSpike.position}");

            playerMovement.StartSmoothMove(
                playerMovement.transform.position,
                currentSpikeTrigger.prevSpike.position,
                0.5f
            );
            SpikeTrigger previousSpikeTrigger = currentSpikeTrigger.prevSpike.GetComponent<SpikeTrigger>();
            if (previousSpikeTrigger != null)
            {
                SetCurrentSpikeTrigger(previousSpikeTrigger);
            }
            else
            {
                Debug.LogError("No SpikeTrigger found on the next spike!");
                playerStateMachine.SwitchState(PlayerStates.Default);
            }
        }
        else
        {
            Debug.LogError("No next spike available!");
            playerStateMachine.SwitchState(PlayerStates.Default);
        }
    }


    private void StickPlayerToSpike()
    {
        if (currentSpikeTrigger != null)
        {
            playerMovement.characterController.enabled = false;
            playerMovement.transform.position = currentSpikeTrigger.transform.position;
            playerMovement.characterController.enabled = true;

            Debug.Log("Player stuck to spike.");
        }
    }

    public void ResetTimer()
    {
        timer = baseTime;
        hasJumped = false;
        Debug.Log("SpikeState timer reset.");
    }

    public override void ExitState()
    {
        inputManager.CanMove = true;
        inputManager.CanJump = true;

        Debug.Log("Exited Spike State.");
    }
}
