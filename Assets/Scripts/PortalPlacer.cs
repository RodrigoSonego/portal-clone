using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PortalPlacer : MonoBehaviour
{
    [SerializeField] private Portal portal1;
    [SerializeField] private Portal portal2;

    [SerializeField] private float offsetFromWall = 0.06f;
    
    [Space]
    [SerializeField] private float verticalTestDistance = 1.2f;
    [SerializeField] private float horizontalTestDistance = 0.9f;
    [Space]
    [SerializeField] LayerMask positionCorrectionMask;
    
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
            PositionAndRotatePortal(hit.point, hit.normal, portal);
            portal.SetWallCollider(hit.collider);
        }
    }

    void PositionAndRotatePortal(Vector3 position, Vector3 wallNormal, Portal portal)
    {
        // hit.normal is negative, since we want the portal to point toward the wall
        Quaternion portalRotation = Quaternion.LookRotation(-wallNormal, Vector3.up);
        
        portal.transform.position = position + (wallNormal * offsetFromWall);
        portal.transform.rotation = portalRotation;
        portal.gameObject.SetActive(true);

        FixPositioningOnIntersection(portal);
        FixOverhangPositioning(portal);
        
        portal.isPlaced = true;
    }

    // Fix positioning on intersections of terrain to no not go through walls, ceiling or floor 
    void FixPositioningOnIntersection(Portal portal)
    {
        IntersectionTest[] tests =
        {
            new (portal.transform.up, verticalTestDistance),
            new (-portal.transform.up, verticalTestDistance),
            new (portal.transform.right, horizontalTestDistance),
            new (-portal.transform.right, horizontalTestDistance),
        };

        for (int i = 0; i < tests.Length; i++)
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(portal.transform.position, tests[i].Direction, out hit, tests[i].Distance, positionCorrectionMask);
            if (hasHit)
            {
                float offset = tests[i].Distance - hit.distance;
                Vector3 newOffset = -offset * tests[i].Direction;
                portal.transform.Translate(newOffset, Space.World);
            }

        }
    }
    
    // Fix positioning on edges/overhangs of terrain
    // Defines a point behind each up-most, right-most, down-most and left-most point in the quad
    // From there if it doesn't find a wall, shoots a ray on the opposite direction to find the distance to the
    //wall and reposition the portal
    void FixOverhangPositioning(Portal portal)
    {
        OverhangTest[] tests =
        {
            new (portal.transform.up, new Vector3(0, -1.2f, 0.1f)),
            new (-portal.transform.up, new Vector3(0, 1.2f, 0.1f)),
            new (portal.transform.right, new Vector3(-0.9f, 0, 0.1f)),
            new (-portal.transform.right, new Vector3(0.9f, 0, 0.1f)),
        };
        
        for (int i = 0; i < tests.Length; i++)
        {
            Vector3 rayPos = portal.transform.TransformPoint(tests[i].Point);

            if (Physics.CheckSphere(rayPos, 0.2f, positionCorrectionMask)) { continue; }
            
            if (Physics.Raycast(rayPos, tests[i].Direction, out RaycastHit hit, 2, positionCorrectionMask))
            {
                Vector3 offset = hit.point - rayPos;
                portal.transform.Translate(Vector3.Scale(-offset, tests[i].Direction), Space.World);
            }
        }
    }

    struct IntersectionTest
    {
        public Vector3 Direction;
        public float Distance;

        public IntersectionTest(Vector3 direction, float distance)
        {
            Direction = direction;
            Distance = distance;
        }
    }

    struct OverhangTest
    {
        public Vector3 Direction;
        public Vector3 Point;

        public OverhangTest(Vector3 direction, Vector3 point)
        {
            Direction = direction;
            Point = point;
        }
    }
}
