using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalTraveler : MonoBehaviour
{
	public float PreviousDot { get; set; }
	public Collider mainCollider;
	
	public GameObject TravelerClone { get; set; }

	[HideInInspector] public Rigidbody rb;
	
	private void Start()
	{
		if (TravelerClone is not null) {  Destroy(TravelerClone); }
		CreateClone();
		
		rb = GetComponent<Rigidbody>();

		mainCollider = GetComponentInChildren<Collider>();
	}

	public void Teleport(Vector3 destination, Quaternion rotation, Transform originPortal, Transform destinationPortal)
	{
		transform.position = destination;
		transform.rotation = rotation;
		
		var relativeLVelocity = originPortal.InverseTransformVector(rb.linearVelocity);
		relativeLVelocity = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeLVelocity;
		relativeLVelocity = destinationPortal.transform.TransformVector(relativeLVelocity);
        
		var relativeAVelocity = originPortal.InverseTransformVector(rb.linearVelocity);
		relativeAVelocity = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeAVelocity;
		relativeAVelocity = destinationPortal.transform.TransformVector(relativeAVelocity);
        
		rb.linearVelocity = relativeLVelocity;
		rb.angularVelocity = relativeAVelocity;
	}

	public void CreateClone()
	{
		var clone = new GameObject();
		var meshRenderer = clone.AddComponent<MeshRenderer>();
		var filter = clone.AddComponent<MeshFilter>();

		meshRenderer.material = this.GetComponentInChildren<MeshRenderer>().material;
		filter.mesh = this.GetComponentInChildren<MeshFilter>().mesh;

		clone.transform.localScale = GetComponentInChildren<MeshRenderer>().transform.localScale;
		
		clone.SetActive(false);

		TravelerClone = clone;
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Portal") == false) { return; }

		other.GetComponentInParent<Portal>().ToggleWallCollision(willIgnore: true, this);
		other.GetComponentInParent<Portal>().OnObjectEnterPortal(this);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Portal") == false) { return; }

		other.GetComponentInParent<Portal>().ToggleWallCollision(willIgnore: false, this);
		other.GetComponentInParent<Portal>().OnPlayerExitPortal(this);
	}
}