using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Portal : MonoBehaviour
{
	[Header("Portal Refences")] [SerializeField]
	Transform portal;

	[SerializeField] MeshRenderer portalMesh;
	[SerializeField] Portal linkedPortal;
	[SerializeField] Collider portalWallCollider;
	[Space] [SerializeField] Player player;

	[Header("Portal Camera Config")] [SerializeField]
	Transform portalCamPivot;

	[SerializeField] Camera portalCamera;
	[SerializeField] float minDotForObliqueProj;
	[SerializeField] private int recursionLimit = 3;

	Camera playerCamera;

	List<PortalTraveler> trackedTravelers;

	private RenderTexture renderTexture;
	
	Matrix4x4[] posMatrices;

	private void Awake()
	{
		if (linkedPortal == null)
		{
			return;
		}

		playerCamera = Camera.main;

		CreateRenderTexture();

		// RenderPipelineManager.beginContextRendering += PreRender;

		trackedTravelers = new List<PortalTraveler>();

		posMatrices = new Matrix4x4[recursionLimit];
	}

	private void Update()
	{
		PreRender();
	}

	void PreRender()
	{
		// print(cam.name);
		if (linkedPortal == null)
		{
			return;
		}
		
		bool linkedPortalIsOnCamera = CameraUtils.AreBoundsOnCamera(playerCamera, linkedPortal.portalMesh.bounds);
		
		if (linkedPortalIsOnCamera == false)
		{
			return;
		}
		
		SetPortalCamRelativeToPlayer();
	}

	private void LateUpdate()
	{
		HandlePortalInteraction();
	}

	private void CreateRenderTexture()
	{
		renderTexture = new RenderTexture(Screen.width, Screen.height, 32);
		portalCamera.targetTexture = renderTexture;
		linkedPortal.portalMesh.material.SetTexture("_MainTex", renderTexture);
	}

	// Sets a Oblique projection to coincide the near plane with the portal plane
	// Further Reference:   https://www.terathon.com/lengyel/Lengyel-Oblique.pdf
	//                      https://danielilett.com/2019-12-18-tut4-3-matrix-matching/
	private void CreateObliqueProjection(bool willUseMinDot)
	{
		// Could also use distance here, using portal.InverseTransformPoint(player.transform.position) and scaling by portal.forward
		float dot = Vector3.Dot(linkedPortal.transform.forward,
			(linkedPortal.transform.position - player.transform.position));

		if (willUseMinDot && dot <= minDotForObliqueProj)
		{
			portalCamera.projectionMatrix = playerCamera.projectionMatrix;
			return;
		}

		//Creates a plane at the portal position and set the normal
		Plane plane = new Plane(-portal.forward, portal.position);
		Vector4 clipPlane = new(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

		//Transforms clip plane to camera space (need to transpose the inverse matrix because dealing with normal vector)
		Vector4 clipPlaneCameraSpace =
			Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlane;

		portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
	}

	private void SetPortalCamRelativeToPlayer()
	{
		// Calculations assume both portals have their Forward direction (blue arrow) facing the wall, will break otherwise
		// var (camPos, camRot) = GetPositionAndRotationToOtherPortal(origin: linkedPortal.transform, portal, playerCamera.transform);
		var relativeMat = playerCamera.transform.localToWorldMatrix;
		var matrices = new Matrix4x4[recursionLimit];

		bool willNeedRecursion = CameraUtils.CheckIfBoundsIntersectOnCamera(portalCamera, portalMesh.bounds, linkedPortal.portalMesh.bounds);
		
		int recursionNumber = willNeedRecursion ? recursionLimit : 1;
		
		Matrix4x4 invertedPortalMat = GetTransformMatrixWithInvertedRotation(portal);
		for (int i = 0; i < recursionNumber; i++)
		{
			relativeMat = invertedPortalMat * linkedPortal.transform.worldToLocalMatrix * relativeMat;
			
			matrices[i] = relativeMat;
			
			posMatrices[i] = relativeMat;
		}

		for (int i = recursionNumber - 1; i >= 0; i--)
		{
			portalCamPivot.transform.position = matrices[i].GetPosition();
			portalCamera.transform.rotation = matrices[i].rotation;
			linkedPortal.portalMesh.material.SetInt("_HideView", 0);

			// Only consider the min dot on the nearest camera, since would cause artifacts when applied to further ones
			CreateObliqueProjection(willUseMinDot: i == 0);
			
			if (willNeedRecursion && i == recursionNumber-1)
			{
				linkedPortal.portalMesh.material.SetInt("_HideView", 1);
			}
			
			// Should use SubmitRenderRequest but that just doesn't work, so will leave this
			// UniversalRenderPipeline.RenderSingleCamera(context, portalCamera);
			var req = new UniversalRenderPipeline.SingleCameraRequest()
			{
				destination = renderTexture
			};
			
			RenderPipeline.SubmitRenderRequest(portalCamera, req);
		}
	}

	private void HandlePortalInteraction()
	{
		foreach (var traveler in trackedTravelers)
		{
			float travelerDot = Vector3.Dot(portal.transform.forward,
				portal.transform.position - traveler.transform.position);
			if (traveler.PreviousDot == 0)
			{
				traveler.PreviousDot = travelerDot;
			}

			GameObject clone = traveler.TravelerClone;

			(Vector3 clonePos, Quaternion cloneRot) =
				GetPositionAndRotationToOtherPortal(portal, linkedPortal.transform, traveler.transform);

			clone.transform.position = clonePos;
			clone.transform.rotation = cloneRot;

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
		Vector3 positionToPortal;
		Quaternion rotationToPortal;

		(positionToPortal, rotationToPortal) =
			GetPositionAndRotationToOtherPortal(origin: portal, linkedPortal.transform, traveler.transform);

		traveler.Teleport(positionToPortal, rotationToPortal, portal, linkedPortal.transform);

		traveler.PreviousDot = Vector3.Dot(linkedPortal.transform.forward,
			linkedPortal.transform.position - traveler.transform.position);

		linkedPortal.ToggleWallCollision(willEnable: false);
	}

	public void ToggleWallCollision(bool willEnable)
	{
		if (portalWallCollider is null)
		{
			print($"portal {gameObject.name} sem parede");
			return;
		}

		portalWallCollider.enabled = willEnable;
	}

	public void OnObjectEnterPortal(PortalTraveler traveler)
	{
		if (trackedTravelers.Contains(traveler))
		{
			return;
		}

		trackedTravelers.Add(traveler);
		traveler.TravelerClone.SetActive(true);
	}

	public void OnPlayerExitPortal(PortalTraveler traveler)
	{
		if (trackedTravelers.Contains(traveler))
		{
			trackedTravelers.Remove(traveler);
			traveler.TravelerClone.SetActive(false);
		}
	}

	/// <summary>
	/// Calculates the World position of subject relative to other portal based on its position to origin portal.
	/// Will return position behind destination portal.
	/// </summary>
	/// <param name="origin"></param>
	/// <param name="destination"></param>
	/// <param name="subject"></param>
	/// <returns>Position and rotation behind destination portal</returns>
	private (Vector3, Quaternion) GetPositionAndRotationToOtherPortal(Transform origin, Transform destination,
		Transform subject)
	{
		Matrix4x4 invertedDestinationRotation = GetTransformMatrixWithInvertedRotation(destination.transform);

		Matrix4x4 mat = invertedDestinationRotation * origin.transform.worldToLocalMatrix *
		                subject.transform.localToWorldMatrix;
		return (mat.GetPosition(), mat.rotation);
	}

	/// <summary>
	/// Generates world matrix of transform with its rotation flipped on the Y axis
	/// </summary>
	/// <param name="target"></param>
	/// <returns>Matrix with rotation flipped</returns>
	private Matrix4x4 GetTransformMatrixWithInvertedRotation(Transform target)
	{
		Matrix4x4 inverseRotatedMatrix = Matrix4x4.identity;

		Quaternion inverseRotation = Quaternion.Euler(0, 180, 0) * target.rotation;

		inverseRotatedMatrix.SetTRS(target.position, inverseRotation, target.localScale);

		return inverseRotatedMatrix;
	}

	private void OnDrawGizmos()
	{
		if (posMatrices == null)
		{
			return;
		}
		Gizmos.color = Color.green;
		
		foreach (var mat in posMatrices)
		{
			Gizmos.DrawSphere(mat.GetPosition(), 0.5f);

		}
	}
}