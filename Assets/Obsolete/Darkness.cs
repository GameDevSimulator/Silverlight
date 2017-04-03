using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Darkness : MonoBehaviour
    {
        public ComputeShader SamplingComputeShader;
        public Material ComputationalMaterial;
        public Texture InitialStateTexture;
        public Texture ShapeTexture;
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

       

        private const string StateTextureName = "_StateTex";
        private const string MaskTextureName = "_MaskTex";
        private const int RenderTextureDepth = 0;
        private const RenderTextureFormat RenderTextureFormat = UnityEngine.RenderTextureFormat.ARGB32;



        // COMPUTE SHADER SAMPLING //

        /// <summary>
        /// Structure of sample, must be eq to compute shade struct
        /// </summary>
        struct Sample
        {
            public Vector2 At;
            public float Value;
            public float Normal;
        }
        private bool _useComputeSampling; // Shader model 5.0 support (DX11)
        private const string ComputeShaderSamplingKernel = "CSSampling"; // Name of the kernel in CS
        private const string ComputeShaderSamplingStateTextue = "StateTex"; // Name of state texture in CS
        private const string ComputeShaderSamplingStateBuffere = "SampleBuffer"; // Name of Sample buffer in CS
        private const int ComputeShaderSamplingBufferSize = 50; // Also check DEFINE in compute shader
        private const int ComputeShaderStructSize = sizeof(float) * 3; // size of Sample struct
        private readonly Sample[] _gpuSamples = new Sample[ComputeShaderSamplingBufferSize]; // Input buffer
        private readonly Sample[] _gpuSamplesOut = new Sample[ComputeShaderSamplingBufferSize]; // Output buffer

#if DEBUG
        // Output rectangle for displaying debug state
        private readonly Rect _rect = new Rect(0, 0, 256, 256);
#endif

        void Start ()
        {
            if (_materialInstance == null)
                _materialInstance = Instantiate(ComputationalMaterial);

            //_meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _stateRenderTexture = new RenderTexture(512, 512, RenderTextureDepth, RenderTextureFormat);
            _stateRenderTexture.Create();

            _bufferRenderTexture = new RenderTexture(512, 512, RenderTextureDepth, RenderTextureFormat);
            _bufferRenderTexture.Create();

            // Set default state
            Graphics.Blit(InitialStateTexture ?? ComputationalMaterial.mainTexture, _stateRenderTexture);

            // Set mask
            _materialInstance.SetTexture(MaskTextureName, ShapeTexture);

            _state2D = new Texture2D(_stateRenderTexture.width, _stateRenderTexture.height)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _state2DRect = new Rect(0, 0, _state2D.width, _state2D.height);

          
            _useComputeSampling = SystemInfo.graphicsShaderLevel >= 50;

            if (SamplingComputeShader == null && _useComputeSampling)
                Debug.LogError("Missing sampling compute shader");

            // Always track players flashlight
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

                if (_useComputeSampling)
                {
                    GpuSampling();
                }
                else
                {
                    _state2D.ReadPixels(_state2DRect, 0, 0, false);
                    _state2D.Apply();
                }

                // Proccess darkness
                Graphics.Blit(_bufferRenderTexture, _stateRenderTexture, _materialInstance, 0);

                // Update visuals from state
                _meshRenderer.material.SetTexture(StateTextureName, _bufferRenderTexture);
            }


            var interactorSampleId = 1;
            foreach (var interactor in _interactors)
            {
                var body = interactor.GetComponent<Rigidbody>();
                if (body != null)
                {
                    var forceFactorSum = 0f;
                    var idx = 0;
                    
                   
                    
                    forceFactorSum = Mathf.Max(forceFactorSum, 1f);

                    if (forceFactorSum > 0)
                    {
                        idx = 0;
                       
                        
                        // Slow dow rotation
                        body.angularVelocity *= AngularDamping;
#if DEBUG
                        if (ShowDebugState)
                        {
                            Debug.DrawRay(body.transform.position, body.velocity, Color.green);
                        }
#endif
                    }

                    interactorSampleId += idx;
                }
            }
        }

        private void RenderState(Texture source, RenderTexture dest)
        {
            Graphics.SetRenderTarget(dest);
            GL.Clear(true, true, Color.black);
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
        
            /*
            foreach (var interactor in _interactors)
                interactor.DrawMesh(transform);*/
            GL.PopMatrix();
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

        private float CalcForceFactor(int sampleId, Vector3 at, Vector3 objectSurfaceNormal, Vector3 forceDirection)
        {
            //return Mathf.Max(Vector3.Dot(forceDirection, objectSurfaceNormal), 0) * GetState(at, sampleId);
            var normal = DequeueNormal(sampleId);
            return Mathf.Max(Vector3.Dot(forceDirection, new Vector3(normal, 1, 0).normalized), 0) * GetState(at, sampleId);
        }

        private float SamplePhysicsAt(int sampleId, Vector3 sample, Rigidbody body, float gravityFactor, float frictionFactor)
        {
            const float threshold = 0.1f;
            var state = GetState(sample, sampleId);

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

                var force = antiGravityforce + friction;
                body.AddForceAtPosition(force, sample, ForceMode.Force);

                //body.AddForceAtPosition(antiGravityforce, sample, ForceMode.Force);
                //body.AddForceAtPosition(friction, sample, ForceMode.Force);
            }

            return state;
        }

        private float GetState(Vector3 worldPosition, int sampleId = 0)
        {
            if (_useComputeSampling)
                return DequeueSample(sampleId);

            var point = WorldToUv(worldPosition);
            return _state2D.GetPixel((int)(point.x * _state2D.width), (int)(point.y * _state2D.height)).r;
        }

        public Vector2 WorldToUv(Vector3 worldPosition)
        {
            return transform.InverseTransformPoint(worldPosition) + new Vector3(0.5f, 0.5f);
        }

        private void QueueSample(int id, Vector3 position)
        {
            if (id > ComputeShaderSamplingBufferSize - 1)
                return;

            _gpuSamples[id].At = WorldToUv(position);
        }

        private float DequeueSample(int id)
        {
            if (id > ComputeShaderSamplingBufferSize - 1)
                return 0;
            return _gpuSamplesOut[id].Value;
        }

        private float DequeueNormal(int id)
        {
            if (id > ComputeShaderSamplingBufferSize - 1)
                return 0;
            return _gpuSamplesOut[id].Normal;
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

        private void GpuSampling()
        {
            var kernel = SamplingComputeShader.FindKernel(ComputeShaderSamplingKernel);
            var buffer = new ComputeBuffer(ComputeShaderSamplingBufferSize, ComputeShaderStructSize);
            buffer.SetData(_gpuSamples);

            SamplingComputeShader.SetTexture(kernel, ComputeShaderSamplingStateTextue, _bufferRenderTexture);
            SamplingComputeShader.SetBuffer(kernel, ComputeShaderSamplingStateBuffere, buffer);
            SamplingComputeShader.Dispatch(kernel, 2, 1, 1);
            
            buffer.GetData(_gpuSamplesOut);
            buffer.Release();
        }
    }
}
