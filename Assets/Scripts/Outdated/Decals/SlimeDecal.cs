using UnityEngine;

public class SlimeDecal : MonoBehaviour
{
    public float length = 5f;  // Length of the slime decal
    public float width = 2f;   // Width of the slime decal

    // The position of the start and end points of the slime decal
    public Vector3 startPoint;
    public Vector3 endPoint;

    private void OnDrawGizmos()
    {
        // Visualize the slime decal in the editor (using gizmos)
        Gizmos.color = Color.green;
        Vector3 center = (startPoint + endPoint) / 2;
        Vector3 size = new Vector3(width, 0.1f, length);
        Gizmos.DrawWireCube(center, size);
    }

    // Initialize the slime decal with custom length, width, and start/end positions
    public void Initialize(Vector3 start, Vector3 end, float decalWidth, float decalLength)
    {
        startPoint = start;
        endPoint = end;
        width = decalWidth;
        length = decalLength;
    }

    // You could add more methods here, such as logic for collision detection
}
