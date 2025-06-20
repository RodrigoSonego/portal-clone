using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PortalPlacer : MonoBehaviour
{
    [SerializeField] private Portal portal1;
    [SerializeField] private Portal portal2;

    [SerializeField] private float offsetFromWall = 0.06f;
    
    void Start()
    {
        portal1.gameObject.SetActive(false);
        portal2.gameObject.SetActive(false);
        
        InputSystem.actions.FindAction("Mouse1").performed += (_) => TryToPlacePortal(portal1);
        InputSystem.actions.FindAction("Mouse2").performed += (_) => TryToPlacePortal(portal2);
    }
    
    private void TryToPlacePortal(Portal portal)
    {
        bool hasHit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit);

        if (hasHit && hit.collider.CompareTag("Wall"))
        {  
            // hit.normal is negative, since we want the portal to point toward the wall

            
            PositionAndRotatePortal(hit.point, hit.normal, portal);
            portal.SetWallCollider(hit.collider);
        }
    }

    void PositionAndRotatePortal(Vector3 position, Vector3 wallNormal, Portal portal)
    {
        Quaternion portalRotation = Quaternion.LookRotation(-wallNormal, Vector3.up);
        
        portal.transform.position = position + (wallNormal * offsetFromWall);
        portal.transform.rotation = portalRotation;
        portal.gameObject.SetActive(true);
        
        portal.isPlaced = true;
    }
}
