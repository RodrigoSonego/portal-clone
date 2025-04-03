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

        RenderPipelineManager.beginContextRendering += (a, b) => OnPreRender();

        trackedTravelers = new List<PortalTraveler>();
    }

    public void OnPreRender()
    {
        if (linkedPortal == null) { return; }

        MovePortalCamRelativeToPlayer();

        RotateCameraWithPlayer();

        CreateObliqueProjection();
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
        Vector3 playerInPortalSpace = GetRelativeWorldPosition(linkedPortal.transform, portal, player.transform);
        
        portalCamPivot.transform.position = playerInPortalSpace;
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

            GameObject clone = traveler.TravelerClone;
            
            Vector3 eulerRotation = traveler.transform.up * (linkedPortal.transform.eulerAngles.y -
                (portal.eulerAngles.y - traveler.transform.eulerAngles.y) + 180);
            
            clone.transform.position =
                GetRelativeWorldPosition(portal, linkedPortal.transform, traveler.transform);

            clone.transform.rotation = Quaternion.Euler(eulerRotation);
            
            if (Math.Sign(travelerDot) != Math.Sign(traveler.PreviousDot))
            {
                TeleportTraveler(traveler);
                trackedTravelers.Remove(traveler);
                
                linkedPortal.OnObjectEnterPortal(traveler);
                return;
            }

            traveler.PreviousDot = travelerDot;
        }
    }
    
    private void TeleportTraveler(PortalTraveler traveler)
    {
        // Teleport to portalCamPivot since it will have the same offset as the player
        Vector3 relativePos = GetRelativeWorldPosition(portal, linkedPortal.transform, traveler.transform);
        
        Vector3 eulerRotation = traveler.transform.up * (linkedPortal.transform.eulerAngles.y -
            (portal.eulerAngles.y - traveler.transform.eulerAngles.y) + 180);
        
        traveler.Teleport(relativePos, Quaternion.Euler(eulerRotation));
        
        traveler.PreviousDot = Vector3.Dot(linkedPortal.transform.forward, linkedPortal.transform.position - traveler.transform.position);
        
        linkedPortal.ToggleWallCollision(willEnable: false);
    }

    public void ToggleWallCollision(bool willEnable)
    {
        if (portalWallCollider is null) { print($"portal {gameObject.name} sem parede"); return; }

        portalWallCollider.enabled = willEnable;
    }
    
    public void OnObjectEnterPortal(PortalTraveler traveler)
    {
        if (trackedTravelers.Contains(traveler)) { return; }

        trackedTravelers.Add(traveler);
        traveler.TravelerClone.SetActive(true);
    }

    public void OnPlayerExitPortal(PortalTraveler traveler) {
        if (trackedTravelers.Contains(traveler))
        {
            trackedTravelers.Remove(traveler);
            traveler.TravelerClone.SetActive(false);
        }
    }

    /// <summary>
    /// Calculates the World position of subject relative to destination based on subject's relation to origin
    /// (i.e. relative position to portal)
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="subject"></param>
    /// <returns>World relative position of target to destination</returns>
    private Vector3 GetRelativeWorldPosition(Transform origin, Transform destination, Transform subject)
    {
        // Teleport to portalCamPivot since it will have the same offset as the player
        Vector3 relativePos = origin.InverseTransformPoint(subject.position);
        relativePos.x *= -1;
        relativePos.z *= -1;
        
        Vector3 worldPos = destination.TransformPoint(relativePos);
        return worldPos;
    }
}
