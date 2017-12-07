using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Point = System.Windows.Point;

namespace BezierTest
{
    public class Program
    {
        public static void Main()
        {
            const int bitmapWidth = 1024;
            const int bitmapHeight = 1024;
            const int bitmapMargin = 20;
            const int bitmapInnerWidth = bitmapWidth - 2 * bitmapMargin;
            const int bitmapInnerHeight = bitmapHeight - 2 * bitmapMargin;
            const float curvePointSize = 2f;
            const float controlPointSize = 10f;
            const float controlPointBorder = 4f;
            const int pointResolution = 200;

            var controlPoints = GeneratePoints();
            var curvePoints = Interpolate(controlPoints, pointResolution);
            var bitmap = new Bitmap(bitmapWidth, bitmapHeight);
            var g = Graphics.FromImage(bitmap);
            var brushWhite = new SolidBrush(Color.White);
            var brushGray = new SolidBrush(Color.Gray);
            var brushBlack = new SolidBrush(Color.Black);

            g.FillRectangle(brushBlack, 0, 0, bitmapWidth, bitmapHeight);

            foreach (var point in curvePoints)
            {
                var x = (float)point.X * bitmapInnerWidth + bitmapMargin - curvePointSize / 2;
                var y = (float)(1 - point.Y) * bitmapInnerHeight + bitmapMargin - curvePointSize / 2;

                g.FillEllipse(brushWhite, x, y, curvePointSize, curvePointSize);
            }

            foreach (var point in controlPoints)
            {
                var initialX = (float)point.X * bitmapInnerWidth + bitmapMargin;
                var initialY = (float)(1 - point.Y) * bitmapInnerHeight + bitmapMargin;

                var size = controlPointSize;
                var x = initialX - size / 2;
                var y = initialY - size / 2;
                
                g.FillRectangle(brushGray, x, y, size, size);

                size -= controlPointBorder;
                x = initialX - size / 2;
                y = initialY - size / 2;
                
                g.FillRectangle(brushWhite, x, y, size, size);
            }

            if (File.Exists("old.bmp"))
            {
                File.Delete("old.bmp");
            }

            if (File.Exists("out.bmp"))
            {
                File.Move("out.bmp", "old.bmp");
            }

            bitmap.Save("out.bmp");
        }

        private static List<Point> GeneratePoints()
        {
            var pointList = new List<Point>
                {
                    new Point(0.00,  0.00),
                    new Point(0.01,  0.00),
                    new Point(0.40,  0.00),
                    new Point(0.70,  0.00),
                    new Point(0.85,  1.00),
                    new Point(0.90,  1.00),
                    new Point(1.00,  1.00)
                };

            return pointList;
        }

        private static IEnumerable<Point> Interpolate(ICollection<Point> controlPoints, int resolution)
        {
            var result = new List<Point>();

            for (var i = 0; i < resolution; i++) {
                result.Add(GetPointRecursive(controlPoints, GetPercentThrough(i, resolution)));
            }

            return result;
        }

        private static double GetPercentThrough(int i1, int i2)
        {
            return i1 / (double)(i2 - 1);
        }

        private static Point GetPointRecursive(ICollection<Point> points, double percentThrough)
        {
            if (points.Count == 1) return points.ElementAt(0);

            var result = new List<Point>();

            for (var i = 1; i < points.Count; i++) {
                var point1 = points.ElementAt(i - 1);
                var point2 = points.ElementAt(i);
                result.Add(GetPointOnLine(point1, point2, percentThrough));
            }

            return GetPointRecursive(result, percentThrough);
        }

        private static Point GetPointOnLine(Point point1, Point point2, double percentThrough)
        {
            var x1 = point1.X;
            var x2 = point2.X;
            var y1 = point1.Y;
            var y2 = point2.Y;

            var x = x1 + (x2 - x1) * percentThrough;
            var y = y1 + (y2 - y1) * percentThrough;

            return CreateAdjustedPoint(x, y);
        }

        private static Point CreateAdjustedPoint(double x, double y)
        {
            return new Point(x, y);
        }
    }
}
