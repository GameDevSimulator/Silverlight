using UnityEngine;

namespace Assets.Scripts.Utility
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonRenderer : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            var col = GetComponent<PolygonCollider2D>();
            if(col == null)
                return;

            Gizmos.color = Color.magenta;

            for (var i = 0; i < col.pathCount; i++)
            {
                var path = col.GetPath(i);
                for (var j = 1; j < path.Length; j++)
                {
                    var p1 = transform.TransformPoint(path[j - 1]);
                    var p2 = transform.TransformPoint(path[j]);
                    Gizmos.DrawLine(p1, p2);
                }

                Gizmos.DrawLine(transform.TransformPoint(path[0]), 
                    transform.TransformPoint(path[path.Length - 1]));
            }
        }
    }
}
