using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
	float previousDot;

	public float PreviousDot
	{
		get { return previousDot; }
		set { previousDot = value; }
	}

	public void Teleport(Vector3 destination, Quaternion rotation)
	{
		transform.position = destination;
		transform.rotation = rotation;
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