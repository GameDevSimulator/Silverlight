using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace Assets.Scripts.Test
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class Darkness2D : MonoBehaviour
    {
        public PolygonCollider2D LightCollider;
        private PolygonCollider2D _collider;


        private float _scale = 100f;

        void Start ()
        {
            _collider = GetComponent<PolygonCollider2D>();

            var clipper = new Clipper();
            AddColliderToClipper(clipper, _collider, PolyType.ptSubject);
            AddColliderToClipper(clipper, LightCollider, PolyType.ptClip);

            var solution = new List<List<IntPoint>>();
            if (clipper.Execute(ClipType.ctXor, solution))
            {
                Debug.Log("CLIP SUCCEED");
                Debug.Log(solution[0].Count);
                ColliderFromSolution(_collider, solution);
            }
            else
            {
                Debug.Log("CLIP FAILED");
            }
        }
        
        void Update ()
        {
		
        }

        IntPoint[] Vec2Point(Vector2[] path)
        {
            var points = new IntPoint[path.Length];
            for (var i = 0; i < path.Length; i++)
            {
                points[i].X = (int)(path[i].x * _scale);
            }
            return points;
        }

        void AddColliderToClipper(Clipper clipper, PolygonCollider2D collider, PolyType polyType)
        {
            for (var i = 0; i < collider.pathCount; i++)
            {
                var pointsRaw = _collider.GetPath(i);
                var points = new IntPoint[pointsRaw.Length];
                for (var j = 0; j < pointsRaw.Length; j++)
                {
                    var t = collider.transform.TransformPoint(pointsRaw[j]) * _scale;
                    points[j].X = (int)(t.x);
                    points[j].Y = (int)(t.y);
                }
                clipper.AddPath(new List<IntPoint>(points), polyType, true);
            }
        }

        void ColliderFromSolution(PolygonCollider2D collider, List<List<IntPoint>> solution)
        {
            var pathIndex = 0;
            foreach (var path in solution)
            {
                var points = new Vector2[path.Count];

                for (var i = 0; i < path.Count; i++)
                {
                    var t = new Vector2(path[i].X, path[i].Y);
                    t = collider.transform.InverseTransformPoint(t / _scale);
                    points[i].x = t.x;
                    points[i].y = t.y;
                }

                collider.SetPath(pathIndex, points);
            }
        }
    }
}
