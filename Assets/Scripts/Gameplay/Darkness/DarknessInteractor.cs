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

        [Range(0f, 2f)] public float Outline = 0f;

        private MeshFilter _meshFilter;

        void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null)
                Interaction = InteractionType.None;
        }

        void Update()
        {
        }

        public Mesh GetMesh()
        {
            return _meshFilter.sharedMesh;
        }

        void OnDrawGizmosSelected()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if(meshFilter != null && Outline > 0.1f)
                Gizmos.DrawWireMesh(meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale * (1.0f + Outline));
        }
    }
}
