using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PortalController : MonoBehaviour
{
    [SerializeField] Transform portal;
    [SerializeField] Camera portalCamera;
    [SerializeField] Transform portalCamPivot;
    Vector3 portalCamStartingPos;
    Vector3 portalCamStartingRot;
    Vector3 portalPivotForward;
    Vector3 portalPivotRight;


    [SerializeField] MeshRenderer portalMesh;

    [SerializeField] Camera playerCamera;
    [SerializeField] Player player;

    Vector3[] portalScreenPoints = new Vector3[4];

    private void Start()
    {
        portalCamStartingPos = portalCamPivot.position;
        portalCamStartingRot = portalCamera.transform.rotation.eulerAngles;
        portalPivotForward = portalCamPivot.transform.forward;
        portalPivotRight = portalCamPivot.transform.right;
    }

    private void Update()
    {
        //float xDistance = Mathf.Abs(player.transform.position.x - portal.position.x);
        //float zDistance = Mathf.Abs(player.transform.position.z - portal.position.z);

        //Vector3 offset = (portalPivotForward * zDistance) + (portalPivotRight * xDistance);

        //print("portal cam pivot forward = " + portalCamPivot.forward);

        //portalCamPivot.position = portalCamStartingPos - offset;

        float distance = Vector3.Distance(playerCamera.transform.position, portal.transform.position);
        //distance = Mathf.Clamp(distance, 0, maxCameraDistance);

        //Vector3 offset = portalCamPivot.transform.forward * distance;
        //portalCamPivot.transform.position = portalCamStartingPos - offset;


        RotateCameraWithPlayer();

        Vector3[] portalVertices =
        {
            portalMesh.bounds.max,                                                          // Top-Right 
            portalMesh.bounds.min,                                                          // Bot-Left  
            new(portalMesh.bounds.min.x, portalMesh.bounds.max.y, portalMesh.bounds.min.z), // Top-Left 
            new(portalMesh.bounds.max.x, portalMesh.bounds.min.y, portalMesh.bounds.max.z)  // Bot-Right 
        };

        //Vector3[] portalScreenPoints = new Vector3[4];
        for (int i = 0; i < portalVertices.Length; i++)
        {
            //portalScreenPoints[i] = playerCamera.WorldToScreenPoint(portalVertices[i]);

            //print($"World  [{i}] {portalVertices[i]}");

            portalScreenPoints[i] = playerCamera.worldToCameraMatrix.MultiplyPoint(portalVertices[i]);
            print($"Screen [{i}] {portalScreenPoints[i]}");
        }

        //print("--------------------");

        float near = playerCamera.nearClipPlane;
        float far = playerCamera.farClipPlane;


        //// Normalize points to device coordinates
        //float top = (portalScreenPoints[0].y / Screen.height) * 2 - 1;
        //float bot = (portalScreenPoints[1].y / Screen.height) * 2 - 1;
        //float left = (portalScreenPoints[2].x / Screen.width) * 2 - 1;
        //float right = (portalScreenPoints[3].x / Screen.width) * 2 - 1;

        float top = portalScreenPoints[0].y  ;
        float bot = portalScreenPoints[1].y  ;
        float left = portalScreenPoints[2].x ;
        float right = portalScreenPoints[3].x;

        // Normalize to near plane
        left *= near;
        right *= near;
        bot *= near;
        top *= near;

        //print($"Top: {top}, Bot: {bot}, Left: {left}, Right: {right}");

        //Matrix4x4 frustum = Matrix4x4.Frustum(left, right, bot, top, near, far);
        Matrix4x4 frustum = Matrix4x4.Frustum(left, right, bot, top, near, far);

        portalCamera.projectionMatrix = frustum;
    }

    private void RotateCameraWithPlayer()
    {
        //float offsetX = portal.position.x - player.transform.position.x;
        //float offsetZ = portal.position.z - player.transform.position.z;

        //float angle = Mathf.Atan2(offsetZ, offsetX);

        //portalCamera.transform.rotation = Quaternion.Euler(0, Mathf.Rad2Deg * -angle, 0);

        //portalCamera.transform.rotation = Quaternion.Euler(new(portalCamStartingRot.x, portalCamStartingRot.y + player.transform.rotation.eulerAngles.y, portalCamStartingRot.z));

        //portalCamPivot.localRotation = Quaternion.Euler(portalPivotStartingRot.x, portalPivotStartingRot.y + player.transform.localRotation.eulerAngles.y, 0);
        //portalCamera.transform.localRotation = Quaternion.Euler(new(playerCamera.transform.localRotation.eulerAngles.x, portalCamera.transform.localRotation.y, 0));
    }
}
