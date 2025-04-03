using System;
using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
	public float PreviousDot { get; set; }

	public GameObject TravelerClone { get; set; }

	private void Start()
	{
		if (TravelerClone is not null) {  Destroy(TravelerClone); }
		CreateClone();
		print(name + " has been created");
	}

	public void Teleport(Vector3 destination, Quaternion rotation)
	{
		transform.position = destination;
		transform.rotation = rotation;
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

		other.GetComponentInParent<Portal>().ToggleWallCollision(willEnable: false);
		other.GetComponentInParent<Portal>().OnObjectEnterPortal(this);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Portal") == false) { return; }

		other.GetComponentInParent<Portal>().ToggleWallCollision(willEnable: true);
		other.GetComponentInParent<Portal>().OnPlayerExitPortal(this);
	}
}