using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PortalPlacer : MonoBehaviour
{
    [SerializeField] private Portal portal1;
    [SerializeField] private Portal portal2;

    [SerializeField] private float offsetFromWall = 0.06f;
    
    // TODO: Name this decently
    [SerializeField] LayerMask layerMask;
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

        FixPortalPositioning(portal, portal.transform.position);
        
        portal.isPlaced = true;
    }

    Vector3 FixPortalPositioning(Portal portal, Vector3 position)
    {
        Vector3[] testDirections =
        {
            portal.transform.up,
            -portal.transform.up,
            portal.transform.right,
            -portal.transform.right
        };

        float[] testDistances = { 1.2f, 1.2f, 0.9f, 0.9f};

        for (int i = 0; i < testDirections.Length; i++)
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(position, testDirections[i], out hit, testDistances[i], layerMask);
            if (hasHit)
            {
                // Debug.DrawRay(position, testDirections[i] * testDistances[i], Color.red, 5f);
                float offset = testDistances[i] - hit.distance;
                Vector3 newOffset = -offset * testDirections[i];
                portal.transform.Translate(newOffset, Space.World);
                // for (int j = 0; j < 4; j++)
                // {
                //     Debug.DrawLine(newOffset, testDirections[j] * testDistances[j], Color.yellow, 5f);
                // }
            }

        }
        
            
        
        return new Vector3();
    }
}
