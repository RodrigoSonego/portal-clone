using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform portal;
    [Space]
    [SerializeField] Player player;
    [Space]
    [SerializeField] Portal linkedPortal;
    [SerializeField] public Transform portalCamPivot;
    [SerializeField] Camera portalCamera;

    [SerializeField] Collider portalWallCollider;

    [SerializeField] float minDistanceToTeleport;

    Camera playerCamera;

    Vector3 portalCamPivotStartingPos;
    Vector3 portalCamPivotStartingRot;
    Vector3 portalCameraStartingRot;

    bool hasObjectInteracting = false;
    bool isReceivingTeleport = false;
    public bool ReceivingTeleport { get { return isReceivingTeleport; } set { isReceivingTeleport = value; } }

    private void Awake()
    {
        if(linkedPortal == null) { return; }

        portalCamPivotStartingPos = portalCamPivot.position;
        portalCamPivotStartingRot = portalCamera.transform.rotation.eulerAngles;
        
        portalCameraStartingRot = portalCamera.transform.localRotation.eulerAngles;

        playerCamera = Camera.main;

        //TODO: Set camera output texture here to avoid editor confusion
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
        float distanceZ = player.transform.position.z - linkedPortal.transform.position.z;
        float distanceX = player.transform.position.x - linkedPortal.transform.position.x;

        //Vector3 forward = Vector3.Scale(player.transform.position, portal.forward) - Vector3.Scale(portal.position, portal.forward);
        //Vector3 right = Vector3.Scale(player.transform.position, portal.right) - Vector3.Scale(portal.position, portal.right);

        //TODO: too sloppy, find another way, maybe using the sacele method up there /\
        float angle = Vector3.SignedAngle(portal.forward, linkedPortal.transform.forward, Vector3.up);

        Vector3 offsetZ = portal.transform.forward * (angle >= 90 ? distanceX : distanceZ);
        Vector3 offsetX = portal.transform.right * (angle >= 90 ? distanceZ : distanceX);

        portalCamPivot.transform.position = portalCamPivotStartingPos - offsetZ - offsetX;
    }

    private void RotateCameraWithPlayer()
    {
        float angle = Vector3.SignedAngle(linkedPortal.transform.forward, player.transform.forward, Vector3.up);

        portalCamPivot.rotation = Quaternion.Euler(0, portalCamPivotStartingRot.y + angle, 0);

        portalCamera.transform.localRotation = Quaternion.Euler(portalCameraStartingRot.x + playerCamera.transform.localRotation.eulerAngles.x, portalCamera.transform.localRotation.y, 0);
    }

    private void HandlePortalInteraction()
    {
        if (hasObjectInteracting == false || isReceivingTeleport) { return; }

        float distToPortal = Vector3.Distance(Vector3.Scale(portal.transform.forward, player.transform.position), Vector3.Scale(portal.position, portal.transform.forward));

        print("distance to portal: " + distToPortal);

        if (distToPortal <= minDistanceToTeleport)
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        player.transform.position = linkedPortal.portalCamPivot.transform.position;
        player.transform.localRotation = linkedPortal.portalCamPivot.rotation;

        linkedPortal.ToggleWallCollision(willEnable: false);
        linkedPortal.isReceivingTeleport = true; 
    }

    public void ToggleWallCollision(bool willEnable)
    {
        if (portalWallCollider == null) { print($"portal {gameObject.name} sem parede"); return; }

        portalWallCollider.enabled = willEnable;
    }

    //TODO: Método pra trackear o collider que ta interagindo com o portal (futuramente ter uma lista de colliders)
    public void OnPlayerEnterPortal()
    {
        hasObjectInteracting = true;
    }

    public void OnPlayerExitPortal() {
        hasObjectInteracting = false;

        isReceivingTeleport = false;
    }
}
