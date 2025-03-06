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
    [SerializeField] public Transform portalCamPivot;
    [SerializeField] Camera portalCamera;

    [SerializeField] Collider portalWallCollider;

    [SerializeField] float minDistance;

    Camera playerCamera;

    Vector3 portalCamPivotStartingPos;
    Vector3 portalCamPivotStartingRot;
    Vector3 portalCameraStartingRot;

    Vector3 camPivotForward;
    Vector3 camPivotRight;

    bool hasObjectInteracting = false;

    private void Awake()
    {
        if(linkedPortal == null) { return; }

        portalCamPivotStartingPos = portalCamPivot.position;
        portalCamPivotStartingRot = portalCamera.transform.rotation.eulerAngles;
        
        portalCameraStartingRot = portalCamera.transform.localRotation.eulerAngles;

        playerCamera = Camera.main;

        camPivotForward = portalCamPivot.forward;
        camPivotRight = portalCamPivot.right;

        //camPivotForward = linkedPortal.transform.forward * -1;
        //camPivotRight = linkedPortal.transform.right * -1;

        //print($"{name} pivot forward: {camPivotForward}, right: {camPivotRight}");
        //print($"{name} forward: {linkedPortal.transform.forward * -1}, right: {linkedPortal.transform.right * -1}");
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
        float distanceZ = player.transform.position.z - linkedPortal.transform.position.z;
        float distanceX = player.transform.position.x - linkedPortal.transform.position.x;

        //Vector3 forward = Vector3.Scale(player.transform.position, portal.forward) - Vector3.Scale(portal.position, portal.forward);
        //Vector3 right = Vector3.Scale(player.transform.position, portal.right) - Vector3.Scale(portal.position, portal.right);

        //float distanceZ = forward.magnitude;
        //float distanceX = right.magnitude;

        float angle = Vector3.SignedAngle(portal.forward, linkedPortal.transform.forward, Vector3.up);

        print($"{name} angle to other: {angle}");

        Vector3 offsetZ = portal.transform.forward * (angle >= 90 ? distanceX : distanceZ);
        Vector3 offsetX = portal.transform.right * (angle >= 90 ? distanceZ : distanceX);
        
        //Vector3 offsetZ = camPivotRight * distanceZ;
        //Vector3 offsetX = camPivotForward * distanceX;

        //print($"{gameObject.name} camera offset: x:{offsetX}, z:{offsetZ}");

        portalCamPivot.transform.position = portalCamPivotStartingPos - offsetZ - offsetX;

        //portalCamPivot.transform.position = portalCamPivotStartingPos - offset;
    }

    private void RotateCameraWithPlayer()
    {
        float angle = Vector3.SignedAngle(linkedPortal.transform.forward, player.transform.forward, Vector3.up);
        

        //portalCamPivot.rotation = Quaternion.Euler(0, -portalCamPivotStartingRot.y + angle, 0);
        portalCamPivot.rotation = Quaternion.Euler(0, portalCamPivotStartingRot.y + angle, 0);

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
        player.transform.position = linkedPortal.portalCamPivot.transform.position;
        player.transform.localRotation = linkedPortal.portalCamPivot.rotation;
        
        //print($"Portal -Front: {-linkedPortal.transform.forward}, Player Front: {player.transform.forward}");
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
