using UnityEngine;

public static class CameraUtils
{
	private static Vector3[] cube3DCorners =
	{
		new(-1, -1, -1),
		new(1, -1, -1),
		new(-1, 1, -1),
		new(-1, -1, 1),
		new(-1, 1, 1),
		new(1, -1, 1),
		new(1, 1, -1),
		new(1, 1, 1),
	};

	public static bool AreBoundsOnCamera(Camera camera, Bounds bounds)
	{
		var planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, bounds);
	}
	
	public static bool CheckIfBoundsIntersectOnCamera(Camera cam, Bounds nearBounds, Bounds farBounds)
	{
		var planes = GeometryUtility.CalculateFrustumPlanes(cam);

		// MinMax3D portalScreenBounds = GetScreenBounds(portalCamera, portalBounds);
		MinMax3D near = GetScreenBounds(cam, nearBounds);
		MinMax3D far = GetScreenBounds(cam, farBounds);

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

	private static MinMax3D GetScreenBounds(Camera cam, Bounds bounds)
	{
		MinMax3D minMaxCorners = new MinMax3D(min: float.MinValue, max: float.MaxValue);

		for (int i = 0; i < 8; i++)
		{
			Vector3 corner = cam.WorldToScreenPoint(bounds.center + Vector3.Scale(bounds.extents, cube3DCorners[i]));
			minMaxCorners.AddVector(corner);
		}

		return minMaxCorners;
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