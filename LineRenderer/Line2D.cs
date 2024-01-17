using Raylib_cs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LineRenderer
{
    public class Line2D
    {

        List<Segment> _segments = new List<Segment>();
        List<Point> _points = new List<Point>();
        public float LineWidth = 20;
        private class Segment
        {
            public Point start, end;
            public Vector2 Normal
            {
                get
                {
                    var lineVector = end.Vector - start.Vector;
                        return Vector2.Normalize(new Vector2(-lineVector.Y, lineVector.X));
                  
                }
            }
            /// <summary>
            /// Automic assign Segment to point properties
            /// </summary>
            /// <param name="p0">start point</param>
            /// <param name="p1">end point</param>
            public Segment(Point p0,Point p1)
            {
                Debug.Assert(p0 != null && p1 != null) ;
                this.start = p0;
                this.end = p1;

                ///Assign segment to those points
                p0.forwardSegment = this;
                p1.backwardSegment = this;
            }
        }
        private class Point
        {
            public Vector2 Vector => new Vector2(X,Y);
            public static implicit operator Vector2(Point point) => point.Vector;

            public float X, Y;
            public Segment? backwardSegment,forwardSegment;
            /// <summary>
            /// bottom left of the screen
            /// </summary>
            public Vector2[] GetOffsets(float offsetDelta)
            {
                if (backwardSegment == null && forwardSegment == null)
                    return new Vector2[0];
                else if (backwardSegment == null && forwardSegment != null) // head
                    return new Vector2[] { forwardSegment.start + forwardSegment.Normal  * offsetDelta };
                else if (forwardSegment == null && backwardSegment != null) // tail
                    return new Vector2[] { backwardSegment.end + backwardSegment.Normal * offsetDelta };
                else
                {

                    //Middle  Points

                    ///miter edge
                    return new Vector2[] { Offset(backwardSegment, forwardSegment, offsetDelta) };

                    

                }
            }

            public Point(float x, float y)
            {
                X = x;
                Y = y;
            }
        }
        public void RemovePointRange(int index,int count)
        {

            _points.RemoveRange(index,count);
        }
        public void AddPoint(Vector2 point)
        {
            // If _points is not emapty, create segment start from last vertex to new point location
            var pointToAdd = new Point(point.X, point.Y);
            if (_points.Count > 0)
            {
                // Create Segment
                Point startSegment = _points[_points.Count - 1];
                Point endSegment = pointToAdd;
                Segment newSegment = new Segment(startSegment, endSegment);

                //Add to keep track of segments
                _segments.Add(newSegment);
            }
            //Add to keep track of points
            _points.Add(pointToAdd);
        }

        public void Clear()
        {
            _points.Clear();
            _segments.Clear();
        }
        public void InsertPoint(Vector2 point)
        {
        }

        public void DrawLineDebug()
        {
            var radius = 3;
            for (int i = 0; i < _segments.Count; i++)
            {
                var seg = _segments[i];
                Raylib_cs.Raylib.DrawLineV(seg.start, seg.end, Raylib_cs.Color.GREEN);
                Raylib_cs.Raylib.DrawCircleV(seg.start, radius, Raylib_cs.Color.RED);
                Raylib_cs.Raylib.DrawCircleV(seg.end, radius + 2, Raylib_cs.Color.YELLOW);

                var center = seg.start + (seg.end - seg.start.Vector) / 2f;
                Raylib.DrawLineV(center, center + seg.Normal * 20, Color.BLUE);

            }
            foreach (Point p in _points)
            {
                foreach (Vector2 left in p.GetOffsets(LineWidth))
                {
                    Raylib.DrawLineV(p,left,Color.GRAY);
                    Raylib.DrawCircleV(left,3,Color.WHITE);
                }
                foreach (Vector2 right in p.GetOffsets(-LineWidth))
                {
                    Raylib.DrawLineV(p,right,Color.GRAY);
                    Raylib.DrawCircleV(right,3,Color.WHITE);
                }
            }
        }
        static Vector2 Offset(Segment a,Segment b,float leftMiterOffset)
        {
            return a.end + (a.Normal + b.Normal) * leftMiterOffset / (1 + Vector2.Dot(a.Normal, b.Normal));
            // e = a + (unA +unB)x o / (1 + (unA ◦ unB))
        }
    }
}
