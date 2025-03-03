using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PortalController : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] Transform targetPortal;
    [SerializeField] Camera portalCamera;
    [SerializeField] Transform portalCamPivot;
    [SerializeField] float maxCameraDistance = 2;

    Vector3 portalCamPivotStartingPos;
    Vector3 portalCamPivotStartingRot;
    Vector3 portalCameraStartingRot;

    [SerializeField] MeshRenderer portalMesh;

    [SerializeField] Camera playerCamera;
    [SerializeField] Player player;

    private void Start()
    {
        portalCamPivotStartingPos = portalCamPivot.position;
        portalCamPivotStartingRot = portalCamera.transform.rotation.eulerAngles;
        
        portalCameraStartingRot = portalCamera.transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        float distanceZ = Mathf.Abs(player.transform.position.z - portal.position.z);
        float distanceX = player.transform.position.x - portal.position.x;

        Vector3 offsetZ = targetPortal.transform.forward * -1 * distanceZ * 0.7f;
        Vector3 offsetX = targetPortal.transform.right * -1 * distanceX;

        portalCamPivot.transform.position = portalCamPivotStartingPos - offsetZ + offsetX;

        RotateCameraWithPlayer();
    }

    private void RotateCameraWithPlayer()
    {
        portalCamPivot.localRotation = Quaternion.Euler(0, portalCamPivotStartingRot.y + player.transform.localRotation.eulerAngles.y, 0);

        portalCamera.transform.localRotation = Quaternion.Euler(portalCameraStartingRot.x + playerCamera.transform.localRotation.eulerAngles.x, portalCamera.transform.localRotation.y, 0);
    }
}
