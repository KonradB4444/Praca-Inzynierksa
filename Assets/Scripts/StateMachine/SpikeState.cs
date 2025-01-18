using System.Collections.Generic;
using UnityEngine;

public class SpikeState : PlayerBaseState
{
    public override HashSet<PlayerStates> AllowedTransitions { get; } =
        new HashSet<PlayerStates>
        {
            PlayerStates.Default
        };

    private float launchForce = 10f; // Adjustable force for launching
    private float spikeDetectionRadius = 5000f; // Radius to detect nearest spikes
    private float stateDuration = 2f; // Time window to press jump
    private float currentTime;

    private PlayerMovement playerMovement;
    private Transform currentSpike;
    private HashSet<Transform> visitedSpikes = new HashSet<Transform>();

    public override void EnterState(PlayerStateMachine player)
    {
        base.EnterState(player);
        playerMovement = player.GetComponent<PlayerMovement>();
        Debug.Log("Entered SpikeState.");

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement is null in SpikeState!");
            return;
        }

        currentTime = stateDuration;

        // Ensure currentSpike is assigned by SpikeTrigger
        if (currentSpike == null)
        {
            Debug.LogWarning("No currentSpike assigned! Transitioning to DefaultState.");
            playerStateMachine.SwitchState(PlayerStates.Default);
            return;
        }

        // Snap player to the current spike
        playerMovement.SetCurrentVelocity(Vector3.zero); // Stop movement
        player.transform.position = currentSpike.position; // Snap to spike
        visitedSpikes.Add(currentSpike);
        Debug.Log($"Player stuck to spike: {currentSpike.position}");
    }

    public override void UpdateState()
    {
        currentTime -= Time.deltaTime;

        Debug.Log($"current time: {currentTime}");
        Debug.Log($"current spike: {currentSpike}");
        Debug.Log($"visited spikes: {visitedSpikes}");

        playerMovement.HandleMovement();
        playerMovement.ApplyCharacterMove();

        // If the player doesn't press jump in time, return to Default State
        if (currentTime <= 0f)
        {
            playerStateMachine.SwitchState(PlayerStates.Default);
            return;
        }

        // Check for jump input
        if (playerMovement.inputManager.GetJumpInputDown())
        {
            Vector3 targetSpike = FindNearestSpike();
            if (targetSpike != Vector3.zero)
            {
                LaunchToSpike(targetSpike);
            }
            else
            {
                playerStateMachine.SwitchState(PlayerStates.Default);
            }
        }
    }

    private Vector3 FindNearestSpike()
    {
        Collider[] spikeColliders = Physics.OverlapSphere(playerMovement.transform.position, spikeDetectionRadius);
        Collider nearestSpike = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider spike in spikeColliders)
        {
            if (!spike.CompareTag("Spikes") || visitedSpikes.Contains(spike.transform)) continue;

            float distance = Vector3.Distance(playerMovement.transform.position, spike.transform.position);
            if (distance < nearestDistance)
            {
                nearestSpike = spike;
                nearestDistance = distance;
            }
        }

        return nearestSpike != null ? nearestSpike.transform.position : Vector3.zero;
    }

    private void LaunchToSpike(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - playerMovement.transform.position).normalized;
        Vector3 launchVelocity = direction * launchForce;

        // Use PlayerMovement's method to set velocity
        playerMovement.SetCurrentVelocity(launchVelocity);

        Debug.Log($"Launching to {targetPosition} with velocity {launchVelocity}");

        currentSpike = FindSpikeTransform(targetPosition);
        if (currentSpike != null)
        {
            visitedSpikes.Add(currentSpike);
        }
    }

    private Transform FindSpikeTransform(Vector3 position)
    {
        Collider[] spikeColliders = Physics.OverlapSphere(position, 0.5f);
        foreach (Collider spike in spikeColliders)
        {
            if (spike.CompareTag("Spikes"))
            {
                return spike.transform;
            }
        }
        return null;
    }

    public void AssignCurrentSpike(Transform spikeTransform)
    {
        if (spikeTransform == null || visitedSpikes.Contains(spikeTransform)) return;

        currentSpike = spikeTransform;
        visitedSpikes.Add(currentSpike); // Mark the spike as visited
        Debug.Log($"Assigned currentSpike: {currentSpike.position}");
    }

    public override void ExitState()
    {
        base.ExitState();
        currentSpike = null; // Clear the current spike reference
    }
}
