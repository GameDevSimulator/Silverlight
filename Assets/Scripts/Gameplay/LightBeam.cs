using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Game;
using Assets.Scripts.Gameplay.Darkness;
using UnityEditor;

[RequireComponent(typeof(MeshRenderer))]
public class LightBeam : MonoBehaviour
{
    struct MeshRay
    {
        public int StartVertex;
        public Vector3 Direction;
        public int EndVertex;
    }

    [SerializeField] public int Rays = 20;
    [SerializeField] public float Width = 1f;
    [SerializeField] public float Spread = 0.5f;
    [SerializeField] public float MaxDistance = 20f;

    private float _spread;
    private float _initialWidth;
    private int _initialRays;
    private MeshFilter _meshFilter;
    private float _maxDistance = 20f;

    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private Vector3[] _normals;
    private int[] _tris;
    private List<MeshRay> _meshRays;

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();

        // Save public variables to private
        // so no-one be able to change this during start
        _spread = Spread;
        _initialWidth = Width;
        _initialRays = Rays;
        _maxDistance = MaxDistance;
        CreateMesh();
    }

    void FixedUpdate()
    {
        UpdateMesh();
    }

    void CreateMesh()
    {
        var totalVertices = _initialRays * 2;
        var totalTris = (_initialRays - 1) * 2;

        var mesh = new Mesh();
        _meshFilter.mesh = mesh;

        _vertices = new Vector3[totalVertices];
        _uvs = new Vector2[totalVertices];
        _normals = new Vector3[totalVertices];
        _meshRays = new List<MeshRay>();

        
        for (var i = 0; i < _initialRays; i++)
        {
            
            var k = 1 - (2 * i/(float)(_initialRays - 1));
            // ray far point
            _vertices[i] = new Vector3(_maxDistance, GetRayY(k, _initialWidth, _spread), 0);
            _uvs[i] = new Vector2(i / (float)(_initialRays - 1), 1f);

            // near point       
            _vertices[_initialRays + i] = new Vector3(0, GetRayY(k, _initialWidth, 0), 0);
            _uvs[_initialRays + i] = new Vector2(i / (float)(_initialRays - 1), 0f);

            _meshRays.Add(new MeshRay
            {
                StartVertex = _initialRays + i,
                EndVertex = i,
                Direction = (_vertices[i] - _vertices[_initialRays + i]).normalized
            });
        }

        mesh.vertices = _vertices;
        //mesh.uv = _uvs;
        
        for (var i = 0; i < totalVertices; i++)
        {
            _normals[i] = -Vector3.forward;
        }

        _tris = new int[totalTris*3]; // 3 vertex index for each tri
        for (var ray = 0; ray < _initialRays - 1; ray++)
        {
            _tris[ray * 6 + 0] = ray;
            _tris[ray * 6 + 1] = ray + 1;
            _tris[ray * 6 + 2] = _initialRays + ray;

            _tris[ray * 6 + 3] = _initialRays + ray + 1;
            _tris[ray * 6 + 4] = _initialRays + ray;
            _tris[ray * 6 + 5] = ray + 1;
        }

        mesh.triangles = _tris;
    }

    private static float GetRayY(float k, float beamWidth, float spread)
    {
        return k*(beamWidth + spread);
    }

    void UpdateMesh()
    {
        var vertices = new List<Vector3>(_vertices);
        //var uvs = new List<Vector2>(_uvs);
        //var normals = new List<Vector3>(_normals);
        var tris = new List<int>(_tris);

        RayPass(_meshRays, vertices, tris);

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.triangles = tris.ToArray();
        //mesh.uv = uvs.ToArray();
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
        //_meshCollider.sharedMesh = mesh;
    }

    private void RayPass(List<MeshRay> rays, List<Vector3> vertices, List<int> tris, int limit = 4)
    {
        RaycastHit hit;
        List<MeshRay> reflected = null;

        foreach (var r in rays)
        {
            var origin = transform.TransformPoint(vertices[r.StartVertex]);
            var rdir = transform.TransformDirection(r.Direction);
            var result = Physics.Raycast(origin, rdir, out hit, _maxDistance);

            if (result)
            {
                while (hit.collider != null && hit.collider.CompareTag(Tags.Darkness))
                {
                    var dkn = hit.collider.gameObject.GetComponent<Darkness>();
                    if (dkn != null && (hit.point - transform.position).magnitude < _maxDistance)
                    {
                        dkn.OnLight();

                        if (dkn.IsRaysCanPass())
                        {
                            RaycastHit hit2;
                            result = Physics.Raycast(
                                hit.point + rdir * 0.001f, 
                                rdir, 
                                out hit2,
                                _maxDistance - (hit.point - transform.position).magnitude);

                            if (result)
                            {
                                hit = hit2;
                                continue;
                            }
                        }
                    }

                    var dknVolume = hit.collider.gameObject.GetComponent<DarknessVolumePrototype>();
                    if (dknVolume != null && (hit.point - transform.position).magnitude < _maxDistance)
                    {
                        Vector3 endPoint;
                        if (dknVolume.OnBeamRayHit(hit, rdir, out endPoint))
                        {
                            hit.point = endPoint;
                        }
                    }

                    break;
                }
                    
                if (hit.collider != null && limit > 0 && hit.collider.CompareTag(Tags.Reflector))
                {
                    if (reflected == null)
                        reflected = new List<MeshRay>();

                    var dir = Vector3.Reflect(hit.point - origin, hit.normal);
                    var reflectedDirection = transform.InverseTransformDirection(dir.normalized);

                    // add new ray vertices
                    vertices.Add(vertices[r.EndVertex] + reflectedDirection*_maxDistance);
                    //normals.Add(-Vector3.forward);
                    //normals.Add(-Vector3.forward);

                    reflected.Add(new MeshRay
                    {
                        StartVertex = r.EndVertex,
                        EndVertex = vertices.Count - 1,
                        Direction = reflectedDirection,
                    });
                }
            }

            if (result)
                vertices[r.EndVertex] = transform.InverseTransformPoint(hit.point);
            else
                vertices[r.EndVertex] = transform.InverseTransformPoint(origin + rdir * _maxDistance);

            
            //vertices[r.EndVertex] = transform.InverseTransformPoint(origin + rdir * _maxDistance);
        }

        if (reflected != null && reflected.Count > 1)
        {
            for (var i = 0; i < reflected.Count - 1; i++)
            {
                var ray = reflected[i];
                var nextRay = reflected[i + 1];

                tris.Add(ray.StartVertex);
                tris.Add(nextRay.StartVertex);
                tris.Add(ray.EndVertex);
                
                tris.Add(nextRay.StartVertex);
                tris.Add(nextRay.EndVertex);
                tris.Add(ray.EndVertex);
                
                // back face
                tris.Add(nextRay.StartVertex);
                tris.Add(ray.StartVertex);
                tris.Add(ray.EndVertex);

                tris.Add(nextRay.EndVertex);
                tris.Add(nextRay.StartVertex);
                tris.Add(ray.EndVertex);
            }

            RayPass(reflected, vertices, tris, limit - 1);
        }
    }

    void OnDrawGizmosSelected()
    {
        var top = new Vector3(0, GetRayY(1f, Width, Spread));
        var bottom = new Vector3(0, GetRayY(-1f, Width, Spread));

        var offset = new Vector3(0, Width, 0);

        Gizmos.color = Color.blue;
        
        Gizmos.DrawLine(transform.TransformPoint(offset), transform.right * MaxDistance + transform.TransformPoint(top));
        Gizmos.DrawLine(transform.TransformPoint(-offset), transform.right * MaxDistance + transform.TransformPoint(bottom));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.right * MaxDistance + transform.position);
    }
}
