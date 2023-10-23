using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SimpleProceduralMesh : MonoBehaviour
{
	public Material MeshMaterial;
	public Vector3[] MeshVertices = new Vector3[4];
	
	public void Initialize () {
		var mesh = new Mesh {
			name = "Procedural Mesh"
		};
		
		mesh.vertices = new Vector3[] {
			MeshVertices[0], MeshVertices[1], MeshVertices[2], MeshVertices[3]
		};

		mesh.normals = new Vector3[] {
			Vector3.back, Vector3.back, Vector3.back, Vector3.back
		};

		mesh.tangents = new Vector4[] {
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f)
		};

		mesh.uv = new Vector2[] {
			Vector2.zero, Vector2.right, Vector2.up, Vector2.one
		};

		mesh.triangles = new int[] {
			0, 2, 1, 1, 2, 3
		};

		GetComponent<MeshFilter>().mesh = mesh;
		var meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material = MeshMaterial;
		mesh.RecalculateNormals();
	}

	public void CreateMeshes()
	{
		
	}
}