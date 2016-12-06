using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Darkness : MonoBehaviour
    {
        public Material ComputationalMaterial;
        public float AngularDamping = 0.5f;

        [Range(1f, 20f)]
        public float Density = 1f;
        public bool ShowDebugState;

        //private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private RenderTexture _stateRenderTexture;
        private RenderTexture _bufferRenderTexture;

        private Material _materialInstance;
        private Texture2D _state2D;

        private readonly List<DarknessInteractor> _interactors = new List<DarknessInteractor>();
        private Rect _state2DRect;
        private bool _update = false;

#if DEBUG
        private readonly Rect _rect = new Rect(0, 0, 256, 256);
#endif
    
        const string StateTextureName = "_StateTex";
        const int RenderTextureDepth = 0;
        const RenderTextureFormat RenderTextureFormat = UnityEngine.RenderTextureFormat.ARGB32;

        void Awake()
        {
            if(_materialInstance == null)
                _materialInstance = Instantiate(ComputationalMaterial);
        }

        void Start ()
        {
            //_meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _stateRenderTexture = new RenderTexture(512, 512, RenderTextureDepth, RenderTextureFormat);
            _stateRenderTexture.Create();

            _bufferRenderTexture = new RenderTexture(512, 512, RenderTextureDepth, RenderTextureFormat);
            _bufferRenderTexture.Create();

            Graphics.Blit(ComputationalMaterial.mainTexture, _stateRenderTexture);

            _state2D = new Texture2D(_stateRenderTexture.width, _stateRenderTexture.height);
            _state2DRect = new Rect(0, 0, _state2D.width, _state2D.height);

            //_interactors.Add(GameManager.Instance.Player.GetComponentInChildren<DarknessInteractor>());
            var fl = GameManager.Instance.Player.GetComponentInChildren<FlashLight>();
            _interactors.Add(fl.GetComponent<DarknessInteractor>());
        }

        void FixedUpdate()
        {
            _update = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                GetComponent<Collider>().bounds);

            if (_update)
            {
                RenderState(_stateRenderTexture, _bufferRenderTexture);

                // Proccess darkness
                Graphics.Blit(_bufferRenderTexture, _stateRenderTexture, _materialInstance, 0);

                // Update visuals from state
                _meshRenderer.material.SetTexture(StateTextureName, _bufferRenderTexture);
            }


            foreach (var interactor in _interactors)
            {
                var body = interactor.GetComponent<Rigidbody>();
                if (body != null)
                {
                    var forceFactorSum = 0f;
                    
                    foreach (var sample in interactor.Samples)
                    {
                        forceFactorSum += CalcForceFactor(
                            interactor.transform.TransformPoint(sample.LocalPosition),
                            interactor.transform.TransformDirection(-sample.Direction.normalized),
                            Vector3.up);
                    }
                    
                    forceFactorSum = Mathf.Max(forceFactorSum, 1f);

                    if (forceFactorSum > 0)
                    {
                        var forceSum = 0f;
                        foreach (var sample in interactor.Samples)
                        {
                            var factor = CalcForceFactor(
                                interactor.transform.TransformPoint(sample.LocalPosition),
                                interactor.transform.TransformDirection(-sample.Direction.normalized),
                                Vector3.up);

                            forceSum += SamplePhysicsAt(
                                interactor.transform.TransformPoint(sample.LocalPosition), 
                                body,
                                factor / forceFactorSum,
                                1f / interactor.Samples.Length
                            );
                        }

                        if (forceSum > 0.1f)
                        {
                            interactor.gameObject.SendMessage("OnDarknessForceApplied", this,
                                SendMessageOptions.DontRequireReceiver);
                        }

                        // Slow dow rotation
                        body.angularVelocity *= AngularDamping;
#if DEBUG
                        if (ShowDebugState)
                        {
                            Debug.DrawRay(body.transform.position, body.velocity, Color.green);
                        }
#endif
                    }
                }
            }
        }

        private void RenderState(Texture source, RenderTexture dest)
        {
            Graphics.SetRenderTarget(dest);
            GL.Clear(false, false, Color.black);
            GL.PushMatrix();
            _materialInstance.SetTexture("_MainTex", source);
            _materialInstance.SetPass(1);
            GL.LoadOrtho();
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
        
            foreach (var interactor in _interactors)
            {
                if (interactor.Interaction == DarknessInteractor.InteractionType.Dark)
                {
                    _materialInstance.SetPass(3);
                    _materialInstance.SetFloat("_Outline", interactor.Outline);
                    RenderMesh(interactor.transform, interactor.GetMesh(), interactor.Mode);
                }


                if (interactor.Interaction == DarknessInteractor.InteractionType.Light)
                {
                    _materialInstance.SetPass(2);
                    _materialInstance.SetFloat("_Outline", interactor.Outline);
                    RenderMesh(interactor.transform, interactor.GetMesh(), interactor.Mode);
                }
            }
            GL.PopMatrix();

            // TODO: OPTIMIZE PERFORMANCE BY C++ PLUGIN OR COMPUTE SHADERS
            // TODO: REMOVE PIPELINE STALLING
            _state2D.ReadPixels(_state2DRect, 0, 0, false);
            _state2D.Apply();
        }

        private void RenderMesh(Transform t, Mesh mesh, DarknessInteractor.ProcessingMode mode)
        {
            GL.Begin(GL.TRIANGLES);
            foreach (var index in mesh.triangles)
            {
                var v = mesh.vertices[index];
                v = World2GL(t.TransformPoint(v));
                //GL.TexCoord(mesh.uv[mesh.triangles[i]]);
                if(mode == DarknessInteractor.ProcessingMode.MeshWithColorData && mesh.colors.Length > 0)
                    GL.Color(mesh.colors32[index]);
                GL.Vertex3(v.x, v.y, v.z + 0.1f);
            }
            GL.End();
        }

        private Vector3 World2GL(Vector3 position)
        {
            return transform.InverseTransformPoint(position) + new Vector3(0.5f, 0.5f);
        }

        void OnGUI()
        {
#if DEBUG
            if (ShowDebugState && _update)
            {
                GUI.DrawTexture(_rect, _stateRenderTexture, ScaleMode.ScaleToFit, false);
            }
#endif
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

        void OnTriggerStay(Collider col)
        {           
        }

        private Vector3 BottomEdgeOfCollider(Collider col, float xFactor)
        {
            var xpos = col.bounds.min.x + xFactor * col.bounds.size.x;

            RaycastHit hit;
            if (col.Raycast(new Ray(new Vector3(xpos, col.bounds.min.y - 1f), Vector3.up), out hit, 2f))
                return hit.point;

            return col.bounds.min;
        }

        private float CalcForceFactor(Vector3 at, Vector3 objectSurfaceNormal, Vector3 forceDirection)
        {
            return Mathf.Max(Vector3.Dot(forceDirection, objectSurfaceNormal), 0) * GetState(at);
        }

        private float SamplePhysicsAt(Vector3 sample, Rigidbody body, float gravityFactor, float frictionFactor)
        {
            const float threshold = 0.1f;
            var state = GetState(sample);

            if (state > threshold)
            {
                var antiGravityforce = -Physics.gravity * body.mass * state * gravityFactor;

                /*var friction = Vector3.ClampMagnitude(-Density * body.velocity.normalized * body.velocity.sqrMagnitude * state, 
                   body.velocity.magnitude * body.mass * 10);*/

                var friction = -Density * body.velocity * state * Mathf.Sqrt(body.mass);

#if DEBUG
                if (ShowDebugState)
                {
                    Debug.DrawLine(sample, sample + antiGravityforce, Color.red);
                    Debug.DrawLine(sample, sample + friction, Color.blue);
                }
#endif

                body.AddForceAtPosition(antiGravityforce + friction, sample, ForceMode.Force);
                
                //body.AddForceAtPosition(antiGravityforce, sample, ForceMode.Force);
                //body.AddForceAtPosition(friction, sample, ForceMode.Force);
            }

            return state;
        }

        private Vector3 GetStateEdgeBetween(Vector3 a, Vector3 b)
        {
            const float magnitudeThreshold = 0.1f;

            while ((b - a).magnitude > magnitudeThreshold)
            {
                var c = (a + b) * 0.5f;
                if (GetState(b) * GetState(c) < 0.5f)
                {
                    a = c;
                }
                else
                {
                    b = c;
                }
            }

            return (a + b) * 0.5f;
        }

        private float GetState(Vector3 worldPosition)
        {
            var point = transform.InverseTransformPoint(worldPosition) + new Vector3(0.5f, 0.5f);

            if (point.x < 0 || point.x > 1)
                return 0;

            if (point.y < 0 || point.y > 1)
                return 0;

            return _state2D.GetPixel((int)(point.x *_state2D.width), (int)(point.y * _state2D.height)).r;
        }

        public bool OnBeamRayHit(RaycastHit hit, Vector3 rdir, float maxDistance, out Vector3 endPoint)
        {
            Debug.DrawRay(hit.point, rdir);
            const int samples = 40;

            var sampleSize = maxDistance / samples;
            var p = hit.point;
            var d = rdir.normalized * sampleSize;
            for (var i = 0; i < samples; i++)
            {
                p += d;
                if (GetState(p) > 0.8f)
                {
                    endPoint = p;
                    return true;
                }
            }

            endPoint = Vector3.zero;
            return false;
        }
    }
}
