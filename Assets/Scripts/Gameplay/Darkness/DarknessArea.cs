using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class DarknessArea : MonoBehaviour
    {
        public struct CollisionInfo
        {
            public const int Size = sizeof(uint) * 7;
            public const float NormalScale = 256f;
            public const float NormalHalfScale = NormalScale * 0.5f;
            
            public uint IntersectionXSum;
            public uint IntersectionYSum;
            public uint IntersectionNormalXSum;
            public uint IntersectionNormalYSum;
            public uint CountIntersection;
            public uint CountEdge;
            public uint CountAll;

            public Vector2 GetContactPoint(float w, float h)
            {
                return new Vector3(
                   (IntersectionXSum / w) / CountIntersection - 0.5f,
                   (IntersectionYSum / h) / CountIntersection - 0.5f);
            }

            public Vector3 GetNormal()
            {
                return new Vector3(
                    ((float)IntersectionNormalXSum / CountIntersection - NormalHalfScale) / NormalHalfScale,
                    ((float)IntersectionNormalYSum / CountIntersection - NormalHalfScale) / NormalHalfScale, 0);
            }
        }

        public struct DarknessCollision
        {
            public Vector3 Contact;
            public Vector3 Normal;
            public uint ObjectArea;
            public uint IntersectedArea;
            public uint EdgeArea;

            public float IntersectedFactor { get { return IntersectedArea/(float) ObjectArea; } }
        }

        public ComputeShader Compute;

        public const float PixelsPerUnit = 48;
        public const string ShaderName = "Darkness/Area";
        private static readonly Matrix4x4 QuadOffset 
            = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, Vector3.one);

        private readonly List<DarknessInteractor> _interactors = new List<DarknessInteractor>();
        private MeshRenderer _renderer;
        private RenderTexture _target;
        private bool _visible;
        private readonly Rect _previewRect = new Rect(0, 0, 256, 256);
        private CommandBuffer _commandsBuffer;

        // Compute stuff
        private uint _threadsPerGroupX;
        private uint _threadsPerGroupY;
        private uint _threadsPerGroupZ;
        private readonly CollisionInfo[] _collisions = new CollisionInfo[256];
        private readonly DarknessCollision[] _collisions2 = new DarknessCollision[256];
        private ComputeShader _compute;
        private ComputeBuffer _computeBuffer;
        private int _kernel;


        void Start ()
        {
            _renderer = GetComponent<MeshRenderer>();

            // COMPUTE SHADER
            //_compute = Instantiate(Compute);
            _compute = ComputeShader.Instantiate(Compute);

            _kernel = _compute.FindKernel("CSCollision");
            Compute.GetKernelThreadGroupSizes(_kernel, out _threadsPerGroupX, out _threadsPerGroupY, out _threadsPerGroupZ);

            var width = (int)(((int)(transform.lossyScale.x * PixelsPerUnit) / _threadsPerGroupX) * _threadsPerGroupX);
            var height = (int)(((int)(transform.lossyScale.y * PixelsPerUnit) / _threadsPerGroupY) * _threadsPerGroupY);

            _target = new RenderTexture(
                width,
                height, 0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default)
            {
                enableRandomWrite = true,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Clamp,
                name = "Darkness area",
                antiAliasing = 1,
                anisoLevel = 0,
            };
            _target.Create();

            _renderer.material.SetTexture("_MainTexture", _target);

            Debug.LogFormat("Darkness area '{0}' initialized. w={1}, h={2}", name, _target.width, _target.height);

            _commandsBuffer = new CommandBuffer { name = "Darkness Command Buffer" };
            //_camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _commandsBuffer);

            
            _computeBuffer = new ComputeBuffer(_collisions.Length, ImagePhysics.ObjectCollisionInfo.Size);
            _compute.SetTexture(_kernel, "StateTex", _target);
            _compute.SetBuffer(_kernel, "CollisionInfoBuffer", _computeBuffer);

            AddIntersectedInteractors();
            //AddInteractor(GameManager.Instance.Player.FlashLight.GetComponent<DarknessInteractor>());
        }

        void AddIntersectedInteractors()
        {
            var col = GetComponent<Collider>();
            var colliders = Physics.OverlapBox(col.bounds.center, col.bounds.extents*0.5f);
            foreach (var interactorCol in colliders)
            {
                AddInteractor(interactorCol.GetComponent<DarknessInteractor>());
            }
        }

        void Update()
        {
            if (_visible)
            {
                Graphics.ExecuteCommandBuffer(_commandsBuffer);
                GpuCompute();
            }
        }

        void FixedUpdate()
        {
            if (_visible)
            {
                _computeBuffer.GetData(_collisions);

                foreach (var interactor in _interactors)
                {
                    if (interactor.Interaction == DarknessInteractor.InteractionType.PhysicsOnly)
                    {
                        var id = interactor.BodyId;
                        var collision = _collisions[id];

                        if (collision.CountIntersection < 1)
                        {
                            if (_collisions2[id].IntersectedArea > 0)
                            {
                                _collisions2[id].IntersectedArea = 0;
                                interactor.SendMessage("OnCollisionWithDarknessExit", SendMessageOptions.DontRequireReceiver);
                            }
                            continue;
                        }

                        // Object space contact point
                        var contact = collision.GetContactPoint(_target.width, _target.height);

                        // Object space normal
                        var normal = collision.GetNormal().normalized;

                        _collisions2[id].Contact = transform.TransformPoint(contact);
                        _collisions2[id].Normal = transform.TransformDirection(normal);
                        _collisions2[id].ObjectArea = collision.CountAll;
                        _collisions2[id].IntersectedArea = collision.CountIntersection;
                        _collisions2[id].EdgeArea = collision.CountEdge;

                        interactor.SendMessage("OnCollisionWithDarkness", _collisions2[id], SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        void RebuildCommandBuffer()
        {
            _commandsBuffer.Clear();
            var view = Matrix4x4.identity;
            //view.m22 = -1f;
            view.m23 = 0f;
            var proj = Matrix4x4.Ortho(0, 1, 1, 0, -100f, 100f);
            //view = _camera.worldToCameraMatrix;
            //proj = _camera.projectionMatrix;

            _commandsBuffer.SetGlobalMatrix("_DarknessAreaTransform", proj * QuadOffset * transform.worldToLocalMatrix);
            _commandsBuffer.SetRenderTarget(_target);            

            foreach (var interactor in _interactors)
            {
                if (interactor.Renderer != null)
                    _commandsBuffer.DrawRenderer(interactor.Renderer, interactor.InteractionMaterial);
            }

            //_commandsBuffer.Blit(_target, _texture);
        }

        void GpuCompute()
        {
            Array.Clear(_collisions, 0, _collisions.Length);
            _computeBuffer.SetData(_collisions);
            _compute.Dispatch(_kernel, _target.width / (int)_threadsPerGroupX, _target.height / (int)_threadsPerGroupY, 1);
        }

        void OnBecameVisible()
        {
            _visible = true;
        }

        void OnBecameInvisible()
        {
            _visible = false;
        }

        public void AddInteractor(DarknessInteractor interactor)
        {
            if (interactor != null && !_interactors.Contains(interactor))
            {
                _interactors.Add(interactor);
                RebuildCommandBuffer();
            }
        }

        public void RemoveInteractor(DarknessInteractor interactor)
        {
            if (interactor != null && _interactors.Contains(interactor))
            {
                if (interactor.Interaction == DarknessInteractor.InteractionType.PhysicsOnly)
                {
                    if (_collisions[interactor.BodyId].CountIntersection > 0)
                        interactor.SendMessage("OnCollisionWithDarknessExit", SendMessageOptions.DontRequireReceiver);
                }


                _interactors.Remove(interactor);
                RebuildCommandBuffer();
            }
        }

        void OnTriggerEnter(Collider col)
        {
            AddInteractor(col.GetComponent<DarknessInteractor>());
        }

        void OnTriggerExit(Collider col)
        {
            RemoveInteractor(col.GetComponent<DarknessInteractor>());
        }

        void OnDestroy()
        {
            _computeBuffer.Release();
        }

        void OnGUI()
        {
#if DEBUG
            GUI.DrawTexture(_previewRect, _target, ScaleMode.ScaleToFit, false);
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
        }
    }
}
