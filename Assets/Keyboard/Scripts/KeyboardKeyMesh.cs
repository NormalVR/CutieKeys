using UnityEngine;

namespace Normal.UI {
    [ExecuteInEditMode]
    public class KeyboardKeyMesh : MonoBehaviour {
        static Mesh _sharedMesh;
        static Mesh CreateMesh() {
            Mesh mesh = new Mesh();

            float width  = 1.0f;
            float height = 1.0f;
            float depth  = 1.0f;

            // Top vertices
            Vector3 p0 = new Vector3(-width * 0.5f,    0.0f,  depth * 0.5f);
            Vector3 p1 = new Vector3(-width * 0.5f,    0.0f, -depth * 0.5f);
            Vector3 p2 = new Vector3( width * 0.5f,    0.0f, -depth * 0.5f);
            Vector3 p3 = new Vector3( width * 0.5f,    0.0f,  depth * 0.5f);

            // Bottom vertex
            Vector3 p4 = new Vector3(         0.0f, -height,          0.0f);

            Vector3[] vertices = new Vector3[]
            {
                // Top
                p0, p1, p2, p3,
         
        	    // Left
        	    p3, p0, p4,
         
        	    // Front
        	    p0, p1, p4,
         
        	    // Right
        	    p1, p2, p4,

                // Back
        	    p2, p3, p4
            };

            // Normals
            Vector3 top   = Vector3.up;
            Vector3 left  = Vector3.Cross(p0 - p1, p0 - p4).normalized;
            Vector3 front = Vector3.Cross(p3 - p0, p3 - p4).normalized;
            Vector3 right = Vector3.Cross(p2 - p3, p2 - p4).normalized;
            Vector3 back  = Vector3.Cross(p1 - p2, p1 - p4).normalized;

            Vector3[] normals = new Vector3[]
            {
                // Top
                top, top, top, top,
         
        	    // Left
        	    left, left, left,
         
        	    // Front
        	    front, front, front,
         
        	    // Right
        	    right, right, right,

                // Back
        	    back, back, back
            };

            // Triangles
            int[] triangles = new int[]
            {
                // Top
                2, 1, 0,
                3, 2, 0,
         
        	    // Left
        	    4, 5, 6,
         
        	    // Front
        	    7, 8, 9,
         
        	    // Right
        	    10, 11, 12,

                // Back
        	    13, 14, 15
            };

            mesh.vertices  = vertices;
            mesh.normals   = normals;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();

            return mesh;
        }

        void Awake() {
            if (_sharedMesh == null)
                _sharedMesh = CreateMesh();

            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            filter.mesh = _sharedMesh;
        }
    }
}