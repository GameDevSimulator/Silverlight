using UnityEngine;
using ClipperLib;

namespace Assets.Scripts.Test
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class Light2D : MonoBehaviour
    {
        private PolygonCollider2D _collider;

        void Start()
        {
            _collider = GetComponent<PolygonCollider2D>();
        }
        
        void Update()
        {
		
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.LogFormat("Collision with {0}", collision.gameObject);
        }

        void OnDrawGizmos()
        {

        }
    }
}
