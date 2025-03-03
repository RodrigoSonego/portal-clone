using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform portal;
    [Space]
    [SerializeField] Camera playerCamera;
    [SerializeField] Player player;
    [Space]
    [SerializeField] Portal linkedPortal;
    [SerializeField] Transform portalCamPivot;
    [SerializeField] Camera portalCamera;

    Vector3 portalCamPivotStartingPos;
    Vector3 portalCamPivotStartingRot;
    Vector3 portalCameraStartingRot;

    private void Start()
    {
        if(portalCamera == null) { return; }

        portalCamPivotStartingPos = portalCamPivot.position;
        portalCamPivotStartingRot = portalCamera.transform.rotation.eulerAngles;
        
        portalCameraStartingRot = portalCamera.transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        if (linkedPortal == null) { return; }

        MovePortalCamRelativeToPlayer();

        RotateCameraWithPlayer();
    }

    private void MovePortalCamRelativeToPlayer()
    {
        float distanceZ = Mathf.Abs(player.transform.position.z - portal.position.z);
        float distanceX = player.transform.position.x - portal.position.x;

        Vector3 offsetZ = linkedPortal.transform.forward * -1 * distanceZ * 0.7f;
        Vector3 offsetX = linkedPortal.transform.right * -1 * distanceX;

        portalCamPivot.transform.position = portalCamPivotStartingPos - offsetZ + offsetX;
    }

    private void RotateCameraWithPlayer()
    {
        portalCamPivot.rotation = Quaternion.Euler(0, portalCamPivotStartingRot.y + player.transform.localRotation.eulerAngles.y, 0);

        portalCamera.transform.localRotation = Quaternion.Euler(portalCameraStartingRot.x + playerCamera.transform.localRotation.eulerAngles.x, portalCamera.transform.localRotation.y, 0);
    }
}
