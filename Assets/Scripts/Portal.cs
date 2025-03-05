using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] MeshRenderer portalMesh;
    [Space]
    [SerializeField] Player player;
    [Space]
    [SerializeField] Portal linkedPortal;
    [SerializeField] Transform portalCamPivot;
    [SerializeField] Camera portalCamera;

    [SerializeField] Collider portalWallCollider;

    [SerializeField] float minDistance;

    Camera playerCamera;

    Vector3 portalCamPivotStartingPos;
    Vector3 portalCamPivotStartingRot;
    Vector3 portalCameraStartingRot;

    Collider playerCollider;

    bool hasObjectInteracting = false;

    private void Start()
    {
        if(linkedPortal == null) { return; }

        portalCamPivotStartingPos = portalCamPivot.position;
        portalCamPivotStartingRot = portalCamera.transform.rotation.eulerAngles;
        
        portalCameraStartingRot = portalCamera.transform.localRotation.eulerAngles;

        playerCamera = Camera.main;

        playerCollider = player.GetComponentInChildren<Collider>();

        //playerCamera.targetTexture = portalMesh.material.GetTexture("_MainTex");
    }

    private void Update()
    {
        if (linkedPortal == null) { return; }

        MovePortalCamRelativeToPlayer();

        RotateCameraWithPlayer();
    }

    private void LateUpdate()
    {
        HandlePortalInteraction();
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

    private void HandlePortalInteraction()
    {
        if (hasObjectInteracting == false) { return; }

        float distToPortal = Vector3.Distance(Vector3.Scale(portal.transform.forward, player.transform.position), Vector3.Scale(portal.position, portal.transform.forward));

        print("distance to portal: " + distToPortal);

        if (distToPortal <= minDistance)
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        player.transform.position = portalCamPivot.transform.position;
        player.transform.localRotation = portalCamPivot.rotation;
        
        print($"Portal -Front: {-linkedPortal.transform.forward}, Player Front: {player.transform.forward}");

        //float offsetX = -linkedPortal.transform.forward.z - player.transform.forward.z;
        //float offsetZ = -linkedPortal.transform.right.x - player.transform.right.x;

        //float angle = Vector3.Angle(-linkedPortal.transform.forward, player.transform.forward);

        //player.transform.localRotation = Quaternion.Euler(0, -angle, 0);

    }

    public void ToggleWallCollision(bool willEnable)
    {
        if (portalWallCollider == null) { print($"portal {gameObject.name} sem parede"); return; }

        portalWallCollider.enabled = willEnable;
    }

    //Método pra trackear o collider que ta interagindo com o portal (futuramente ter uma lista de colliders)

    public void OnPlayerEnterPortal()
    {
        hasObjectInteracting = true;
    }

    public void OnPlayerExitPortal() { hasObjectInteracting = false; }
}
