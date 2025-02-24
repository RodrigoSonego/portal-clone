using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] Camera portalCamera;
    Vector3 portalCamStartingPos;
    [SerializeField] float maxCameraDistance = 3;

    [SerializeField] Camera playerCamera;

    private void Start()
    {
        portalCamStartingPos = portalCamera.transform.position;
    }

    private void Update()
    {
        RotateCameraWithPlayer();

        float distance = Vector3.Distance(playerCamera.transform.position, portal.transform.position);
        distance = Mathf.Clamp(distance, 0, maxCameraDistance);

        Vector3 offset = portalCamera.transform.forward * distance;
        portalCamera.transform.position = portalCamStartingPos - offset;
    }

    private void RotateCameraWithPlayer()
    {
        float offsetX = portal.position.x - playerCamera.transform.position.x;
        float offsetZ = portal.position.z - playerCamera.transform.position.z;

        float angle = Mathf.Atan2(offsetZ, offsetX);

        portalCamera.transform.rotation = Quaternion.Euler(0, Mathf.Rad2Deg * -angle, 0);
    }
}
