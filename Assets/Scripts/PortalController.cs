using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86;
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


    [SerializeField] Transform targetPortal;

    [SerializeField] MeshRenderer portalMesh;

    [SerializeField] Camera playerCamera;
    [SerializeField] Player player;

    Vector3[] portalScreenPoints = new Vector3[4];
    Vector2[] portalUvPoints = new Vector2[4];

    private void Start()
    {
        portalCamStartingPos = portalCamPivot.position;
        portalCamStartingRot = portalCamPivot.transform.localRotation.eulerAngles;
        portalPivotForward = portalCamPivot.transform.forward;
        portalPivotRight = portalCamPivot.transform.right;
    }

    private void Update()
    {
        float distanceZ = Mathf.Abs(player.transform.position.z - portal.position.z);
        float distanceX = player.transform.position.x - portal.position.x;

        Vector3 offsetZ = targetPortal.transform.forward * -1 * distanceZ * 0.7f;
        Vector3 offsetX = targetPortal.transform.right * -1 * distanceX;

        portalCamPivot.transform.position = portalCamStartingPos - offsetZ + offsetX;


        RotateCameraWithPlayer();

        //Vector3[] portalVertices =
        //{
        //    portalMesh.bounds.min,                                                          // Bot-Left  
        //    new(portalMesh.bounds.max.x, portalMesh.bounds.min.y, portalMesh.bounds.max.z), // Bot-Right 
        //    new(portalMesh.bounds.min.x, portalMesh.bounds.max.y, portalMesh.bounds.min.z), // Top-Left 
        //    portalMesh.bounds.max,                                                          // Top-Right
        //};

        Vector3[] portalVertices = portalMesh.GetComponent<MeshFilter>().mesh.vertices;

        float near = playerCamera.nearClipPlane;
        
        for (int i = 0; i < portalVertices.Length; i++)
        {
            //portalScreenPoints[i] = playerCamera.WorldToScreenPoint(portalVertices[i]);

            //print($"World  [{i}] {portalVertices[i]}");

            Vector3 point = playerCamera.worldToCameraMatrix.MultiplyPoint(portalMesh.transform.TransformPoint(portalVertices[i]));

            Vector3 rotatedPos = Quaternion.Inverse(playerCamera.transform.rotation) * point;


            Vector4 projected = playerCamera.projectionMatrix * new Vector4(rotatedPos.x, rotatedPos.y, rotatedPos.z, 1);
            projected /= projected.w;

            //print($"point {i} in vp: " + playerCamera.WorldToViewportPoint(portalVertices[i]));

            //float scaledX = point.x / -point.z;
            //float scaledY = point.y / -point.z;

            //float uvX = (scaledX + 1) / 2f;
            //float uvY = (scaledY + 1) / 2f;

            float uvX = (projected.x + 1) / 2f;
            float uvY = (projected.y + 1) / 2f;

            //uvX = Mathf.Clamp01(uvX);
            //uvY = Mathf.Clamp01(uvY);

            portalUvPoints[i] = new(uvX, uvY);
            //print($"Screen [{i}] {portalUvPoints[i]}");
        }

        portalMesh.GetComponent<MeshFilter>().mesh.uv = portalUvPoints;

        //print("--------------------");

        //float near = playerCamera.nearClipPlane;
        //float far = playerCamera.farClipPlane;

        //float scale = near / Mathf.Abs(portalScreenPoints[0].z);

        //float top = portalScreenPoints[0].y   / portalScreenPoints[0].z * near;
        //float bot = portalScreenPoints[1].y   / portalScreenPoints[1].z * near;
        //float left = portalScreenPoints[1].x  / portalScreenPoints[1].z * near;
        //float right = portalScreenPoints[0].x / portalScreenPoints[0].z * near;


        //print($"Left: {left}, Right: {right}, Bot: {bot}, Top: {top}");

        //Matrix4x4 frustum = Matrix4x4.Frustum(left, right, bot, top, near, far);

        //Matrix4x4 flipY = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));

        //frustum *= flipY;

        //float fov = 2.0f * Mathf.Atan(Mathf.Abs(top - bot) / (2 * near)) * Mathf.Rad2Deg;

        //portalCamera.fieldOfView = 60;

        //print($"fov calculado: {fov}, fov da camera: {portalCamera.fieldOfView}");
        //float aspect = Screen.width / Screen.height;

        //print($"aspect certo : {aspect}, aspect da camera: {portalCamera.aspect}");
        //portalCamera.aspect = aspect;

        //portalCamera.projectionMatrix = frustum;

        
    }

    private void RotateCameraWithPlayer()
    {
        float offsetX = portal.position.x - player.transform.position.x;
        float offsetZ = portal.position.z - player.transform.position.z;

        float angle = Mathf.Atan2(offsetZ, offsetX) * Mathf.Rad2Deg;

        //portalCamPivot.transform.localRotation = Quaternion.Euler(0,(Mathf.Rad2Deg * angle), 0);

        //playerCamera.transform.rotation = Quaternion.Euler(new(portalCamStartingRot.x, portalCamStartingRot.y + angle, portalCamStartingRot.z));

        //portalCamPivot.rotation = Quaternion.Euler(0, -angle, 0);

        portalCamPivot.localRotation = Quaternion.Euler(0, portalCamStartingRot.y + player.transform.localRotation.eulerAngles.y, 0);
        //portalCamera.transform.localRotation = Quaternion.Euler(new(playerCamera.transform.localRotation.eulerAngles.x, portalCamera.transform.localRotation.y, 0));
    }
}
