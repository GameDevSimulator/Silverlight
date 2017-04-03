using System;
using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Collider))]
    public class ImagePhysics : MonoBehaviour
    {
        public ComputeShader CollisionComputeShader;

        [Range(0f, 10f)]
        public float Depenetration = 1f;

        private MeshRenderer _meshRenderer;

        private RenderTexture _stateTexture;
        private readonly List<DarknessInteractor> _interactors = new List<DarknessInteractor>();

        private const int ThreadsPerGroupX = 8;
        private const int ThreadsPerGroupY = 8;
        private const int RenderTextureWidth = 512;
        private const int RenderTextureHeight = 512;
        private const int RenderTextureDepth = 0;
        private const RenderTextureFormat RenderTextureFormat = UnityEngine.RenderTextureFormat.ARGB32;

        private const int MinPixels = 3;
        

        public struct ObjectCollisionInfo
        {
            public const float NormalScale = 256f;
            public const float NormalHalfScale = NormalScale * 0.5f;

            public Vector2 dNormal;
            public Vector2 dPoint;
            public uint d;
            public uint IntersectionXSum;
            public uint IntersectionYSum;
            public uint IntersectionNormalXSum;
            public uint IntersectionNormalYSum;
            public uint CountIntersection;
            public uint CountEdge;
            public uint CountAll;

            public Vector2 ContactPoint()
            {
                return new Vector2(
                    ((float)IntersectionXSum / RenderTextureWidth) / CountIntersection - 0.5f, 
                    ((float)IntersectionYSum / RenderTextureHeight) / CountIntersection - 0.5f);
            }

            public Vector3 GetNormal()
            {
                return new Vector3(
                    ((float)IntersectionNormalXSum / CountIntersection - NormalHalfScale) / NormalHalfScale, 
                    ((float)IntersectionNormalYSum / CountIntersection - NormalHalfScale) / NormalHalfScale, 0);
            }

            public Vector3 GetPenetrationNormal()
            {
                return new Vector3(dNormal.x, dNormal.y, 0);
            }

            public Vector3 GetPenetrationPoint()
            {
                return new Vector3(dPoint.x / RenderTextureWidth - 0.5f, dPoint.y / RenderTextureHeight - 0.5f, 0);
            }

            public float GetPenetrationDepth()
            {
                return (float)d/RenderTextureWidth;
            }

            public const int Size = sizeof (uint)*8 + sizeof(float) * 4;
        };
        private ObjectCollisionInfo[] _collisions = new ObjectCollisionInfo[256];
        private ComputeBuffer _buffer;
        private int _kernel;


#if DEBUG
        // Output rectangle for displaying debug state
        private readonly Rect _rect = new Rect(0, 0, 256, 256);
#endif

        void Start ()
        {
            _meshRenderer = GetComponent<MeshRenderer>();

            _stateTexture = new RenderTexture(RenderTextureWidth, RenderTextureHeight,
                RenderTextureDepth, RenderTextureFormat)
            {
                //useMipMap = false,
                //antiAliasing = 1,
                //autoGenerateMips = false,
                //anisoLevel = 0,
                //filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                enableRandomWrite = true
            };
            
            _stateTexture.Create();



            _kernel = CollisionComputeShader.FindKernel("CSCollision");
            _buffer = new ComputeBuffer(_collisions.Length, ObjectCollisionInfo.Size);
            CollisionComputeShader.SetTexture(_kernel, "StateTex", _stateTexture);
            CollisionComputeShader.SetBuffer(_kernel, "CollisionInfoBuffer", _buffer);

            
            var flashlight = GameManager.Instance.Player.FlashLight.GetComponent<DarknessInteractor>();
            if (flashlight != null)
            {
                Debug.LogFormat("Auto added flashlight to darkness: {0}", flashlight);
                _interactors.Add(flashlight);
            }
        }
        
        void Update()
        {
        }

        void FixedUpdate()
        {
            GpuCompute();
            RenderState(_stateTexture);
            _meshRenderer.material.SetTexture("_StateTex", _stateTexture);
            ApplyCollisions();
        }
        

        private void RenderState(RenderTexture dest)
        {
            Graphics.SetRenderTarget(dest);
            //GL.Clear(true, true, Color.black);
            GL.PushMatrix();
            
            //_materialInstance.SetTexture("_MainTex", source);
            //_normalMatInstance.SetPass(0);
            GL.LoadOrtho();

            /*
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);

            // Darkness quad
            GL.TexCoord(new Vector3(0, 0));
            GL.Vertex3(0, 0, 0);

            GL.TexCoord(new Vector3(0, 1));
            GL.Vertex3(0, 1, 0);

            GL.TexCoord(new Vector3(1, 1));
            GL.Vertex3(1, 1, 0);

            GL.TexCoord(new Vector3(1, 0));
            GL.Vertex3(1, 0, 0);

            GL.End();
            */

            /*
            for (var i = 0; i < _interactors.Count; i++)
                _interactors[i].DrawMesh(transform, i + 1);*/
            
            GL.PopMatrix();
        }


        void OnTriggerEnter(Collider col)
        {
            var interactor = col.GetComponent<DarknessInteractor>();
            if (interactor != null && !_interactors.Contains(interactor))
            {
                _interactors.Add(interactor);
            }
        }

        void OnTriggerExit(Collider col)
        {
            var interactor = col.GetComponent<DarknessInteractor>();
            if (interactor != null && _interactors.Contains(interactor))
                _interactors.Remove(interactor);
        }

        private Vector3 InverseComponents(Vector3 v)
        {
            return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
        }

        private Vector3 MultiplyComponents(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        private void ApplyCollisions()
        {
            _buffer.GetData(_collisions);

            // https://en.wikipedia.org/wiki/Collision_response#Impulse-Based_Reaction_Model
            for (var i = 0; i < _interactors.Count; i++)
            {
                var c = _collisions[i + 1];
                if (c.CountIntersection < MinPixels)
                    continue;

                var interactor = _interactors[i];
                if (interactor != null)
                {
                    var body = interactor.GetRigidbody();
                    if (body == null)
                        continue;

                    var intersectedMassFactor = c.CountIntersection / (float)c.CountEdge; // / (float)c.CountAll;
                    var contact = transform.TransformPoint(c.ContactPoint());
                    var n = transform.TransformDirection(c.GetNormal()).normalized;
                    var v = body.GetPointVelocity(contact);
                    var r = contact - body.worldCenterOfMass;
                    var e = 0.0f; // RESTITUTION


                    
                    //var dp = transform.TransformPoint(c.GetPenetrationPoint());
                    //Debug.DrawRay(dp, transform.TransformVector(-c.GetPenetrationNormal().normalized * c.GetPenetrationDepth()), Color.magenta, 1f);

                    // LINEAR MODEL
                    //var jLinear = Mathf.Max(-(1 + e) * Vector3.Dot(body.velocity * body.mass, n), 0);
                    //body.AddForce(jLinear * n, ForceMode.Impulse);
                    //continue;


                    //var a2 = MultiplyComponents(Iwi, a1);

                    var inertiatensor3 = body.inertiaTensorRotation*body.inertiaTensor;
                    var inverseBodyTensor = new Matrix4x4
                    {
                        m00 = 1f / 1f, 
                        m11 = 1f / 1f,
                        m22 = 1f / inertiatensor3.z,
                        m33 = 1.0f
                    };
                    
                    var rotation = new Matrix4x4();
                    rotation.SetTRS(Vector3.zero, body.rotation, Vector3.one);
                    
                    var inverseInertiaTensorWorld = rotation * inverseBodyTensor * rotation.transpose;
                    // parallel axis theorem
                    var inverseInertiaTensorWithOffset = new Matrix4x4
                    {
                        m00 = inverseInertiaTensorWorld.m00,
                        m01 = inverseInertiaTensorWorld.m01 - r.x * r.y * body.mass,
                        m02 = inverseInertiaTensorWorld.m02 - r.x * r.z * body.mass,

                        m10 = inverseInertiaTensorWorld.m10 - r.y * r.x * body.mass,
                        m11 = inverseInertiaTensorWorld.m11,
                        m12 = inverseInertiaTensorWorld.m12 - r.y * r.z * body.mass,

                        m20 = inverseInertiaTensorWorld.m20 - r.z * r.x * body.mass,
                        m21 = inverseInertiaTensorWorld.m21 - r.z * r.y * body.mass,
                        m22 = inverseInertiaTensorWorld.m22,
                        m33 = 1f
                    };
                    print(inverseInertiaTensorWithOffset);

                    //var iIz = (body.rotation*body.inertiaTensorRotation*body.inertiaTensor).z;
                    //var iIz = (body.rotation*body.inertiaTensorRotation*body.inertiaTensor).z + body.mass * transform.InverseTransformVector(r).sqrMagnitude;
                    //var iIz = (body.inertiaTensorRotation * body.inertiaTensor).z - body.mass * r.sqrMagnitude;
                    //var iIz = InverseComponents(body.inertiaTensorRotation*body.inertiaTensor);
                    var iIz = r.sqrMagnitude * body.mass;
                    var a2 = Vector3.Cross(r, n)/iIz;

                    //var a2 = inverseInertiaTensorWithOffset.MultiplyPoint(Vector3.Cross(r, n));
                    //var a2 = inverseInertiaTensorWorld.MultiplyPoint(Vector3.Cross(r, n));
                    //var a2 = MultiplyByInverseInertiaTensorAt(body, contact, Vector3.Cross(r, n));

                    //Debug.DrawRay(contact, a2 * 5, Color.blue);

                    var a4 = Vector3.Dot(Vector3.Cross(a2, r), n);


                    // PENETRATION CORRECTION
                    //body.transform.Translate(transform.TransformVector(-c.GetPenetrationNormal().normalized * c.GetPenetrationDepth()), Space.World);
                    //body.transform.Translate(n * intersectedMassFactor * Time.fixedDeltaTime * 100f, Space.World);

                    if(c.CountEdge > 0f)
                        body.transform.Translate(n * (Depenetration * intersectedMassFactor / RenderTextureWidth), Space.World);

                    // reaction impulse magnitude
                    var jr = Mathf.Max((-(1 + e) * Vector3.Dot(v, n)) / (1f / body.mass + a4), 0);
                    body.AddForceAtPosition(jr * n, contact, ForceMode.Impulse);

                    // update particle velocity at contact
                    //v = v + jr * n / body.mass;


                    // DYNAMIC FRICTION
                    // tangent (based on velocity direction)
                    var t = (v - Vector3.Dot(v, n) * n).normalized;

                    // dynamic friction impulse magnitude
                    var jd = 0.5f * jr; 

                    // static friction 
                    var js = 0.5f * jr;

                    
                    var dot = Vector3.Dot(body.mass*v, t);
                    if (Mathf.Abs(Vector3.Dot(v, t)) < Mathf.Epsilon && dot <= js)
                    {
                        // static friction impulse
                        var jf = -dot*t;
                        body.AddForceAtPosition(jf, contact, ForceMode.Impulse);
                        Debug.DrawRay(contact, jf, Color.red, 1f);
                    }
                    else
                    {
                        // dynamic friction impulse
                        var jf = -jd*t;

                        body.AddForceAtPosition(jf, contact, ForceMode.Impulse);
                        Debug.DrawRay(contact, jf, Color.cyan);
                    }
                }
            }
        }

        private Vector3 MultiplyByInverseInertiaTensorAt(Rigidbody body, Vector3 contactPoint, Vector3 b)
        {
            var inertiaTensorReal = body.inertiaTensorRotation*body.inertiaTensor;
            var r = contactPoint - body.transform.TransformPoint(body.centerOfMass);

            //  inertiaTensor at contact point
            var i11 = inertiaTensorReal.x + body.mass * (r.sqrMagnitude - r.x * r.x);
            var i12 = body.mass * (-r.x * r.y);
            var i13 = body.mass * (-r.x * r.z);

            var i21 = body.mass * (-r.y * r.x);
            var i22 = inertiaTensorReal.y + body.mass * (r.sqrMagnitude - r.y * r.y);
            var i23 = body.mass * (-r.y * r.z);

            var i31 = body.mass * (-r.z * r.x);
            var i32 = body.mass * (-r.z * r.y);
            var i33 = inertiaTensorReal.z + body.mass * (r.sqrMagnitude - r.z * r.z);

            var detf = 1 / (i11*i22*i33 + i21*i32*i13 + i31*i12*i23 - i11*i32*i23 - i31*i22*i13 - i21*i12*i33);

            // Inverse inertiaTensor at contact point
            var a11 = detf * (i22 * i33 - i23 * i32);
            var a12 = detf * (i13 * i32 - i12 * i33);
            var a13 = detf * (i12 * i23 - i13 * i22);

            var a21 = detf * (i23 * i31 - i21 * i33);
            var a22 = detf * (i11 * i33 - i13 * i31);
            var a23 = detf * (i13 * i21 - i11 * i23);

            var a31 = detf * (i21 * i32 - i22 * i31);
            var a32 = detf * (i12 * i31 - i11 * i32);
            var a33 = detf * (i11 * i22 - i12 * i21);

            return new Vector3(
                a11 * b.x + a12 * b.y + a13 * b.z,
                a21 * b.x + a22 * b.y + a23 * b.z,
                a31 * b.x + a32 * b.y + a33 * b.z);
        }

        private Vector3 TangentFromNormal(Vector3 normal)
        {
            var tangent = Vector3.Cross(normal, Vector3.forward);

            if (tangent.magnitude < Mathf.Epsilon)
                tangent = Vector3.Cross(normal, Vector3.up);
            
            return tangent;
        }

        private void GpuCompute()
        {
            Array.Clear(_collisions, 0, _collisions.Length);
            _buffer.SetData(_collisions);
            CollisionComputeShader.Dispatch(_kernel, RenderTextureWidth / ThreadsPerGroupX, RenderTextureHeight / ThreadsPerGroupY, 1);
        }

        void OnDestroy()
        {
            _buffer.Release();
        }

        void OnGUI()
        {
#if DEBUG
            GUI.DrawTexture(_rect, _stateTexture, ScaleMode.ScaleToFit, false);
#endif
        }

        public static Color GizmoColor = new Color(0, 0, 0, 0.1f);
        public static Color GizmoOutline = new Color(0f, 1f, 1f, 0.8f);

        void OnDrawGizmos()
        {
            Gizmos.color = GizmoOutline;
            Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);

            Gizmos.color = GizmoColor;
            Gizmos.DrawCube(transform.position, GetComponent<Collider>().bounds.size);
            

            Gizmos.color = Color.blue;
            for (var i = 1; i < _collisions.Length; i++)
            {
                var c = _collisions[i];
                var p = transform.TransformPoint(c.ContactPoint());
                Gizmos.DrawSphere(p, 0.05f);

                if (c.CountIntersection > MinPixels)
                {
                    Gizmos.color = Color.green;
                    Debug.DrawRay(p, transform.TransformDirection(c.GetNormal()).normalized, Color.green);
                }

                if (c.CountAll > 0)
                {
                    DrawString(string.Format("A:{0} I:{1} E:{2}", c.CountAll, c.CountIntersection, c.CountEdge), p, Color.black);
                }
            }
        }

        static void DrawString(string text, Vector3 worldPos, Color? color = null)
        {
            UnityEditor.Handles.BeginGUI();
            if (color.HasValue)
                GUI.color = color.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            var screenPos = view.camera.WorldToScreenPoint(worldPos);
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
            UnityEditor.Handles.EndGUI();
        }
    }
}
