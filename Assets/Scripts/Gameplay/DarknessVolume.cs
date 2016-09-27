using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshCollider))]
public class DarknessVolume : MonoBehaviour
{
    [Range(0.1f, 10f)]
    public float Density;
    public Vector3 Size;
    public float AppearTime;
    public float DisappearTime;
    public bool DrawDebugMesh;

    public class VolumePoint
    {
        public bool IsActive { get { return State > 0.2f; } }

        public Vector3 Position;
        public float State;

        public VolumePoint()
        {
            State = 1f;
        }

        public VolumePoint(Vector3 position)
        {
            Position = position;
            State = 1f;
        }
    }

    public class Face
    {
        public int Point1;
        public int Point1Back;
        public int Point2;
        public int Point2Back;
        public int Rotation;
        public bool ReverseFace;
        public int X1;
        public int Y1;
        public int X2;
        public int Y2;
    }

    private MeshCollider _meshCollider;
    private Mesh _debugMesh;
    private List<VolumePoint> _points = new List<VolumePoint>();
    private List<Face> _faces = new List<Face>();
    private List<Face> _frontier = new List<Face>();
    private int _xCount;
    private int _yCount;
    private bool _rebuildRequired = false;

    // В редакторе не работает. Только при запуске игры
    void Start ()
	{
	    _meshCollider = GetComponent<MeshCollider>();
        Init();
	}
	
	void Update ()
    {
        foreach (var p in _points)
	    {
	        if (p.State < 1f)
	        {
	            p.State += AppearTime*Time.deltaTime;
	            
                if(p.State > 1f)
	            {
	                p.State = 1f;
	                _rebuildRequired = true;
	            }
	        }
	    }

        if (_rebuildRequired)
        {
            BuildFrontier();
            BuildMesh();
            _rebuildRequired = false;
        }
    }

    void FixedUpdate()
    {
        
    }

    public void OnBeamRayHit(RaycastHit hit)
    {
        // USEFUL:
        // hit.point 
        // hit.triangleIndex
        // hit.barycentricCoordinate ??? google

        /*
        // DRAW HIT TRIANGLE
        Mesh mesh = _meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
        Transform hitTransform = hit.collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);
        Debug.DrawLine(p0, p1);
        Debug.DrawLine(p1, p2);
        Debug.DrawLine(p2, p0);
        */

        // Debug.Log("HIT!!");

        var mesh = _meshCollider.sharedMesh;
        var triangles = mesh.triangles;
        var p0 = _points[triangles[hit.triangleIndex * 3 + 0]];
        var p1 = _points[triangles[hit.triangleIndex * 3 + 1]];
        var p2 = _points[triangles[hit.triangleIndex * 3 + 2]];

        if (p0.IsActive)
        {
            p0.State -= DisappearTime*Time.deltaTime;
            if (!p0.IsActive)
            {
                _rebuildRequired = true;
            }
        }

        if (p1.IsActive)
        {
            p1.State -= DisappearTime * Time.deltaTime;
            if (!p1.IsActive)
            {
                _rebuildRequired = true;
            }
        }

        if (p2.IsActive)
        {
            p2.State -= DisappearTime * Time.deltaTime;
            if (!p2.IsActive)
            {
                _rebuildRequired = true;
            }
        }
    }

    [ContextMenu("CreateTrianlges")]
    void CreateTrianlges()
    {
        if (_meshCollider == null)
            _meshCollider = GetComponent<MeshCollider>();

       

       var mesh = CreateMesh(10f, 10f);

        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // обновить коллайдер
        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;

        _debugMesh = mesh;
    }

    [ContextMenu("Init")]
    void Init()
    {
        _points.Clear();
        _frontier.Clear();

        var count = Size * Density;
        _yCount = (int) Mathf.Round(count.y) + 1;
        _xCount = (int) Mathf.Round(count.x) + 1;
        var idx = 0;
        
        for (var xi = 0; xi < _xCount; xi++)
        {
            for (var yi = 0; yi < _yCount; yi++)
            {
                var x = xi * (1f / Density);
                var y = yi * (1f / Density);

                var p1 = new VolumePoint(new Vector3(x, y, 0f) - Size/2f);
                _points.Add(p1);
                idx++;

                var p2 = new VolumePoint(new Vector3(x, y, Size.z) - Size / 2f);
                _points.Add(p2);
                idx++;

                if (x > 0)
                {
                    var f = new Face()
                    {
                        Point1 = idx - 2, 
                        Point1Back = idx - 1,
                        Point2 = idx - _yCount * 2 - 2,
                        Point2Back = idx - _yCount * 2 - 1,
                        X1 = xi,
                        X2 = xi - 1,
                        Y1 = yi,
                        Y2 = yi,
                        Rotation = 0
                    };
                    _faces.Add(f);
                }

                if (y > 0)
                {
                    var f = new Face()
                    {
                        Point1 = idx - 2,
                        Point1Back = idx - 1,
                        Point2 = idx - 4,
                        Point2Back = idx - 3,
                        X1 = xi,
                        X2 = xi,
                        Y1 = yi,
                        Y2 = yi - 1,
                        Rotation = 2
                    };
                    _faces.Add(f);
                }

                if (x > 0 && y > 0)
                {
                    var f = new Face()
                    {
                        Point1 = idx - 2,
                        Point1Back = idx - 1,
                        Point2 = idx - _yCount * 2 - 4,
                        Point2Back = idx - _yCount * 2 - 3,
                        X1 = xi,
                        X2 = xi - 1,
                        Y1 = yi,
                        Y2 = yi - 1,
                        Rotation = 1
                    };
                    _faces.Add(f);

                    var fOpposite = new Face()
                    {
                        Point1 = idx - _yCount * 2 - 2,
                        Point1Back = idx - _yCount * 2 - 1,
                        Point2 = idx - 4,
                        Point2Back = idx - 3,
                        X1 = xi - 1,
                        X2 = xi,
                        Y1 = yi,
                        Y2 = yi - 1,
                        Rotation = 4
                    };
                    _faces.Add(fOpposite);
                }
            }
        }

        BuildFrontier();
        BuildMesh();
        _rebuildRequired = false;
    }

