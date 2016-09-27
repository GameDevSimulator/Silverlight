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

    [Range(0f, 1f)]
    public float ActiveColliderThreshold;

    [Range(0f, 1f)]
    public float Splash = 0.1f;

    [Range(0.1f, 2f)]
    public float NeighboursModifier = 1f;

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
    private readonly List<Vector3> _verticies = new List<Vector3>();
    private readonly List<Face> _faces = new List<Face>();
    private readonly List<Face> _frontier = new List<Face>();
    private readonly List<float> _states = new List<float>();
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
    }

    void FixedUpdate()
    {
        for (var xi = 0; xi < _xCount; xi++)
        {
            for (var yi = 0; yi < _yCount; yi++)
            {
                if (IsActivePoint(xi, yi))
                    continue;

                var activeNeighbors = 0;

                if (IsActivePoint(xi + 1, yi)) activeNeighbors++;
                if (IsActivePoint(xi - 1, yi)) activeNeighbors++;
                if (IsActivePoint(xi, yi + 1)) activeNeighbors++;
                if (IsActivePoint(xi, yi - 1)) activeNeighbors++;
                if (IsActivePoint(xi + 1, yi + 1)) activeNeighbors++;
                if (IsActivePoint(xi + 1, yi - 1)) activeNeighbors++;
                if (IsActivePoint(xi - 1, yi + 1)) activeNeighbors++;
                if (IsActivePoint(xi - 1, yi - 1)) activeNeighbors++;

                if (activeNeighbors > 2)
                {
                    _states[PointToIndex(xi, yi)] += AppearTime * Time.deltaTime * activeNeighbors * NeighboursModifier;
                    if (IsActivePoint(xi, yi))
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

    public void OnBeamRayHit(RaycastHit hit)
    {
        var mesh = _meshCollider.sharedMesh;
        var triangles = mesh.triangles;
        var i1 = triangles[hit.triangleIndex * 3 + 0] / 2;
        var i2 = triangles[hit.triangleIndex * 3 + 1] / 2;
        var i3 = triangles[hit.triangleIndex * 3 + 2] / 2;

        var x1 = 0;
        var y1 = 0;
        CoordFromIndex(i1, out x1, out y1);
        Decrease(i1);
        Decrease(i2);
        Decrease(i3);

        Decrease(x1, y1 - 1, Splash);
        Decrease(x1, y1 + 1, Splash);
        Decrease(x1 - 1, y1, Splash);
        Decrease(x1 + 1, y1, Splash);
    }

    

    [ContextMenu("Init")]
    void Init()
    {
        _verticies.Clear();
        _states.Clear();

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

                _verticies.Add(new Vector3(x, y, 0f) - Size / 2f);
                _states.Add(1f);
                idx++;
                
                _verticies.Add(new Vector3(x, y, Size.z) - Size / 2f);
                _states.Add(1f);
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
            if(!IsActivePoint(face.X1, face.Y1) || !IsActivePoint(face.X2, face.Y2))
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
        mesh.vertices = _verticies.ToArray();

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
    
    private void CoordFromIndex(int index, out int x, out int y)
    {
        x = index / _yCount;
        y = index % _xCount;
    }

    private int PointToIndex(int x, int y)
    {
        return x * _yCount + y;
    }

    private bool IsActivePoint(int x, int y)
    {
        if (x < 0 || x >= _xCount)
            return false;
        if (y < 0 || y >= _yCount)
            return false;
        return _states[PointToIndex(x,y)] > ActiveColliderThreshold;
    }

    private bool IsActivePoint(int index)
    {
        return _states[index] > ActiveColliderThreshold;
    }

    private void Decrease(int index, float modifier = 1f)
    {
        if (IsActivePoint(index))
        {
            _states[index] -= DisappearTime * Time.deltaTime * modifier;
            if (!IsActivePoint(index))
                _rebuildRequired = true;
        }
    }

    private void Decrease(int x, int y, float modifier = 1f)
    {
        if (x < 0 || x >= _xCount)
            return;
        if (y < 0 || y >= _yCount)
            return;
        Decrease(PointToIndex(x, y));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Size);

        if (_debugMesh != null && DrawDebugMesh)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireMesh(_debugMesh, transform.position);
        }
    }
}
