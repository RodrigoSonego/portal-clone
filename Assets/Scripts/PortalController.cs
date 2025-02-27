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
    [SerializeField] float maxCameraDistance = 2;
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
        float distance = Mathf.Abs(player.transform.position.z - portal.position.z);
        distance = Mathf.Clamp(distance, 0, maxCameraDistance);

        Vector3 offset = portalCamPivot.transform.forward * distance;
        portalCamPivot.transform.position = portalCamStartingPos - offset;


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
            //print($"Screen [{i}] {portalScreenPoints[i]}");
        }

        //print("--------------------");

        float near = playerCamera.nearClipPlane;
        float far = playerCamera.farClipPlane;


        float top = portalScreenPoints[0].y   / portalScreenPoints[0].z * near;
        float bot = portalScreenPoints[1].y   / portalScreenPoints[1].z * near;
        float left = portalScreenPoints[1].x  / portalScreenPoints[2].z * near;
        float right = portalScreenPoints[0].x / portalScreenPoints[3].z * near;


        Matrix4x4 frustum = Matrix4x4.Frustum(left, right, bot, top, near, far);

        Matrix4x4 flipY = Matrix4x4.Rotate(Quaternion.Euler(0,0,180));

        frustum = frustum * flipY;

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