    void BuildFrontier()
    {
        _frontier.Clear();

        foreach (var face in _faces)
        {
            var p1 = _points[face.Point1];
            var p1B = _points[face.Point1Back];
            var p2 = _points[face.Point2];
            var p2B = _points[face.Point2Back];

            if(!p1.IsActive || !p2.IsActive)
                continue;

            if(face.Rotation == 0)
            {
                if ((IsActivePoint(face.X1, face.Y1 + 1) || IsActivePoint(face.X2, face.Y2 + 1)) &&
                    !IsActivePoint(face.X1, face.Y1 - 1) &&
                    !IsActivePoint(face.X2, face.Y2 - 1))
                {
                    _frontier.Add(face);
                    face.ReverseFace = true;
                    continue;
                }

                if (!IsActivePoint(face.X1, face.Y1 + 1) &&
                    !IsActivePoint(face.X2, face.Y2 + 1) &&
                    (IsActivePoint(face.X1, face.Y1 - 1) || IsActivePoint(face.X2, face.Y2 - 1)))
                {
                    _frontier.Add(face);
                    continue;
                }
            }

            if (face.Rotation == 2)
            {
                if ((IsActivePoint(face.X1 + 1, face.Y1) || IsActivePoint(face.X2 + 1, face.Y2)) &&
                    !IsActivePoint(face.X1 - 1, face.Y1) &&
                    !IsActivePoint(face.X2 - 1, face.Y2))
                {
                    _frontier.Add(face);
                    continue;
                }

                if (!IsActivePoint(face.X1 + 1, face.Y1) &&
                    !IsActivePoint(face.X2 + 1, face.Y2) &&
                    (IsActivePoint(face.X1 - 1, face.Y1) || IsActivePoint(face.X2 - 1, face.Y2)))
                {
                    _frontier.Add(face);
                    face.ReverseFace = true;
                    continue;
                }
            }

            if (face.Rotation == 1)
            {
                if (IsActivePoint(face.X2, face.Y1) && !IsActivePoint(face.X1, face.Y2))
                {
                    _frontier.Add(face);
                    face.ReverseFace = true;
                    continue;
                }

                if (!IsActivePoint(face.X2, face.Y1) && IsActivePoint(face.X1, face.Y2))
                {
                    _frontier.Add(face);
                    //face.ReverseFace = true;
                    continue;
                }
            }

            if (face.Rotation == 4)
            {
                if (IsActivePoint(face.X1, face.Y2) && !IsActivePoint(face.X2, face.Y1))
                {
                    _frontier.Add(face);
                    face.ReverseFace = true;
                    continue;
                }

                if (!IsActivePoint(face.X1, face.Y2) && IsActivePoint(face.X2, face.Y1))
                {
                    _frontier.Add(face);
                    //face.ReverseFace = true;
                    continue;
                }
            }
        }
    }

    void BuildMesh()
    {
        var mesh = new Mesh();
        mesh.name = "ScriptedMesh2";
        mesh.vertices = _points.Select(p => p.Position).ToArray();

        var faceTriangles = new int[_faces.Count * 3 * 2];
        var faceIdx = 0;
        foreach (var face in _frontier)
        {
            if (!face.ReverseFace)
            {
                faceTriangles[faceIdx*6 + 0] = face.Point1;
                faceTriangles[faceIdx*6 + 1] = face.Point2;
                faceTriangles[faceIdx*6 + 2] = face.Point1Back;

                faceTriangles[faceIdx*6 + 3] = face.Point1Back;
                faceTriangles[faceIdx*6 + 4] = face.Point2;
                faceTriangles[faceIdx*6 + 5] = face.Point2Back;
            }
            else
            {
                faceTriangles[faceIdx * 6 + 1] = face.Point1;
                faceTriangles[faceIdx * 6 + 0] = face.Point2;
                faceTriangles[faceIdx * 6 + 2] = face.Point1Back;

                faceTriangles[faceIdx * 6 + 4] = face.Point1Back;
                faceTriangles[faceIdx * 6 + 3] = face.Point2;
                faceTriangles[faceIdx * 6 + 5] = face.Point2Back;
            }
            faceIdx++;
        }

        mesh.triangles = faceTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (_meshCollider == null)
            _meshCollider = GetComponent<MeshCollider>();

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;
        _debugMesh = mesh;
    }

    private VolumePoint PointByCoord(int x, int y, int z = 0)
    {
        if (y >= _yCount || y < 0)
            return null;

        if (x >= _xCount || x < 0)
            return null;

        return _points[(x * _yCount + y) * 2 + z];
    }

    private void CoordFromIndex(int index, out int x, out int y)
    {
        x = (index / 2) / _yCount;
        y = (index / 2) % _xCount;
    }

    private bool IsActivePoint(int x, int y, int z = 0)
    {
        var p = PointByCoord(x, y, z);

        if (p == null)
            return false;

        return p.IsActive;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Size);

        foreach (var point in _points)
        {
            Gizmos.color = new Color(point.State, point.State, point.State);           
            Gizmos.DrawWireSphere(transform.position + point.Position, 0.01f);
        }

        if (_debugMesh != null && DrawDebugMesh)
        {
            Gizmos.color = Color.cyan;
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
