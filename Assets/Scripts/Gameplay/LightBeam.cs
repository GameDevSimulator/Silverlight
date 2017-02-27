using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
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
        private Color32[] _colors;
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
            _colors = new Color32[totalVertices];
            _uvs = new Vector2[totalVertices];
            _normals = new Vector3[totalVertices];
            _meshRays = new List<MeshRay>();

        
            for (var i = 0; i < _initialRays; i++)
            {
            
                var k = 1 - (2 * i/(float)(_initialRays - 1));
                // ray far point
                _vertices[i] = new Vector3(_maxDistance, GetRayY(k, _initialWidth, _spread), 0);
                _uvs[i] = new Vector2(i / (float)(_initialRays - 1), 1f);
                _colors[i] = new Color32(0, 0, 0, 255);

                // near point       
                _vertices[_initialRays + i] = new Vector3(0, GetRayY(k, _initialWidth, 0), 0);
                _uvs[_initialRays + i] = new Vector2(i / (float)(_initialRays - 1), 0f);
                _colors[_initialRays + i] = new Color32(255, 255, 255, 255);

                _meshRays.Add(new MeshRay
                {
                    StartVertex = _initialRays + i,
                    EndVertex = i,
                    Direction = (_vertices[i] - _vertices[_initialRays + i]).normalized
                });
            }

            mesh.vertices = _vertices;
            mesh.uv = _uvs;
        
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

            var mesh = _meshFilter.mesh;
            mesh.SetVertices(vertices);
            mesh.colors32 = _colors;
            mesh.triangles = tris.ToArray();
            //mesh.uv = uvs.ToArray();
            //mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
        
            //_meshFilter.sharedMesh = mesh;
            //_meshCollider.sharedMesh = mesh;
        }

        private void RayPass(List<MeshRay> rays, List<Vector3> vertices, List<int> tris, int limit = 4)
        {
            //RaycastHit hit;
            //List<MeshRay> reflected = null;

            foreach (var r in rays)
            {
                var origin = transform.TransformPoint(vertices[r.StartVertex]);
                var rdir = transform.TransformDirection(r.Direction);
                var result = RayCast(origin, rdir, _maxDistance);

                /*
                    
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
                }*/

                vertices[r.EndVertex] = transform.InverseTransformPoint(result.Point);
                //_colors[r.StartVertex].r = (byte)(int)(0f);
                _colors[r.EndVertex].r = (byte) (int) (255f * Mathf.Max(1f - result.Distance/MaxDistance, 0f));

                //_colors[r.StartVertex] = new Color32(0,0,0,0);
                //_colors[r.EndVertex] = new Color32(255, 255, 255, 255);
            }

            /*
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
        }*/
        }

        public struct RayCastResult
        {
            public bool Collided;
            public Vector3 Point;
            public float Distance;
        }

        private RayCastResult RayCast(Vector3 origin, Vector3 direction, float maxDistance)
        {
            var result = new RayCastResult() { Distance = maxDistance, Point = origin + direction * maxDistance};
            if (maxDistance < 0.1f)
                return result;

            RaycastHit hit;
            if (Physics.Raycast(new Ray(origin, direction), out hit, maxDistance))
            {
                result.Point = hit.point;
                result.Collided = true;
                result.Distance = (result.Point - origin).magnitude;

                if (hit.collider.CompareTag(WellKnown.Tags.Darkness))
                {
                    //result.Collided = false;
                    var darkness = hit.collider.gameObject.GetComponent<Darkness.Darkness>();
                    if (darkness != null)
                    {
                        if (maxDistance - result.Distance > 0.1f)
                        {
                            Vector3 endPoint;
                            if (darkness.OnBeamRayHit(hit, direction, maxDistance - result.Distance, out endPoint))
                            {
                                result.Collided = true;
                                result.Point = endPoint;
                                result.Distance = (result.Point - origin).magnitude;
                            }
                            else
                            {
                                result.Collided = false;
                                result.Point = origin + direction*maxDistance;
                                result.Distance = maxDistance;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private void OnDrawGizmosSelected()
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
}
