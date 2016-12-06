using System;
using UnityEngine;

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
        }

        public InteractionType Interaction;
        public ProcessingMode Mode;
        public DarknessSample[] Samples;

        [Range(0f, 2f)] public float Outline = 0f;

        private MeshFilter _meshFilter;
        private Rigidbody _rigidbody;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null)
                Interaction = InteractionType.None;
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
    }

    [Serializable]
    public struct DarknessSample
    {
        public Vector3 LocalPosition;
        public Vector3 Direction;

        [Range(0f, 1f)]
        public float MassPart;
    }
}
