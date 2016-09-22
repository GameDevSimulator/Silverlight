using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshCollider))]
public class DarknessVolume : MonoBehaviour
{
    private MeshCollider _meshCollider;
    private Mesh _debugMesh;

    [Range(0.1f, 1f)]
    public float Density;
    public Vector3 Size;

    private List<VolumePoint> _points = new List<VolumePoint>();

    public class VolumePoint
    {
        public Vector3 Position;
        public float State;
    }

    // В редакторе не работает. Только при запуске игры
	void Start ()
	{
	    _meshCollider = GetComponent<MeshCollider>();

        FillWithPoints();
	    
	}
	
	void Update ()
    {
	    foreach (var volumePoint in _points)
	    {
	        volumePoint.State = Random.value;
	    }
	}

    public void OnBeamRayHit(RaycastHit hit)
    {
        // USEFUL:
        // hit.point 
        // hit.triangleIndex
        // hit.barycentricCoordinate ??? google


        // Debug.Log("HIT!!");
    }

    [ContextMenu("CreateTrianlges")]
    void CreateTrianlges()
    {
        if (_meshCollider == null)
            _meshCollider = GetComponent<MeshCollider>();

        /*
        var mesh = new Mesh();
        mesh.vertices = new Vector3[3];
        mesh.vertices[0] = _points[0];
        mesh.vertices[1] = _points[1];
        mesh.vertices[2] = _points[2];

        // ИНДЕКСЫ ВЕРШИН
        // 1 треугольник - 3 вершины, поэтому размер trinagles = размер vertices * 3
        mesh.triangles = new int[6];
        mesh.triangles[0] = 0; // индекс (номер) вершины
        mesh.triangles[1] = 1;
        mesh.triangles[2] = 2;

        mesh.triangles[3] = 2; // индекс (номер) вершины
        mesh.triangles[4] = 1;
        mesh.triangles[5] = 0;
        */

        var mesh = CreateMesh(10f, 10f);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // обновить коллайдер
        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;

        _debugMesh = mesh;
    }

    [ContextMenu("Fill With Points")]
    void FillWithPoints()
    {
        if(_points == null)
            _points = new List<VolumePoint>();

        var x = 0f;
        while (x < Size.x)
        {
            var y = 0f;
            while (y < Size.y)
            {
                var z = 0f;
                while (z < Size.z)
                {
                    var pos = new Vector3(x, y, z) - Size / 2f;
                    _points.Add(new VolumePoint() { Position = pos });
                    z += Density;
                }

                y += Density;
            }

            x += Density;
        }
    }

    [ContextMenu("Clear Points")]
    void ClearPoints()
    {
        if (_points == null)
            _points = new List<VolumePoint>();

        _points.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Size);

        foreach (var point in _points)
        {
            Gizmos.color = new Color(point.State, point.State, point.State);
            //Gizmos.DrawSphere(transform.position + point.Position, 0.1f);
            Gizmos.DrawWireSphere(transform.position + point.Position, 0.1f);
        }

        if (_debugMesh != null)
        {
            Gizmos.DrawWireMesh(_debugMesh, transform.position);
        }
    }

    Mesh CreateMesh(float width, float height)
    {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        m.vertices = new Vector3[] {
             new Vector3(-width, -height, 0.01f),
             new Vector3(width, -height, 0.01f),
             new Vector3(width, height, 0.01f),
             new Vector3(-width, height, 0.01f)
         };
        
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();

        return m;
    }
}
