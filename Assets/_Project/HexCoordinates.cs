using UnityEngine;

public class HexCoordinates : MonoBehaviour
{
    public int q; // x-axis
    public int r; // y-axis
    public int s; // z-axis (redundant, but stored for convenience)

    void OnValidate()
    {
        // Ensure cube constraint: q + r + s = 0
        s = -q - r;
    }
}
