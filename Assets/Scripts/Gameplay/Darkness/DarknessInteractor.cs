using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Gameplay.Darkness
{
    public class DarknessInteractor : MonoBehaviour
    {
        public enum InteractionType
        {
            None,
            Light,
            Dark,
        }

        public enum ProcessingMode
        {
            MeshOnly,
            MeshWithColorData,
            MeshWithMask,
        }

        public InteractionType Interaction;
        public ProcessingMode Mode;
        public DarknessSample[] Samples;
        public Texture2D Mask;
        public Material Material;


        [Range(0f, 2f)] public float Outline = 0f;

        private MeshFilter _meshFilter;
        private Rigidbody _rigidbody;
        private Material _materialInstance;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null)
                Interaction = InteractionType.None;

            if (_materialInstance == null && Material != null)
            {
                _materialInstance = Instantiate(Material);
                _materialInstance.SetFloat("_Outline", Outline);

                if (Mask != null)
                {
                    _materialInstance.SetTexture("_MaskTex", Mask);
                }
            }
        }

        public Mesh GetMesh()
        {
            return _meshFilter.sharedMesh;
        }

        public Rigidbody GetRigidbody()
        {
            return _rigidbody;
        }

        void OnDrawGizmosSelected()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if(meshFilter != null && Outline > 0.1f)
                Gizmos.DrawWireMesh(meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale * (1.0f + Outline));

            if (Samples != null && Samples.Length > 0)
            {
                foreach (var sample in Samples)
                {
                    Gizmos.DrawWireCube(transform.TransformPoint(sample.LocalPosition), Vector3.one*0.1f);
                    Gizmos.DrawRay(transform.TransformPoint(sample.LocalPosition),
                        transform.TransformDirection(sample.Direction));
                }
            }
        }

        public void DrawMesh(Transform darknessTransform)
        {
            if (_materialInstance != null && _meshFilter != null && Interaction != InteractionType.None)
            {
                var mesh = _meshFilter.sharedMesh;
                _materialInstance.SetPass(0);
                
                var dknShift = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.1f), Quaternion.identity, Vector3.one);
                var t = dknShift * darknessTransform.worldToLocalMatrix * transform.localToWorldMatrix;

                GL.Begin(GL.TRIANGLES);
                switch (Mode)
                {
                    case ProcessingMode.MeshOnly:
                        foreach (var index in mesh.triangles)
                        {
                            if (Mode == ProcessingMode.MeshWithMask)
                                GL.TexCoord(mesh.uv[index]);
                            if (Mode == ProcessingMode.MeshWithColorData && mesh.colors.Length > 0)
                                GL.Color(mesh.colors32[index]);
                            var v = t.MultiplyPoint3x4(mesh.vertices[index]);
                            GL.Vertex3(v.x, v.y, v.z);
                        }
                        break;
                    case ProcessingMode.MeshWithColorData:
                        if (mesh.colors32.Length == 0)
                            break;
                        foreach (var index in mesh.triangles)
                        {
                            GL.Color(mesh.colors32[index]);
                            var v = t.MultiplyPoint3x4(mesh.vertices[index]);
                            GL.Vertex3(v.x, v.y, v.z);
                        }
                        break;
                    case ProcessingMode.MeshWithMask:
                        foreach (var index in mesh.triangles)
                        {
                            GL.TexCoord(mesh.uv[index]);
                            var v = t.MultiplyPoint3x4(mesh.vertices[index]);
                            GL.Vertex3(v.x, v.y, v.z);
                        }
                        break;
                }
                GL.End();
            }
        }
    }

    [Serializable]
    public struct DarknessSample
    {
        public Vector3 LocalPosition;
        public Vector3 Direction;

        [Range(0f, 1f)]
        public float MassPart;

        [HideInInspector] public float LastState;
    }
}
