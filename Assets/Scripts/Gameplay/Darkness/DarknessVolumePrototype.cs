using UnityEngine;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class DarknessVolumePrototype : MonoBehaviour
    {
        // Number of cells per unit
        [Range(1f, 10f)]
        public float Density = 1f;

        [Range(0.1f, 10f)]
        public float LightenSpeed = 1f;

        [Range(0.1f, 10f)]
        public float DarkenSpeed = 1f;

        [Range(0.1f, 0.9f)]
        public float NeighborThreshold = 0.5f;

        [Range(0.1f, 0.9f)]
        public float RayPassThreshold = 0.5f;

        public AnimationCurve DarkenCurve;
        public AnimationCurve RaypassCurve;

        private MeshFilter _meshFilter;
        private Collider _collider;

        private Color32[] _colors;
        private int _cellsWidth;
        private int _cellsHeight;

        private int _verticesWidth;
        private int _verticesHeight;

        private Vector3 _cellExtents;

        private bool _meshColorsUpdateRequired = false;
        private bool _stateUpdateRequired = false;

        void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _collider = GetComponent<Collider>();

            if (!_collider.isTrigger)
            {
                Debug.LogWarning("Collider is not a trigger. Fixing");
                _collider.isTrigger = true;
            }

            _cellsWidth = (int)(Density * _collider.bounds.size.x);
            _cellsHeight = (int)(Density * _collider.bounds.size.y);

            _verticesWidth = _cellsWidth + 1;
            _verticesHeight = _cellsHeight + 1;

            _cellExtents = new Vector3(_collider.bounds.size.x/_cellsWidth, _collider.bounds.size.y/_cellsHeight)/2f;

            _colors = new Color32[_verticesWidth * _verticesHeight];
            _meshFilter.mesh = CreateMesh();

            // RANDOM VERTEX COLORS (JUST FOR TESTING
            for (var x = 0; x < _cellsWidth; x++)
            {
                for (var y = 0; y < _cellsHeight; y++)
                {
                    _colors[XY2I(x, y, _verticesWidth)] = new Color32((byte)(int)(Mathf.Sin(x / 5f) * 255), 0, 0, 255);
                }
            }

            _meshColorsUpdateRequired = true;
        }

        void Update()
        {
            if (_meshColorsUpdateRequired)
            {
                _meshFilter.mesh.colors32 = _colors;
                _meshColorsUpdateRequired = false;
            }

#if DEBUG
            if (Input.GetKey(KeyCode.Alpha8))
            {
                for (var x = 0; x < _cellsWidth; x++)
                {
                    for (var y = 0; y < _cellsHeight; y++)
                    {
                        DarkenCell(x, y);
                    }
                }
            }

            if (Input.GetKey(KeyCode.Alpha9))
            {
                for (var x = 0; x < _cellsWidth; x++)
                {
                    for (var y = 0; y < _cellsHeight; y++)
                    {
                        LightCell(x, y);
                    }
                }
            }
#endif
        }

        void FixedUpdate()
        {
            for (var x = 0; x < _cellsWidth; x++)
            {
                for (var y = 0; y < _cellsHeight; y++)
                {
                    var val = DarkenCurve.Evaluate(PeripherySum(x, y) / 10f);
                    if (val < NeighborThreshold)
                    {
                        DarkenCell(x, y, val);
                        _meshColorsUpdateRequired = true;
                    }
                }
            }

        }

        public bool OnBeamRayHit(RaycastHit hit, Vector3 direction, out Vector3 endPoint)
        {
            var dir = direction.normalized;
            endPoint = BoundsIntersect(_collider.bounds, hit.point, hit.point + direction*100f);
            
            //Debug.DrawLine(hit.point, endPoint);

            var x1 = 0;
            var y1 = 0;
            V2XY(transform.InverseTransformPoint(hit.point + _cellExtents), out x1, out y1, _cellsWidth, _cellsHeight);

            var x2 = 0;
            var y2 = 0;
            V2XY(transform.InverseTransformPoint(endPoint + _cellExtents), out x2, out y2, _cellsWidth, _cellsHeight);

            LightLine(x1, y1, x2, y2, 0.2f);
            var d1 = transform.TransformPoint(XY2V(x1, y1, _cellsWidth, _cellsHeight));
            var d2 = transform.TransformPoint(XY2V(x2, y2, _cellsWidth, _cellsHeight));
            Debug.DrawLine(d1, d2, Color.green);
            Debug.DrawLine(d1, d1 + Vector3.back, Color.green);

            return true;
        }

        void OnDrawGizmos()
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();

            if (_collider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(_collider.bounds.center, _collider.bounds.size);
            }
        }

        private float CellState(int x, int y)
        {
            var sum = _colors[XY2I(x, y, _verticesWidth)].r;
            sum += _colors[XY2I(x, y + 1, _verticesWidth)].r;
            sum += _colors[XY2I(x + 1, y, _verticesWidth)].r;
            sum += _colors[XY2I(x + 1, y + 1, _verticesWidth)].r;
            return sum * 0.25f / 255f;
        }

        private float PeripherySum(int x, int y)
        {
            var sum = 0f;

            if (x > 0)
            {
                sum += _colors[XY2I(x - 1, y, _verticesWidth)].r;

                if (y > 0)
                    sum += _colors[XY2I(x - 1, y - 1, _verticesWidth)].r;

                if (y < _verticesHeight - 1)
                    sum += _colors[XY2I(x - 1, y + 1, _verticesWidth)].r;

                if (y < _verticesHeight - 2)
                    sum += _colors[XY2I(x - 1, y + 2, _verticesWidth)].r;
            }

            if (y > 0)
                sum += _colors[XY2I(x, y - 1, _verticesWidth)].r;

            if (y < _verticesHeight - 2)
                sum += _colors[XY2I(x, y + 2, _verticesWidth)].r;

            if (x < _verticesWidth - 1)
            {
                if (y > 0)
                    sum += _colors[XY2I(x + 1, y - 1, _verticesWidth)].r;

                if (y < _verticesHeight - 2)
                    sum += _colors[XY2I(x + 1, y + 2, _verticesWidth)].r;
            }

            if (x < _verticesWidth - 2)
            {
                if (y > 0)
                    sum += _colors[XY2I(x + 2, y - 1, _verticesWidth)].r;

                sum += _colors[XY2I(x + 2, y, _verticesWidth)].r;

                if (y < _verticesHeight - 1)
                    sum += _colors[XY2I(x + 2, y + 1, _verticesWidth)].r;

                if (y < _verticesHeight - 2)
                    sum += _colors[XY2I(x + 2, y + 2, _verticesWidth)].r;
            }

            return sum / 255f;
        }

        private void DarkenCell(int x, int y, float modifier = 1f)
        {
            var amount = (255) * DarkenSpeed * Time.deltaTime * modifier;
            _colors[XY2I(x, y, _verticesWidth)].r = (byte)Mathf.Max(0, _colors[XY2I(x, y, _verticesWidth)].r - amount);
            _colors[XY2I(x, y + 1, _verticesWidth)].r = (byte)Mathf.Max(0, _colors[XY2I(x, y + 1, _verticesWidth)].r - amount);
            _colors[XY2I(x + 1, y, _verticesWidth)].r = (byte)Mathf.Max(0, _colors[XY2I(x + 1, y, _verticesWidth)].r - amount);
            _colors[XY2I(x + 1, y + 1, _verticesWidth)].r = (byte)Mathf.Max(0, _colors[XY2I(x + 1, y + 1, _verticesWidth)].r - amount);
        }

        private void LightCell(int x, int y, float modifier = 1f)
        {
            var amount = (255) * LightenSpeed * Time.deltaTime * modifier;
            _colors[XY2I(x, y, _verticesWidth)].r = (byte)Mathf.Min(255, _colors[XY2I(x, y, _verticesWidth)].r + amount);
            _colors[XY2I(x, y + 1, _verticesWidth)].r = (byte)Mathf.Min(255, _colors[XY2I(x, y + 1, _verticesWidth)].r + amount);
            _colors[XY2I(x + 1, y, _verticesWidth)].r = (byte)Mathf.Min(255, _colors[XY2I(x + 1, y, _verticesWidth)].r + amount);
            _colors[XY2I(x + 1, y + 1, _verticesWidth)].r = (byte)Mathf.Min(255, _colors[XY2I(x + 1, y + 1, _verticesWidth)].r + amount);
        }

        private void LightLine(int x1, int y1, int x2, int y2, float modifier = 1f)
        {
            var len = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
            var steps = Mathf.Max(_cellsWidth, _cellsHeight);
            var dx = (x2 - x1) / (float)steps;
            var dy = (y2 - y1) / (float)steps;
            var x = (float)x1;
            var y = (float)y1;

            var mod = len / steps;
            var currentMod = mod;

            for (var i = 0; i < steps; i++)
            {
                //var xx = Mathf.RoundToInt(x);
                //var yy = Mathf.RoundToInt(y);
                var xx = Mathf.FloorToInt(x);
                var yy = Mathf.FloorToInt(y);

                LightCell(xx, yy, currentMod * modifier);
                x += dx;
                y += dy;

                if(x < 0 || x > _cellsWidth - 1)
                    break;

                if (y < 0 || y > _cellsHeight - 1)
                    break;

                currentMod = currentMod * RaypassCurve.Evaluate(1f - CellState(xx, yy));
            }
        }

        private static int XY2I(int x, int y, int width)
        {
            return y * width + x;
        }

        private static void I2XY(int index, int width, int height, out int x, out int y)
        {
            x = index % width;
            y = index / height;
        }

        private Vector3 XY2V(int x, int y, float width, float height)
        {
            return new Vector3(x / width * _collider.bounds.size.x, y / height * _collider.bounds.size.y) - new Vector3(_collider.bounds.extents.x, _collider.bounds.extents.y);
        }

        private void V2XY(Vector3 pos, out int x, out int y, float width, float height)
        {
            var relPos = new Vector3(_collider.bounds.extents.x, _collider.bounds.extents.y) + pos;
            x = Mathf.FloorToInt(relPos.x * width / _collider.bounds.size.x + 0.01f);
            y = Mathf.FloorToInt(relPos.y * height / _collider.bounds.size.y + 0.01f);
        }

        private static Vector3 BoundsIntersect(Bounds bounds, Vector3 inside, Vector3 outside)
        {
            var dir = outside - inside;
            dir.Normalize();

            var distance = 0f;
            bounds.IntersectRay(new Ray(outside, -dir), out distance);

            return outside - dir * distance;
        }

        private Mesh CreateMesh()
        {
            var vertices = new Vector3[_verticesWidth * _verticesHeight];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var tris = new int[_verticesWidth * _verticesHeight * 2 * 3];

            for (var x = 0; x < _verticesWidth; x++)
            {
                for (var y = 0; y < _verticesHeight; y++)
                {
                    var i = XY2I(x, y, _verticesWidth);
                    vertices[i] = XY2V(x, y, _cellsWidth, _cellsHeight);
                    normals[i] = -Vector3.forward;
                    uvs[i] = new Vector2(x / (float)_cellsWidth, y / (float)_cellsHeight);

                    if (x < _cellsWidth && y < _cellsHeight)
                    {
                        tris[i * 6 + 0] = XY2I(x, y, _verticesWidth);
                        tris[i * 6 + 2] = XY2I(x + 1, y, _verticesWidth);
                        tris[i * 6 + 1] = XY2I(x, y + 1, _verticesWidth);

                        tris[i * 6 + 3] = XY2I(x + 1, y, _verticesWidth);
                        tris[i * 6 + 4] = XY2I(x, y + 1, _verticesWidth);
                        tris[i * 6 + 5] = XY2I(x + 1, y + 1, _verticesWidth);
                    }
                }
            }

            var mesh = new Mesh()
            {
                name = "Darkness Mesh",
                vertices = vertices,
                uv = uvs,
                triangles = tris,
            };

            mesh.RecalculateBounds();

            return mesh;
        }

        private void OnTriggerStay(Collider collider)
        {
            /*
            var x = 0;
            var y = 0;
            V2XY(transform.InverseTransformPoint(collider.bounds.center), out x, out y, _cellsWidth, _cellsHeight);

            if (x < _cellsWidth && y < _cellsHeight && x > 0 && y > 0)
                LightCell(x, y, 10f);*/

            //if (collider.attachedRigidbody)
                //collider.attachedRigidbody.AddForce(Vector3.up * 20f);

            var minx = 0;
            var miny = 0;
            V2XY(transform.InverseTransformPoint(collider.bounds.min), out minx, out miny, _cellsWidth, _cellsHeight);

            if (minx < _cellsWidth && miny < _cellsHeight && minx > 0 && miny > 0)
            {
                if (collider.attachedRigidbody.velocity.y < 0)
                {
                    collider.attachedRigidbody.velocity = new Vector3(collider.attachedRigidbody.velocity.x, 0,
                        collider.attachedRigidbody.velocity.z);

                    collider.attachedRigidbody.AddForce(Vector3.up * 200f);
                }
            }
        }
    }
}
