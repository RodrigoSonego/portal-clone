using UnityEngine;

public static class CameraUtils
{
	public static bool AreBoundsOnCamera(Camera camera, Bounds bounds)
	{
		var planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, bounds);
	}
	
	public static bool CheckIfBoundsIntersectOnCamera(Camera cam, Bounds nearBounds, Bounds farBounds)
	{
		MinMax3D near = GetMinMaxFromPositions(GetScreenBounds(cam, nearBounds));
		MinMax3D far = GetMinMaxFromPositions(GetScreenBounds(cam, farBounds));
		
		if (far.ZMax < near.ZMin)
		{
			return false;
		}

		if (far.XMax < near.XMin || far.XMin > near.XMax)
		{
			return false;
		}

		if (far.YMax < near.YMin || far.YMin > near.YMax)
		{
			return false;
		}
		
		return true;
	}
	
	// Got it from Sebastian League's video, which he got from https://www.turiyaware.com/a-solution-to-unitys-camera-worldtoscreenpoint-causing-ui-elements-to-display-when-object-is-behind-the-camera/
	private static MinMax3D GetMinMaxFromPositions(Vector3[] positions)
	{
		MinMax3D minMaxCorners = new MinMax3D(min: float.MinValue, max: float.MaxValue);
		
		foreach (var pos in positions)
		{
			minMaxCorners.AddVector(pos);
		}

		return minMaxCorners;
	}
	
	private static Vector3[] GetScreenBounds(Camera cam, Bounds bounds)
	{
		Vector3[] worldSpaceCorners = MeshUtils.GetWorldSpaceCorners(bounds);
		Vector3[] camSpaceCorners = new Vector3[8];
		
		for (int i = 0; i < worldSpaceCorners.Length; i++)
		{
			Vector3 corner = cam.WorldToViewportPoint(worldSpaceCorners[i]);

			if (corner.z < 0)
			{
				corner.x = corner.x <= 0.5f ? 1 : 0;
				corner.y = corner.y <= 0.5f ? 1 : 0;
			}

			camSpaceCorners[i] = corner;
		}
		
		return camSpaceCorners;
	}
}


public struct MinMax3D
{
	public float XMin;
	public float XMax;
	public float YMin;
	public float YMax;
	public float ZMax;
	public float ZMin;

	public MinMax3D(float min, float max)
	{
		XMax = min;
		XMin = max;
		YMax = min;
		YMin = max;
		ZMax = min;
		ZMin = max;
	}

	public void AddVector(Vector3 vector)
	{
		XMax = Mathf.Max(vector.x, XMax);
		XMin = Mathf.Min(vector.x, XMin);
		YMax = Mathf.Max(vector.y, YMax);
		YMin = Mathf.Min(vector.y, YMin);
		ZMax = Mathf.Max(vector.z, ZMax);
		ZMin = Mathf.Min(vector.z, ZMin);
	}
}