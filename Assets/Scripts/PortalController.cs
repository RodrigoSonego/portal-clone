using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] Camera portalCamera;

    [SerializeField] Camera playerCamera;

    private void Update()
    {
        float angle = Vector3.Angle(playerCamera.transform.position, portal.position);

        print(angle);

        
        //portalCamera.transform.rotation = Quaternion.Euler(0, result.y, 0);
    }
}
