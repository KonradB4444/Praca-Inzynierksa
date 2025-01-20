using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine playerStateMachine;
    public float SurfaceY => GetComponent<Collider>().bounds.max.y; // water surface Y position

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out var playerMovement))
            {
                playerMovement.IsInWater = true;

                playerMovement.SetWaterSurfaceY(SurfaceY);

                Debug.Log("Player entered water.");
                if(playerStateMachine.currentStateEnum == PlayerStates.Iced)
                {
                    playerStateMachine.SwitchState(PlayerStates.Bubble);
                }
            }
            else Debug.Log(other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out var playerMovement))
            {
                playerMovement.IsInWater = false;
                playerMovement.SetWaterSurfaceY(0f); // resetting the value

                Debug.Log("Player exited water.");
            }
        }
    }
}
