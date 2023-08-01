using UnityEngine;

public class TextureScript : MonoBehaviour
{
    public void PerformUVDecomposition()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found on the GameObject.");
            return;
        }

        // Get the mesh from the MeshFilter
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("Mesh not found on the MeshFilter.");
            return;
        }

        // Calculate UVs for the mesh
        Vector2[] uvs = new Vector2[mesh.vertexCount];
        Unwrapping.GenerateSecondaryUVSet(mesh, uvs);

        // Apply the new UVs to the mesh
        mesh.uv2 = uvs;

        Debug.Log("UV unwrapping completed.");
    }
}
