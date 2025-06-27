using UnityEngine;

public static class MeshUtils
{
	static readonly Vector3[] Cube3DCorners =
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

	public static Vector3[] GetWorldSpaceCorners(MeshRenderer meshRenderer)
	{
		Vector3[] worldSpaceCorners = new Vector3[Cube3DCorners.Length];

		return GetWorldSpaceCorners(meshRenderer.bounds);
	}
	
	public static Vector3[] GetWorldSpaceCorners(Bounds bounds)
	{
		Vector3[] worldSpaceCorners = new Vector3[Cube3DCorners.Length];
		
		for (int i = 0; i < Cube3DCorners.Length; i++)
		{
			worldSpaceCorners[i] = bounds.center + Vector3.Scale(bounds.extents, Cube3DCorners[i]);
		}
		
		return worldSpaceCorners;
	}
}
