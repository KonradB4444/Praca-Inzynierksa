using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
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
