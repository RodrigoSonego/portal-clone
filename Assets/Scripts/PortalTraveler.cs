using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    float previousDot;

    public float PreviousDot { get { return previousDot; } set { previousDot = value; } }

    public void Teleport(Vector3 destination, Quaternion rotation)
    {
        transform.position = destination;
        transform.rotation = rotation;
    }
}
