using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] Camera portalCamera;

    [SerializeField] Camera playerCamera;

    private void Update()
    {
        float offsetX = portal.position.x - playerCamera.transform.position.x;
        float offsetZ = portal.position.z - playerCamera.transform.position.z;

        float angle = Mathf.Atan2(offsetZ, offsetX);

        portalCamera.transform.rotation = Quaternion.Euler(0, Mathf.Rad2Deg * -angle, 0);
    }
}
