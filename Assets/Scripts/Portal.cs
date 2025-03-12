using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] Transform portalMesh;
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

        CreateObliqueProjection();
    }

    // Sets a Oblique projection to coincide the near plane with the portal plane
    // Further Reference:   https://www.terathon.com/lengyel/Lengyel-Oblique.pdf
    //                      https://danielilett.com/2019-12-18-tut4-3-matrix-matching/
    private void CreateObliqueProjection()
    {
        //Creates a plane at the portal position and set the normal
        Plane plane = new Plane(-portal.forward, portal.position);
        Vector4 clipPlane = new(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

        //Transforms clip plane to camera space (still didn't understand the need to transpose the inverse matrix)
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlane;

        portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private void LateUpdate()
    {
        PreventPortalClip();
        HandlePortalInteraction();
    }

    private void MovePortalCamRelativeToPlayer()
    {
        float distanceZ = player.transform.position.z - linkedPortal.transform.position.z;
        float distanceX = player.transform.position.x - linkedPortal.transform.position.x;

        //Vector3 forward = Vector3.Scale(player.transform.position, portal.forward) - Vector3.Scale(portal.position, portal.forward);
        //Vector3 right = Vector3.Scale(player.transform.position, portal.right) - Vector3.Scale(portal.position, portal.right);

        //TODO: too sloppy and gambiarra, find another way, maybe using the sacele method up there /\
        float angle = Vector3.SignedAngle(portal.forward, linkedPortal.transform.forward, Vector3.up);
        Vector3 offsetZ = portal.transform.forward * (angle >= 90 ? distanceX : distanceZ);
        Vector3 offsetX = portal.transform.right * (angle >= 90 ? -distanceZ : distanceX);

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
        if (hasObjectInteracting == false) { return; }

        var playerDot = Vector3.Dot(portal.transform.forward, (portal.transform.position - player.transform.position));

        if (playerDot < 0)
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        player.transform.position = linkedPortal.portalCamPivot.transform.position;
        player.transform.localRotation = linkedPortal.portalCamPivot.rotation;

        linkedPortal.ToggleWallCollision(willEnable: false);
    }

    public void ToggleWallCollision(bool willEnable)
    {
        if (portalWallCollider == null) { print($"portal {gameObject.name} sem parede"); return; }

        portalWallCollider.enabled = willEnable;
    }

    void PreventPortalClip()
    {
        float halfHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        var halfWidth = halfHeight * playerCamera.aspect;

        float distToClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCamera.nearClipPlane).magnitude;

        bool isPlayerInFrontOfPortal = Vector3.Dot(portal.transform.forward, (portal.transform.position - player.transform.position)) > 0;

        portalMesh.localScale = new Vector3(portalMesh.localScale.x, portalMesh.localScale.y, distToClipPlaneCorner);
        portalMesh.localPosition = portal.forward * distToClipPlaneCorner * (isPlayerInFrontOfPortal ? -0.3f : 0.3f);
    }

    //TODO: Método pra trackear o collider que ta interagindo com o portal (futuramente ter uma lista de colliders)
    public void OnPlayerEnterPortal()
    {
        hasObjectInteracting = true;
    }

    public void OnPlayerExitPortal() {
        hasObjectInteracting = false;
    }
}
