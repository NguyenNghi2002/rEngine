using System.Collections.Generic;
using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Linq;
using Engine;

namespace Engine
{
    public class TrailSystem
    {
        struct TrailSegment
        {
            public Vector2 p1, p2, p3, p4;
            public Rectangle src;
        }
        private Vector2 _position;
        private List<TrailSegment> trailSegs;
        private List<Vector2> _points = new List<Vector2>();

        public int SegmentCount;
        public float SegmentLength;
        public float LineWidth;

        //delete a segment
        private float DeathDueTime = 2.447321f;
        private float deathTimer = 0;

        public TrailSystem(Vector2 startPos)
        {
            SegmentLength = 20f;
            SegmentCount = 20;
            LineWidth = 10;

            trailSegs = new List<TrailSegment>(SegmentCount * 2);
            _points.Add(startPos);
        }
        public void SetPosition(Vector2 pos)
        {
            if (pos == _position) return;

            if (_points.Count < 2)
            {
                PushPoint(pos);
                return;
            }
            while (Vector2.Distance(_points.Last(), pos) > SegmentLength)
            {
                var dir = pos - _points.Last();
                PushPoint(_points.Last() + Vector2.Normalize(dir) * SegmentLength);
                if (_points.Count > SegmentCount - 1)
                {
                    PopPoint();
                }
            }
            _position = pos;
        }
        private void PushPoint(Vector2 point)
        {
            _points.Add(point);
        }
        private void PopPoint()
        {
            if (_points.Count > 0)
            {
                _points.RemoveAt(0);
                _points.TrimExcess();
            }
        }
        private void HandleCounter(float step)
        {
            //Update timer
            deathTimer -= step;
            while (deathTimer <= 0)
            {
                deathTimer += DeathDueTime;
                PopPoint();
            }
        }
        public void UpdateTrail(float step)
        {
            //make sure atleast one segment 
            if (trailSegs.Count == 0)
            {
                var segment = new TrailSegment();
                trailSegs.Add(segment);
            }
            if (_points.Count < 2) return;

            HandleCounter(step);


            var points = new List<Vector2>(_points);
            points.Add(_position);
            Vector2 normal = Vector2.Zero;

            float width = 1f / (points.Count - 2);
            float height = 1f;

            trailSegs.Clear();
            //Calculate 
            for (int i = 0; i < points.Count - 2; i++)
            {
                var coordX = i * width;
                Rectangle src = new Rectangle(coordX, 0, width, height);

                var quad = new TrailSegment();
                quad.src = src;

                var range = points[i + 1] - points[i];


                //Bot Left
                normal = Rot90CCW(range);
                normal = Vector2.Normalize(normal) * 1 * LineWidth;
                quad.p1 = points[i + 1] + normal;

                //Top left
                normal = Rot90CW(range);
                normal = Vector2.Normalize(normal) * (1) * LineWidth;
                quad.p4 = points[i + 1] + normal;


                var nextRange = points[i + 2] - points[i + 1];
                //Bot Right
                normal = Rot90CCW(nextRange);
                normal = Vector2.Normalize(normal) * 1 * LineWidth;
                quad.p2 = points[i + 2] + normal;

                //Top Right
                normal = Rot90CW(nextRange);
                normal = Vector2.Normalize(normal) * 1 * LineWidth;
                quad.p3 = points[i + 2] + normal;

                trailSegs.Add(quad);
            }

        }
        public void DrawTrail(Texture2D texture, Color color)
        {

            if (trailSegs.Count == 0 || _points.Count < 2) return;
            for (int i = 0; i < trailSegs.Count; i++)
            {
                var col = Fade(color, (float)i / trailSegs.Count);
                var q = trailSegs[i];
                //DrawLineEx(q.p1, q.p2,1,Color.DARKGRAY);
                //DrawLineEx(q.p3, q.p4,1,Color.DARKGRAY);
                DrawTextureRectDynamic(texture, q.src,
                    q.p1, q.p2, q.p3, q.p4, col, 1);
                //DrawLineEx(q.p2, q.p3, 1, Color.DARKGRAY);

            }
        }

        static Vector2 Rot90CCW(Vector2 vector) => new Vector2(-vector.Y, vector.X);
        static Vector2 Rot90CW(Vector2 vector) => new Vector2(vector.Y, -vector.X);
         static List<Vector2> Smooth(List<Vector2> input)
    => Smooth(input, 0, input.Count);
        static List<Vector2> Smooth(List<Vector2> input, int startIndex, int range)
        {
            //expected size
            var output = new List<Vector2>(input.Count);

            //first element
            output.Add(input[0]);

            //average elements
            var count = startIndex + range;
            if (count < 2) return input;
            for (int i = startIndex; i < count - 1; i++)
            {
                Vector2 p0 = input[i];
                Vector2 p1 = input[i + 1];

                Vector2 Q = new Vector2(
                    0.75f * p0.X + 0.25f * p1.X,
                    0.75f * p0.Y + 0.25f * p1.Y);
                Vector2 R = new Vector2(
                    0.25f * p0.X + 0.75f * p1.X,
                    0.25f * p0.Y + 0.75f * p1.Y);
                output.Add(Q);
                output.Add(R);
            }

            //last element
            output.Add(input[input.Count - 1]);
            return output;
        }

        void DrawTextureRectDynamic(Texture2D texture, Rectangle src,
            Vector2 dest1, Vector2 dest2, Vector2 dest3, Vector2 dest4, Color color, int quadCount)
            => RayUtils.DrawTextureDynamicPro(
                texture,
                new Vector2(src.x, src.y + src.height),
                new Vector2(src.x + src.width, src.y + src.height),
                new Vector2(src.x + src.width, src.y),
                new Vector2(src.x, src.y),
                dest1, dest2, dest3, dest4, color, quadCount
                );
    }
}
