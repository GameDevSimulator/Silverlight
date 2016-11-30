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
        public float Friction = 0.9f;
        public float VelocityDamping = 5f;
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

            _interactors.Add(GameManager.Instance.Player.GetComponentInChildren<DarknessInteractor>());
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

            // TODO: OPTIMIZE PERFORMANCE BY C++ PLUGIN
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
            if (interactor != null)
                _interactors.Add(interactor);
        }

        void OnTriggerExit(Collider col)
        {
            var interactor = col.GetComponent<DarknessInteractor>();
            if (interactor != null)
                _interactors.Remove(interactor);
        }

        void OnTriggerStay(Collider col)
        {
            if (col.attachedRigidbody != null)
            {
                var sample = 0f;
                sample += SamplePhysicsAt(col, 0.1f, 0.5f);
                sample += SamplePhysicsAt(col, 0.9f, 0.5f);
                if (sample > 0.1f)
                {
                    col.gameObject.SendMessage("OnDarknessForceApplied", this, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        private float SamplePhysicsAt(Collider col, float xFactor, float mod)
        {
            var xpos = col.bounds.min.x + xFactor * col.bounds.size.x;

            RaycastHit hit;
            if (col.Raycast(new Ray(new Vector3(xpos, col.bounds.min.y - 1f), Vector3.up), out hit, 2f))
            {
                //var center = new Vector3(xpos, col.bounds.max.y);
                //var bottom = new Vector3(xpos, col.bounds.min.y);
                var sample = hit.point;
                //var h = (sample - center).magnitude / (bottom - center).magnitude;
                //h = Mathf.Max(h, 1f);

                var forceFactor = GetState(sample);

                if (forceFactor > 0.1f)
                {
                    //col.attachedRigidbody.AddForce(-col.attachedRigidbody.velocity * forceFactor, ForceMode.VelocityChange);
                    //col.attachedRigidbody.velocity = col.attachedRigidbody.velocity*(1 - forceFactor);
                
                    var uplift = -Physics.gravity * col.attachedRigidbody.mass * forceFactor - col.attachedRigidbody.velocity * VelocityDamping;
                    var friction = new Vector3(-col.attachedRigidbody.velocity.x * Friction, 0);
                    Debug.DrawLine(sample, sample + uplift, Color.red);
                    col.attachedRigidbody.AddForceAtPosition(uplift * mod + friction * mod, sample);
                    col.attachedRigidbody.angularVelocity *= AngularDamping;
                }


                return forceFactor;
            }

            return 0f;
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
