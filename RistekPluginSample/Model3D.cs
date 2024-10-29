using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace RistekPluginSample
{
    public class Model3D
    {
        public Point3D startPoint3D { get; set; }
        public Point3D endPoint3D { get; set; }
        public List<Point3D> points3D { get; set; }
        public List<List<Point3D>> objects3D { get; set; } = new List<List<Point3D>>();


        public Model3D(Point3D startPoint3D, Point3D endPoint3D)
        {
            points3D = new List<Point3D>();
            points3D.Add(startPoint3D);
            points3D.Add(endPoint3D);
        }

        private void CalculateExistBeamPoints()
        {
            //startPoint3D = new Point3D(m0XStart, distYm0Center, m0YStart);
            //endPoint3D = new Point3D(m0XEnd, distYm0Center, m0YEnd);
            //startPoint3Dm1 = new Point3D(m1XStart, distYm1Center, m1YStart);
            //endPoint3Dm1 = new Point3D(m1XEnd, distYm1Center, m1YEnd);
        }
    }
}
