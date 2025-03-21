using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Portal : MonoBehaviour
{
    [Header("Portal Refences")]
    [SerializeField] Transform portal;
    [SerializeField] MeshRenderer portalMesh;
    [SerializeField] Portal linkedPortal;
    [SerializeField] Collider portalWallCollider;
    [Space]
    [SerializeField] Player player;
    [Header("Portal Camera Config")]
    [SerializeField] Transform portalCamPivot;
    [SerializeField] Camera portalCamera;
    [SerializeField] float minDotForObliqueProj;

    Camera playerCamera;
    Vector3 portalCamPivotStartingRot;
    Vector3 portalCameraStartingRot;

    List<PortalTraveler> trackedTravelers;

    private void Awake()
    {
        if (linkedPortal == null) { return; }

        portalCamPivotStartingRot = portalCamera.transform.rotation.eulerAngles;

        portalCameraStartingRot = portalCamera.transform.localRotation.eulerAngles;

        playerCamera = Camera.main;

        CreateRenderTexture();

        RenderPipelineManager.beginContextRendering += (a, b) => DoStuff();

        trackedTravelers = new List<PortalTraveler>();
    }

    public void DoStuff()
    {
        if (linkedPortal == null) { return; }

        MovePortalCamRelativeToPlayer();

        RotateCameraWithPlayer();

        CreateObliqueProjection();

        PreventPortalClip();
    }
    private void LateUpdate()
    {
        HandlePortalInteraction();
    }

    private void CreateRenderTexture()
    {
        RenderTexture tex = new RenderTexture(Screen.width, Screen.height, 32);
        portalCamera.targetTexture = tex;
        linkedPortal.portalMesh.material.SetTexture("_MainTex", tex);
    }

    // Sets a Oblique projection to coincide the near plane with the portal plane
    // Further Reference:   https://www.terathon.com/lengyel/Lengyel-Oblique.pdf
    //                      https://danielilett.com/2019-12-18-tut4-3-matrix-matching/
    private void CreateObliqueProjection()
    {
        // Could also use distance here, using portal.InverseTransformPoint(player.transform.position) and scaling by portal.forward
        float dot = Vector3.Dot(linkedPortal.transform.forward, (linkedPortal.transform.position - player.transform.position));

        if (dot <= minDotForObliqueProj)
        {
            portalCamera.projectionMatrix = playerCamera.projectionMatrix;
            return;
        }

        //Creates a plane at the portal position and set the normal
        Plane plane = new Plane(-portal.forward, portal.position);
        Vector4 clipPlane = new(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

        //Transforms clip plane to camera space (need to transpose the inverse matrix because dealing with normal vector)
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlane;

        portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private void MovePortalCamRelativeToPlayer()
    {
        Vector3 playerInPortalSpace = linkedPortal.transform.InverseTransformPoint(player.transform.position);
        // Only reversing X and Z since we want the camera to be behind the portal, but at the same height
        portalCamPivot.transform.localPosition = new(-playerInPortalSpace.x, playerInPortalSpace.y, -playerInPortalSpace.z);
    }

    private void RotateCameraWithPlayer()
    {
        float angle = Vector3.SignedAngle(linkedPortal.transform.forward, player.transform.forward, Vector3.up);

        portalCamPivot.rotation = Quaternion.Euler(0, portalCamPivotStartingRot.y + angle, 0);

        portalCamera.transform.localRotation = Quaternion.Euler(portalCameraStartingRot.x + playerCamera.transform.localRotation.eulerAngles.x, portalCamera.transform.localRotation.y, 0);
    }

    private void HandlePortalInteraction()
    {
        foreach (var traveler in trackedTravelers)
        {
            float travelerDot = Vector3.Dot(portal.transform.forward, portal.transform.position - traveler.transform.position);
            if(traveler.PreviousDot == 0) { traveler.PreviousDot = travelerDot; }
            
            print($"{name} currentDot: {travelerDot} old dot: {traveler.PreviousDot}");
            print($"{name} currentDot: {Mathf.Sign(travelerDot)} old dot: {Mathf.Sign(traveler.PreviousDot)}");
            
            if (Mathf.Sign(travelerDot) != Math.Sign(traveler.PreviousDot))
            {
                TeleportTraveler(traveler);
                trackedTravelers.Remove(traveler);
                return;
            }

            traveler.PreviousDot = travelerDot;
        }
    }

    // TODO: only work for teleporting player
    private void TeleportTraveler(PortalTraveler traveler)
    {
        // Teleport to portalCamPivot since it will have the same offset as the player
        Vector3 relativePos = portal.InverseTransformPoint(player.transform.position);
        relativePos.x *= -1;
        relativePos.z *= -1;
        
        Vector3 worldPos = linkedPortal.transform.TransformPoint(relativePos);
        
        traveler.Teleport(worldPos, linkedPortal.portalCamPivot.rotation);
        
        traveler.PreviousDot = Vector3.Dot(linkedPortal.transform.forward, linkedPortal.transform.position - traveler.transform.position);
        
        linkedPortal.ToggleWallCollision(willEnable: false);
        
        
    }

    public void ToggleWallCollision(bool willEnable)
    {
        if (portalWallCollider == null) { print($"portal {gameObject.name} sem parede"); return; }

        portalWallCollider.enabled = willEnable;
    }

    //  Sets the MeshRenderer scale so that's just enough to not be clipped by the near plane
    // also moves the portal to the back of the player when dot is negative
    //  Useful to hide the portal from the player camera when he teleports
    void PreventPortalClip()
    {
        float halfHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        var halfWidth = halfHeight * playerCamera.aspect;

        float distToClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCamera.nearClipPlane).magnitude;

        bool isPlayerInFrontOfPortal = Vector3.Dot(portal.transform.forward, (portal.transform.position - player.transform.position)) > 0;

        portalMesh.transform.localScale = new Vector3(portalMesh.transform.localScale.x, portalMesh.transform.localScale.y, distToClipPlaneCorner);
        portalMesh.transform.localPosition = Vector3.forward * distToClipPlaneCorner * 1.5f;// (isPlayerInFrontOfPortal ? 1.5f : -1.5f);

        //portalWallCollider.GetComponent<MeshRenderer>().enabled = Vector3.Dot(portal.transform.forward, (portal.transform.position - player.transform.position)) > 0.01f;

        //print(name + ": " + Vector3.Dot(portal.transform.forward, (portal.transform.position - player.transform.position)));
    }

    //TODO: M�todo pra trackear o collider que ta interagindo com o portal (futuramente ter uma lista de colliders)
    public void OnObjectEnterPortal(PortalTraveler traveler)
    {
        if (trackedTravelers.Contains(traveler)) { return; }

        trackedTravelers.Add(traveler);
    }

    public void OnPlayerExitPortal(PortalTraveler traveler) {
        if (trackedTravelers.Contains(traveler))
        {
            trackedTravelers.Remove(traveler);
        }
    }
}
