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
            var y =0f;
            while (y < Size.y)
            {
                var z = Size.z / 4f;
                while (z < Size.z)
                {
                    var pos = new Vector3(x, y, z) - Size / 2f;
                    _points.Add(new VolumePoint() { Position = pos });
                    z += Size.z/2f;
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
            Gizmos.color = new Color(0.5f, 0.2f, 0);           
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
        var tmpVertices = new Vector3[_points.Count];
        for (int i = 0; i < _points.Count; i++)
        {

            tmpVertices[i] = _points[i].Position;
        }
        m.vertices = tmpVertices;
        int nx = (int)(Size.x / Density) + 1;
        int ny = (int)(Size.y / Density) + 1;

        int tempTrianglesCount = 4 * (nx + ny + (nx - 1) * (ny - 1) - 2);
        var tmpTriangles = new int[tempTrianglesCount * 3];
        int triCount = 0;
        int triCountL = 6 * (ny - 1) + 12 * (nx - 1);
        int counter = 0;
          while (counter < ny-1)
             {
                 //левая сторона
                 tmpTriangles[counter * 6] = 2*counter;
                 tmpTriangles[counter * 6+ 1] = 2*counter + 1;
                 tmpTriangles[counter * 6 + 2] = 2*counter + 2;
                 tmpTriangles[counter * 6 + 3] = 2*counter + 1;
                 tmpTriangles[counter * 6 + 4] = 2*counter + 3;
                 tmpTriangles[counter * 6 + 5] = 2*counter + 2;
                 //правая сторона
                 tmpTriangles[triCountL+counter * 6] = 2*ny * (nx-1) + 2*counter;
                 tmpTriangles[triCountL + counter * 6 + 1] = 2 * ny * (nx - 1) + 2*counter + 2;
                 tmpTriangles[triCountL + counter * 6 + 2] = 2 * ny * (nx - 1) + 2*counter + 1;
                 tmpTriangles[triCountL + counter * 6 + 3] = 2 * ny * (nx - 1) + 2*counter + 1;
                 tmpTriangles[triCountL + counter * 6 + 4] = 2 * ny * (nx - 1) + 2*counter + 2;
                 tmpTriangles[triCountL + counter * 6 + 5] = 2 * ny * (nx - 1) + 2*counter + 3;

                 counter += 1;
                 triCount += 6;
             }

             counter = 0;
             triCountL = triCount+6*(nx-1);

             while (counter < nx - 1)
             {
                 if (counter == 0)
                 {
                     //нижняя сторона
                     tmpTriangles[triCount + counter * 6] = 0;
                     tmpTriangles[triCount + counter * 6 + 2] = 1;
                     //верхняя сторона
                     tmpTriangles[triCountL + counter * 6] = ny*2-2;
                     tmpTriangles[triCountL + counter * 6 + 1] = ny * 2 - 1;

                 }
                 else
                 {
                     //нижняя сторона
                     tmpTriangles[triCount + counter * 6] = tmpTriangles[triCount + counter * 6 - 2];
                     tmpTriangles[triCount + counter * 6 + 2] = tmpTriangles[triCount + counter * 6 - 1];
                     //верхняя сторона
                     tmpTriangles[triCountL + counter * 6] = tmpTriangles[triCountL + counter * 6 - 1];
                     tmpTriangles[triCountL + counter * 6 + 1] = tmpTriangles[triCountL + counter * 6 - 2];
                 }
                 //нижняя сторона
                 tmpTriangles[triCount + counter * 6 + 1] = tmpTriangles[triCount + counter * 6] + ny * 2;
                 tmpTriangles[triCount + counter * 6 + 3] = tmpTriangles[triCount + counter * 6 + 2];
                 tmpTriangles[triCount + counter * 6 + 4] = tmpTriangles[triCount + counter * 6 + 1];
                 tmpTriangles[triCount + counter * 6 + 5] = tmpTriangles[triCount + counter * 6 + 4] + 1;
                 //верхняя сторона
                 tmpTriangles[triCountL + counter * 6 + 2] = tmpTriangles[triCountL + counter * 6] + ny * 2;
                 tmpTriangles[triCountL + counter * 6 + 3] = tmpTriangles[triCountL + counter * 6 + 1];
                 tmpTriangles[triCountL + counter * 6 + 4] = tmpTriangles[triCountL + counter * 6 + 1]+ny*2;
                 tmpTriangles[triCountL + counter * 6 + 5] = tmpTriangles[triCountL + counter * 6 + 4] - 1;
                 counter += 1;

             }
             triCount = 12 * (ny - 1) + 12 * (nx - 1);
        counter = 0;
        //лицевая сторона
        while (counter < nx - 1)
        {
            int counter2 = 0;
            while (counter2 < ny - 1)
            {
                tmpTriangles[triCount+6 * counter*(ny-1)+counter2 * 6] = 2 * counter * ny +2 * counter2 + ny * 2;
                tmpTriangles[triCount + 6 *counter * (ny - 1) + counter2 * 6 + 1] = 2 * counter * ny + 2 * counter2;
                tmpTriangles[triCount + 6 *counter * (ny - 1) + counter2 * 6 + 2] = 2 * counter * ny + 2 * counter2 + ny * 2 + 2;
                tmpTriangles[triCount + 6 *counter * (ny - 1) + counter2 * 6 + 3] = 2 * counter * ny + 2 * counter2;
                tmpTriangles[triCount + 6 *counter * (ny - 1) + counter2 * 6 + 4] = 2 * counter * ny + 2 * counter2 + 2;
                tmpTriangles[triCount + 6 *counter * (ny - 1) + counter2 * 6 + 5] = 2 * counter * ny + 2 * counter2 + ny * 2 + 2;
                counter2++;
            }
            counter++;
        }
        triCount += 6*(ny-1)*(nx-1);
        counter = 0;
        //задняя сторона
        while (counter < nx - 1)
        {
            int counter2 = 0;
            while (counter2 < ny - 1)
            {
                tmpTriangles[triCount + 6 * counter * (ny - 1) + counter2 * 6] = 2 * counter * ny + 2 * counter2 + ny * 2+1;
                tmpTriangles[triCount + 6 * counter * (ny - 1) + counter2 * 6 + 1] = 2 * counter * ny + 2 * counter2 + ny * 2 + 3;
                tmpTriangles[triCount + 6 * counter * (ny - 1) + counter2 * 6 + 2] = 2 * counter * ny + 2 * counter2+1;
                tmpTriangles[triCount + 6 * counter * (ny - 1) + counter2 * 6 + 3] = 2 * counter * ny + 2 * counter2+1;
                tmpTriangles[triCount + 6 * counter * (ny - 1) + counter2 * 6 + 4] = 2 * counter * ny + 2 * counter2 + ny * 2 + 3;
                tmpTriangles[triCount + 6 * counter * (ny - 1) + counter2 * 6 + 5] = 2 * counter * ny + 2 * counter2 + 3; 
                counter2++;
            }
            counter++;
        }
        m.triangles = tmpTriangles;
        // m.triangles = new int[] { 48,49,50,49,51,50};
        m.RecalculateNormals();

        return m;
    }
}
