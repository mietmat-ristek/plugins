using Epx.Ristek.Data.Models;
using RistekPluginSample.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RistekPluginSample
{
    public static class MyUtils
    {

        #region object manage

        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            MemberExpression me = propertyLambda.Body as MemberExpression;
            if (me == null)
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");

            return me.Member.Name;
        }

        // see https://stackoverflow.com/questions/5508050/how-to-get-a-property-value-based-on-the-name
        public static object GetPropertyValue(object container, string propertyName)
        {
            return container.GetType().GetProperties()
               .Single(pi => pi.Name == propertyName)
               .GetValue(container, null);
        }

        // see https://stackoverflow.com/questions/9404523/set-property-value-using-property-name
        public static void SetPropertyValue(object container, string propertyName, object value)
        {
            container.GetType().GetProperties()
               .Single(pi => pi.Name == propertyName)
               .SetValue(container, value, null);
        }

        // see https://stackoverflow.com/questions/347614/storing-wpf-image-resources
        internal static ImageSource doGetImageSourceFromResource(string psAssemblyName, string psResourceName)
        {
            Uri oUri = new Uri("pack://application:,,,/" + psAssemblyName + ";component/" + psResourceName, UriKind.RelativeOrAbsolute);
            return System.Windows.Media.Imaging.BitmapFrame.Create(oUri);
        }

        internal static ImageSource doGetImageSourceFromResource(string psResourceName)
        {
            string _psAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            return doGetImageSourceFromResource(_psAssemblyName, psResourceName);
        }

        public static System.Globalization.NumberFormatInfo getCurrentNumberFormatInfo()
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.PositiveInfinitySymbol = "\u221E";
            //System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NegativeInfinitySymbol = "\u221E";
            return System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat;
        }

        public static bool IsSameOrSubclass(Type potentialDescendant, Type potentialBase)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        public static bool CompareList<T>(List<T> list1, List<T> list2) where T : IComparable
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;
            // Early check for difference
            if (list1.Count != list2.Count)
            {
                return false;
            }
            int count = list1.Count;
            for (int index = 0; index < count; index++)
            {
                T _item = list1[index];
                T _objList2 = list2[index];
                if (_item == null && _objList2 == null)
                    return true;
                if (_item == null || _objList2 == null)
                    return false;
                if (!_item.Equals(_objList2))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valA"></param>
        /// <param name="valB"></param>
        /// <param name="tollerance"></param>
        /// <returns>0 if values equal by tollerance;
        /// 1 if valA > valB by tollerance;
        /// -1 if valA ＜ valB by tollerance;
        /// </returns>
        public static int CompareWithTollerance(double valA, double valB, double tollerance = 0.001)
        {
            if (Math.Abs(valA - valB) <= tollerance)
            {
                return 0;
            }
            if (valA - valB > tollerance)
            {
                return 1;
            }
            if (valB - valA > tollerance)
            {
                return -1;
            }
            else { throw new NotImplementedException(); }
        }

        public static bool AreEqual(Point point1, Point point2, double tolerance = 0.001)
        {
            // Check if the absolute difference in each coordinate is within the tolerance
            return Math.Abs(point1.X - point2.X) <= tolerance && Math.Abs(point1.Y - point2.Y) <= tolerance;
        }

        public static double getListForModuleValueMax(IList<double> list, bool preferPositiveValues_andNotNegative = true)
        {
            double res = 0;

            foreach (double valCurr in list)
            {
                if (Math.Abs(valCurr) > Math.Abs(res))
                {
                    res = valCurr;
                }
                else if (valCurr == -res)
                {
                    if (preferPositiveValues_andNotNegative)
                    {
                        res = Math.Abs(res);
                    }
                    else
                    {
                        res = -Math.Abs(res);
                    }
                }
            }

            return res;
        }

        public static double getListForModuleValueMin(IList<double> list, bool preferPositiveValues_andNotNegative = true)
        {
            double res = 0;

            bool foundFirst = false;
            foreach (double valCurr in list)
            {
                if (!foundFirst)
                {
                    res = valCurr;
                    foundFirst = true;
                }
                else if (Math.Abs(valCurr) < Math.Abs(res))
                {
                    res = valCurr;
                }
                else if (valCurr == -res)
                {
                    if (preferPositiveValues_andNotNegative)
                    {
                        res = Math.Abs(res);
                    }
                    else
                    {
                        res = -Math.Abs(res);
                    }
                }
            }

            return res;
        }

        public static Object returnObjectDependingOnCondVal(bool _cond, Object objectForTrue, Object objectForFalse)
        {
            if (_cond)
            {
                return objectForTrue;
            }
            else
            {
                return objectForFalse;
            }
        }

        // see https://stackoverflow.com/questions/11959380/how-to-get-the-descriptionattribute-value-from-an-enum-member
        public static string GetEnumDescription(Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static MyExtendedInfoAttributeAttribute GetMyExtendedInfoAttribute(Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            MyExtendedInfoAttributeAttribute[] attributes = (MyExtendedInfoAttributeAttribute[])fi.GetCustomAttributes(typeof(MyExtendedInfoAttributeAttribute), false);

            if (attributes.Length > 0)
                return attributes[0];
            else
                return null;
        }

        #endregion

        #region geommetry tools

        public static double Magnitude(Vector3D vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        public static Vector3D Normalize(Vector3D vector)
        {
            double magnitude = Magnitude(vector);
            return new Vector3D(vector.X / magnitude, vector.Y / magnitude, vector.Z / magnitude);
        }

        public static Vector3D CrossProduct(Vector3D vector1, Vector3D vector2)
        {
            double x = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            double y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            double z = vector1.X * vector2.Y - vector1.Y * vector2.X;
            return new Vector3D(x, y, z);
        }

        public static double DotProduct(Vector vector1, Vector vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y;
        }

        public static double DotProduct(Vector3D vector1, Vector3D vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        public static bool AreParallel(Vector3D vector1, Vector3D vector2, double tolerance = 1e-6)
        {
            Vector3D crossProduct = CrossProduct(vector1, vector2);
            double crossProductMagnitude = Magnitude(crossProduct);
            return Math.Abs(crossProductMagnitude) < tolerance;
        }

        public static Vector Normalize(Vector vector)
        {
            double magnitude = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            return new Vector(vector.X / magnitude, vector.Y / magnitude);
        }

        public static Point CastPointOnLine(Point lineStart, Point lineEnd, Point point)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;

            double t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);

            double newX = lineStart.X + t * dx;
            double newY = lineStart.Y + t * dy;

            return new Point(newX, newY);
        }
        public static Vector3D Convert2DTo3D(Vector vector2D, double z = 0)
        {
            return new Vector3D(vector2D.X, vector2D.Y, z);
        }

        public static bool AreParallel(Vector vector1, Vector vector2, double tolerance = 1e-6)
        {
            Vector3D vector3D_1 = Convert2DTo3D(vector1);
            Vector3D vector3D_2 = Convert2DTo3D(vector2);
            return AreParallel(vector3D_1, vector3D_2, tolerance);
        }

        public static bool ArePararrelMembers(Member mA, Member mB, double tolerance = 1e-6)
        {
            return (MyUtils.AreParallel(MyUtils.Normalize(mA.XAxis), MyUtils.Normalize(mB.XAxis), tolerance)
                && MyUtils.AreParallel(MyUtils.Normalize(mA.YAxis), MyUtils.Normalize(mB.YAxis), tolerance)
                && MyUtils.AreParallel(MyUtils.Normalize(mA.ZAxis3D), MyUtils.Normalize(mB.ZAxis3D), tolerance)
                );
        }

        public static void OrientVectors(ref Vector3D vector1, ref Vector3D vector2)
        {
            double dotProduct = DotProduct(vector1, vector2);

            if (dotProduct < 0)
            {
                // Flip the direction of vector2
                vector2 = -vector2;
            }
        }

        public static void OrientVectors(ref Vector vector1, ref Vector vector2)
        {
            double dotProduct = DotProduct(vector1, vector2);

            if (dotProduct < 0)
            {
                // Flip the direction of vector2
                vector2 = -vector2;
            }
        }

        private static bool AreVectorsColinear(Vector3D vector1, Vector3D vector2, double tolerance)
        {
            OrientVectors(ref vector1, ref vector2);

            double dotProduct = DotProduct(vector1, vector2);
            double magnitudeProduct = vector1.Length * vector2.Length;

            double cosTheta = dotProduct / magnitudeProduct;
            double angle = Math.Acos(cosTheta);

            return Math.Abs(angle) < tolerance || Math.Abs(angle - Math.PI) < tolerance;
        }

        /*
        public static bool AreMembersColinear(Line3D line1, Line3D line2, double tolerance = 1e-6)
        {
            Vector3D direction1 = line1.EndPoint - line1.StartPoint;
            Vector3D direction2 = line2.EndPoint - line2.StartPoint;

            return AreVectorsColinear(direction1, direction2, tolerance);
        }
        */

        public static bool AreMembersColinear(Member mA, Member mB, double tolerance = 1e-6)
        {
            Vector direction1 = mA.EndPoint - mA.StartPoint;
            Vector direction2 = mB.EndPoint - mB.StartPoint;

            return AreVectorsColinear(direction1, direction2, tolerance);
        }

        private static bool AreVectorsColinear(Vector vector1, Vector vector2, double tolerance)
        {
            OrientVectors(ref vector1, ref vector2);

            double dotProduct = DotProduct(vector1, vector2);
            double magnitudeProduct = vector1.Length * vector2.Length;

            double cosTheta = dotProduct / magnitudeProduct;
            double angle = Math.Acos(cosTheta);

            return (vector1.X == vector2.X && vector1.Y == vector2.Y)
                || Math.Abs(angle) < tolerance
                || Math.Abs(angle - Math.PI) < tolerance
                ;
        }

        public static bool ArePararrelTrusses(ParametricTruss pA, ParametricTruss pB, double tolerance = 1e-6)
        {
            return (MyUtils.AreParallel(MyUtils.Normalize(pA.XAxis), MyUtils.Normalize(pB.XAxis), tolerance)
                && MyUtils.AreParallel(MyUtils.Normalize(pA.YAxis), MyUtils.Normalize(pB.YAxis), tolerance)
                && MyUtils.AreParallel(MyUtils.Normalize(pA.ZAxis), MyUtils.Normalize(pB.ZAxis), tolerance)
                );
        }

        public static Point? FindIntersection(Point line_start1, Point line_end1, Point segment_start2, Point segment_end2, double tolerance = 1e-6)
        {
            // Define vectors
            double dx1 = line_end1.X - line_start1.X;
            double dy1 = line_end1.Y - line_start1.Y;
            double dx2 = segment_end2.X - segment_start2.X;
            double dy2 = segment_end2.Y - segment_start2.Y;

            // Calculate determinant
            double det = dx1 * dy2 - dy1 * dx2;

            // If determinant is zero, the lines are parallel
            if (Math.Abs(det) < tolerance)
            {
                return null;
            }

            // Calculate parameters for each segment
            double t1 = ((line_start1.Y - segment_start2.Y) * dx2 + (segment_start2.X - line_start1.X) * dy2) / det;
            double t2 = ((line_start1.Y - segment_start2.Y) * dx1 + (segment_start2.X - line_start1.X) * dy1) / det;

            // Check if the intersection point is within the segments
            if (t1 >= 0 && t2 >= 0 && t2 <= 1)
            {
                // Calculate intersection point
                double intersectionX = line_start1.X + t1 * dx1;
                double intersectionY = line_start1.Y + t1 * dy1;
                return new Point(intersectionX, intersectionY);
            }
            else
            {
                // No intersection
                return null;
            }
        }

        public static double CalculateZ_forPointOnLine(Point3D line_startPoint, Point3D line_endPoint, double in_x, double in_y)
        {
            // Check if the line is vertical (parallel to the X or Y axis)
            if (Math.Abs(line_startPoint.X - line_endPoint.X) < double.Epsilon)
            {
                // The line is parallel to the Y axis
                return line_startPoint.Z + (line_endPoint.Z - line_startPoint.Z) * (in_y - line_startPoint.Y) / (line_endPoint.Y - line_startPoint.Y);
            }
            else if (Math.Abs(line_startPoint.Y - line_endPoint.Y) < double.Epsilon)
            {
                // The line is parallel to the X axis
                return line_startPoint.Z + (line_endPoint.Z - line_startPoint.Z) * (in_x - line_startPoint.X) / (line_endPoint.X - line_startPoint.X);
            }
            else
            {
                // Calculate the Z value using linear interpolation
                double t = (in_x - line_startPoint.X) / (line_endPoint.X - line_startPoint.X);
                return line_startPoint.Z + t * (line_endPoint.Z - line_startPoint.Z);
            }
        }

        public static Point3D CastPointOnPlane(Point3D point, Point3D origin, Vector3D normal)
        {
            // Calculate vector from plane origin to the given point
            Vector3D pointVector = new Vector3D((point.X - origin.X), (point.Y - origin.Y), (point.Z - origin.Z));

            // Calculate the projection of pointVector onto the plane's normal vector
            double distance = Vector3D.DotProduct(pointVector, normal);

            // Calculate the projected point on the plane
            double projectedX = point.X - distance * normal.X;
            double projectedY = point.Y - distance * normal.Y;
            double projectedZ = point.Z - distance * normal.Z;

            return new Point3D(projectedX, projectedY, projectedZ);
        }

        public static bool IsPointInExtension(Vector3D vector, Vector3D point, double tolerance = 1e-6)
        {
            Vector3D normalizedVector = Normalize(vector);
            double dotProduct = vector.X * point.X + vector.Y * point.Y + vector.Z * point.Z;
            return dotProduct >= -tolerance;
        }

        public static bool IsPointInExtension(Vector vector, Vector point, double tolerance = 1e-6)
        {
            Vector normalizedVector = Normalize(vector);
            double dotProduct = vector.X * point.X + vector.Y * point.Y;
            return dotProduct >= -tolerance;
        }

        public static bool CheckSameOrientation(Point3D start1, Point3D end1, Point3D start2, Point3D end2)
        {
            Vector3D dir1 = end1 - start1;
            Vector3D dir2 = end2 - start2;

            double dotProduct = DotProduct(dir1, dir2);
            return dotProduct > 0;
        }

        public static bool CheckSameOrientation(Point start1, Point end1, Point start2, Point end2)
        {
            Vector dir1 = end1 - start1;
            Vector dir2 = end2 - start2;

            double dotProduct = DotProduct(dir1, dir2);
            return dotProduct > 0;
        }

        // not working properly for every case
        public static Point3D TransformLocalToGlobal(Point3D origin, Vector3D xAxis, Vector3D yAxis, Vector3D zAxis, double xCoord, double yCoord)
        {
            double x = origin.X + xAxis.X * xCoord + yAxis.X * yCoord + zAxis.X * 0;
            double y = origin.Y + xAxis.Y * xCoord + yAxis.Y * yCoord + zAxis.Y * 0;
            double z = origin.Z + xAxis.Z * xCoord + yAxis.Z * yCoord + zAxis.Z * 0;
            return new Point3D(x, y, z);
        }

        // not working properly for every case
        public static Point3D TransformLocalToGlobal(ParametricTruss parametricTrussPlain, Point pointOnPlainLocal)
        {
            Point3D origin = parametricTrussPlain.Origin;
            Vector3D xAxis = parametricTrussPlain.XAxis;
            Vector3D yAxis = parametricTrussPlain.YAxis;
            Vector3D zAxis = parametricTrussPlain.ZAxis;
            double xCoord = pointOnPlainLocal.X;
            double yCoord = pointOnPlainLocal.Y;

            double x = origin.X + xAxis.X * xCoord + yAxis.X * yCoord + zAxis.X * 0;
            double y = origin.Y + xAxis.Y * xCoord + yAxis.Y * yCoord + zAxis.Y * 0;
            double z = origin.Z + xAxis.Z * xCoord + yAxis.Z * yCoord + zAxis.Z * 0;
            return new Point3D(x, y, z);
        }

        public static double PointDistanceToTrussPlane(ParametricTruss parametricTrussPlain, Point3D point)
        {
            IList<Point> _pt1_localBoundsDiagonalPoints = parametricTrussPlain.GetLocalBounds();
            List<Point3D> _pt1_surface3DVertices = new Point3D[] {
                    parametricTrussPlain.LocalToGlobal.Transform(Epx.BIM.GeometryTools.GeometryMath.ToPoint3D(_pt1_localBoundsDiagonalPoints[0])),
                    parametricTrussPlain.LocalToGlobal.Transform(new Point3D(_pt1_localBoundsDiagonalPoints[1].X, _pt1_localBoundsDiagonalPoints[0].Y, 0)),
                    parametricTrussPlain.LocalToGlobal.Transform(Epx.BIM.GeometryTools.GeometryMath.ToPoint3D(_pt1_localBoundsDiagonalPoints[1])),
                    parametricTrussPlain.LocalToGlobal.Transform(new Point3D(_pt1_localBoundsDiagonalPoints[0].X, _pt1_localBoundsDiagonalPoints[1].Y, 0))
                }.ToList();
            Epx.BIM.GeometryTools.Surface3D _pt1_surface3D = new Epx.BIM.GeometryTools.Surface3D(_pt1_surface3DVertices);

            double res;
            {
                Point3D pointOnPlane = _pt1_surface3DVertices[0];
                Vector3D planeNormal = _pt1_surface3D.Normal;
                res = Math.Abs(Epx.BIM.GeometryTools.GeometryMath.PointDistanceToPlane3D(point, pointOnPlane, planeNormal));
            }

            return res;
        }

        public static (Vector3D, Vector3D, Vector3D) DeterminePlaneAxes(Point3D point1, Point3D point2, Point3D point3)
        {
            Vector3D vector1 = ConvertPoint3DToVector3D(point1);
            Vector3D vector2 = ConvertPoint3DToVector3D(point2);
            Vector3D vector3 = ConvertPoint3DToVector3D(point3);

            Vector3D xAxis = vector2 - vector1;
            Vector3D zAxis = CrossProduct(xAxis, vector3 - vector1);
            Vector3D yAxis = CrossProduct(zAxis, xAxis);

            return (xAxis, yAxis, zAxis);
        }

        public static Point3D RotateAroundZ(Point3D point, double angleInRadians)
        {
            double cosA = Math.Cos(angleInRadians);
            double sinA = Math.Sin(angleInRadians);

            double xNew = point.X * cosA - point.Y * sinA;
            double yNew = point.X * sinA + point.Y * cosA;
            double zNew = point.Z; 

            return new Point3D(xNew, yNew, zNew);
        }

        // not tested
        public static (Point3D, Vector3D) ConvertPlaneDefinitionToOriginAndNormal(Vector3D Xaxis, Vector3D Yaxis, Vector3D Zaxis)
        {
            // Calculate the origin as the intersection of the three axes
            Vector3D originVector = CrossProduct(Yaxis, Zaxis);
            double denominator = DotProduct(Xaxis, originVector);
            if (Math.Abs(denominator) < 1e-6)
            {
                throw new ArgumentException("Axes are not orthogonal");
            }
            double x = DotProduct(originVector, originVector) / denominator;
            Vector3D origin = Xaxis * x;

            // Calculate the normal vector
            Vector3D normal = CrossProduct(Xaxis, Yaxis);

            Point3D Origin = new Point3D(origin.X, origin.Y, origin.Z);
            Vector3D Normal = normal;
            return (Origin, Normal);
        }

        // 20240328 Knaga
        /*
        public static Point3D ElevateOriginGlobalWithForcedDirectionZ(Vector3D Xaxis, Vector3D Yaxis, Vector3D Zaxis, Point3D origin, double t, bool enforce_zElevationVectorPositive_andNotNegative)
        {
            Zaxis = Normalize(Zaxis);

            Vector3D elevationVector = Zaxis * t;
            if (elevationVector.Z < 0
                && enforce_zElevationVectorPositive_andNotNegative
                )
            {
                elevationVector = Zaxis * (-t);
            }
            if (elevationVector.Z > 0
                && !enforce_zElevationVectorPositive_andNotNegative
                )
            {
                elevationVector = Zaxis * (-t);
            }
            Point3D updatedOrigin = origin + elevationVector;

            return updatedOrigin;
        }
        */
        // ElevateOriginGlobalWithForcedOuterDirectionZaxis
        public enum EnforceElevationVectorEnum { enforcePositive, enforceNegative, doNothing, sideSwitch_whenReferenceResultNegative, sideSwitch_whenReferenceResultPositive }
        public static Point3D ElevateOriginGlobalWithForcedDirectionZ(Vector3D Xaxis, Vector3D Yaxis, Vector3D Zaxis, Point3D origin, double t, Vector3D? referenceEnforcementDirectionVector, EnforceElevationVectorEnum enforceElevationVectorPositive)
        {
            Vector3D _referenceEnforcementDirectionVectorNormalized = Normalize(referenceEnforcementDirectionVector.Value);
            Zaxis = Normalize(Zaxis);

            double _testSkalar = 100;
            Point3D resultPointTest = origin + (Zaxis * _testSkalar);
            Point3D _referencePointTest_ = origin + (_referenceEnforcementDirectionVectorNormalized * _testSkalar);
            //bool _arePointsOnSameSide = elevationVector.Z > 0;
            bool _arePointsOnSameSide;
            {
                Point3D point1 = resultPointTest;
                Point3D point2 = _referencePointTest_;
                Point3D plane_origin = origin;
                Vector3D plane_normal = Zaxis;
                _arePointsOnSameSide = ArePointsOnSameSide(point1, point2, plane_origin, plane_normal);
            }

            Vector3D elevationVector = Zaxis * t;
            if (!_arePointsOnSameSide
                && (enforceElevationVectorPositive == EnforceElevationVectorEnum.enforcePositive
                    || enforceElevationVectorPositive == EnforceElevationVectorEnum.sideSwitch_whenReferenceResultNegative
                    )
                )
            {
                elevationVector = Zaxis * (-t);
            }
            if (_arePointsOnSameSide
                && (enforceElevationVectorPositive == EnforceElevationVectorEnum.enforceNegative
                    || enforceElevationVectorPositive == EnforceElevationVectorEnum.sideSwitch_whenReferenceResultPositive
                    )
                )
            {
                elevationVector = Zaxis * (-t);
            }
            Point3D updatedOrigin = origin + elevationVector;

            return updatedOrigin;
        }

        public static Point3D MovePointForPlainLocalWithResultInGlobal(Vector3D Xaxis, Vector3D Yaxis, Vector3D Zaxis, Point3D origin, double x, double y, double z)
        {
            Xaxis = Normalize(Xaxis);
            Yaxis = Normalize(Yaxis);
            Zaxis = Normalize(Zaxis);

            Vector3D movementVector = Xaxis * x + Yaxis * y + Zaxis * z;
            Point3D updatedOrigin = origin + movementVector;

            return updatedOrigin;
        }

        public static bool ArePointsOnSameSide(Point3D point1, Point3D point2, Point3D plane_origin, Vector3D plane_normal)
        {
            // Calculate vectors from plane's origin to the points
            Vector3D vector1 = new Vector3D((point1.X - plane_origin.X), (point1.Y - plane_origin.Y), (point1.Z - plane_origin.Z));
            Vector3D vector2 = new Vector3D((point2.X - plane_origin.X), (point2.Y - plane_origin.Y), (point2.Z - plane_origin.Z));

            // Calculate dot products with the plane's normal vector
            double dotProduct1 = DotProduct(vector1, plane_normal);
            double dotProduct2 = DotProduct(vector2, plane_normal);

            // Check if dot products have the same sign
            return Math.Sign(dotProduct1) == Math.Sign(dotProduct2);
        }

        public static double DistanceBetweenParallelPlanes(
            Vector3D xaxis1, Vector3D yaxis1, Vector3D zaxis1, Point3D origin1,
            Vector3D xaxis2, Vector3D yaxis2, Vector3D zaxis2, Point3D origin2)
        {
            // Calculate the normal vectors of the planes (cross product of local axes)
            Vector3D normal1 = Vector3D.CrossProduct(xaxis1, yaxis1);
            Vector3D normal2 = Vector3D.CrossProduct(xaxis2, yaxis2);

            // Calculate the direction vector from the origin of plane1 to plane2
            Vector3D directionVector = origin2 - origin1;

            // Calculate the distance between the parallel planes
            double distance = Math.Abs(Vector3D.DotProduct(directionVector, normal1));

            return distance;
        }

        public static Vector3D CalculateNormal(Point3D point1, Point3D point2, Point3D point3)
        {
            // Vector from point1 to point2
            Vector3D vector1 = new Vector3D(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);

            // Vector from point1 to point3
            Vector3D vector2 = new Vector3D(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the cross product to get the normal vector
            double normalX = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            double normalY = vector1.Z * vector2.X - vector1.X * vector2.Z;
            double normalZ = vector1.X * vector2.Y - vector1.Y * vector2.X;

            return new Vector3D(normalX, normalY, normalZ);
        }

        public static Vector3D CalculateNormal(Vector3D inputVector, Point3D point1, Point3D point2)
        {
            // Vector from point1 to point2
            Vector3D vector1 = new Vector3D(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);

            // Cross product of inputVector and vector1 gives the normal vector
            double normalX = inputVector.Y * vector1.Z - inputVector.Z * vector1.Y;
            double normalY = inputVector.Z * vector1.X - inputVector.X * vector1.Z;
            double normalZ = inputVector.X * vector1.Y - inputVector.Y * vector1.X;

            return new Vector3D(normalX, normalY, normalZ);
        }

        public static Point3D TranslatePoint(Point3D point, Vector3D translationVector, double distance)
        {
            // Calculate the translation components
            double translateX = translationVector.X * distance;
            double translateY = translationVector.Y * distance;
            double translateZ = translationVector.Z * distance;

            // Translate the point
            return new Point3D(point.X + translateX, point.Y + translateY, point.Z + translateZ);
        }

        private static Vector3D ConvertPoint3DToVector3D(Point3D point)
        {
            return new Vector3D(point.X, point.Y, point.Z);
        }

        public static Point3D ConvertToPoint3D(Vector3D vector)
        {
            return new Point3D(vector.X, vector.Y, vector.Z);
        }

        public static Vector3D CalculateDirection(Point3D point1, Point3D point2)
        {
            return new Vector3D(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
        }

        public static double PointsDistanceAbsolute(Point3D point1, Point3D point2)
        {
            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;
            double dz = point2.Z - point1.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static double PointsDistanceAbsolute(Point point1, Point point2)
        {
            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static (Point3D, Point3D) FindFarthestPoints(List<Point3D> points, out double maxDistance)
        {
            if (points == null || points.Count < 2)
                throw new ArgumentException("At least two points are required.");

            Point3D farthestPoint1 = new Point3D();
            Point3D farthestPoint2 = new Point3D();
            maxDistance = double.MinValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {

                    double distance = PointsDistanceAbsolute(points[i], points[j]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestPoint1 = points[i];
                        farthestPoint2 = points[j];
                    }
                }
            }

            return (farthestPoint1, farthestPoint2);
        }

        public static (Point, Point) FindFarthestPoints(List<Point> points, out double maxDistance, bool orientPairFromLowerToHigherY = false)
        {
            if (points == null || points.Count < 2)
                throw new ArgumentException("At least two points are required.");

            Point farthestPoint1 = new Point();
            Point farthestPoint2 = new Point();
            maxDistance = double.MinValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    double distance = PointsDistanceAbsolute(points[i], points[j]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestPoint1 = points[i];
                        farthestPoint2 = points[j];
                    }
                }
            }

            //return (farthestPoint1, farthestPoint2);
            if (orientPairFromLowerToHigherY)
            {
                if (farthestPoint1.Y <= farthestPoint2.Y)
                {
                    return (farthestPoint1, farthestPoint2);
                }
                else //if (farthestPoint1.Y > farthestPoint2.Y)
                {
                    return (farthestPoint2, farthestPoint1);
                }
            }
            else
            {
                return (farthestPoint1, farthestPoint2);
            }
        }

        public static string FormatPoint3DWithPrecision(Point3D point, int decimalPrecision = 2)
        {
            string format = $"({{0:F{decimalPrecision}}}, {{1:F{decimalPrecision}}}, {{2:F{decimalPrecision}}})";
            return string.Format(format, point.X, point.Y, point.Z);
        }

        #endregion

        #region presentation

        internal static void ShowErrorMessage(string v)
        {
            MessageBox.Show(v, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal static void ShowInfoMessage(string v)
        {
            MessageBox.Show(v, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal static void ShowWarningMessage(string v)
        {
            MessageBox.Show(v, "", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // unchecked
        // see https://stackoverflow.com/questions/1517743/in-wpf-how-can-i-determine-whether-a-control-is-visible-to-the-user
        public static bool IsControlForUserVisible(UIElement element)
        {
            if (!element.IsVisible)
                return false;
            var container = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (container == null) throw new ArgumentNullException("container");

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.RenderSize.Width, element.RenderSize.Height));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.IntersectsWith(bounds);
        }

        // see: https://stackoverflow.com/questions/11826272/find-a-wpf-element-inside-datatemplate-in-the-code-behind
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : Visual
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        #endregion

        #region strings handling

        public static double ToDouble(object objectIn)
        {
            if (objectIn is string)
            {
                string text = objectIn as string;
                char decimalDeparator = Convert.ToChar(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                text = text.Replace('.', decimalDeparator);
                text = text.Replace(',', decimalDeparator);

                return Convert.ToDouble(text);
            }

            return Convert.ToDouble(objectIn);
        }

        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        /// <summary>
        /// gets a string after specified start string
        /// </summary>
        /// <param name="sourceStr">source string</param>
        /// <param name="afterThatStr">start string</param>
        /// <returns></returns>
        public static string stringAfter(string sourceStr, string afterThatStr)
        {
            int posA = sourceStr.LastIndexOf(afterThatStr);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + afterThatStr.Length;
            if (adjustedPosA >= sourceStr.Length)
            {
                return "";
            }
            return sourceStr.Substring(adjustedPosA);
        }

        /// <summary>
        /// return input string with capitalized first letter
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns></returns>
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                //throw new ArgumentException("ARGH!");
                return null;
            }
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">start from 1 (A)</param>
        /// <returns></returns>
        public static string IntToLetters(int value)
        {
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }
            return result;
        }

        public static string getJoinedErrorsStringList(ICollection<string> errorStrCollection)
        {
            List<string> finalList = new List<string>();

            foreach (string strCurr in errorStrCollection?.Distinct())
            {
                if (!String.IsNullOrEmpty(strCurr))
                {
                    int _strCurrCount = errorStrCollection.Count(x => x.Equals(strCurr));
                    string strCurrModified;
                    if (_strCurrCount > 1)
                    {
                        strCurrModified = strCurr + "\n" + "(x" + _strCurrCount + ")" + "\n";
                    }
                    else
                    {
                        strCurrModified = strCurr;
                    }
                    finalList.Add(strCurrModified);
                }
            }

            if (finalList.Count > 0)
            {
                return string.Join("\n\n", finalList);
            }
            else
            {
                return null;
            }
        }

        public static string[] JoinStringArr(string[] stringArr_1, string[] stringArr_2)
        {
            List<string> resList = new List<string>();
            resList.AddRange(stringArr_1);
            resList.AddRange(stringArr_2);
            return resList.ToArray();
        }

        public static string[] JoinStringArr(string string_1, string[] stringArr_2)
        {
            return JoinStringArr(new string[] { string_1 }, stringArr_2);
        }

        public static string[] JoinStringArr(string[] stringArr_1, string string_2)
        {
            return JoinStringArr(stringArr_1, new string[] { string_2 });
        }

        public static string getJoinedStringListWithWrapper(IList<string> list, string separator, string startWrapper, string endWrapper, bool useWrapper)
        {
            if (!useWrapper)
            {
                return String.Join(separator, list);
            }
            else //if (useWrapper)
            {
                return startWrapper + String.Join(separator, list) + endWrapper;
            }
        }

        public static string getJoinedStringListWithWrapper(IList<double> listVals, int precision, string separator, string startWrapper, string endWrapper, bool useWrapper)
        {
            List<string> listStrNum = new List<string>();
            foreach (double valCurr in listVals)
            {
                listStrNum.Add(MyUtils.pv(valCurr, precision));
            }
            return getJoinedStringListWithWrapper(listStrNum, separator, startWrapper, endWrapper, useWrapper);
        }

        public static string getJoinedStringListWithWrapperGeneral(IList<string> list, string separator, string startWrapper = "( ", string endWrapper = " )")
        {
            return getJoinedStringListWithWrapper(list, separator, startWrapper, endWrapper, list.Count > 1);
        }

        public static string getJoinedStringListWithWrapperGeneral(IList<double> listVals, int precision, string separator, string startWrapper = "( ", string endWrapper = " )")
        {
            return getJoinedStringListWithWrapper(listVals, precision, separator, startWrapper, endWrapper, listVals.Count > 1);
        }

        public static string getJoinedStringListWithWrapperMin(IList<string> list, string separator = "  ;  ", string startWrapper = "min( ", string endWrapper = " )")
        {
            return getJoinedStringListWithWrapper(list, separator, startWrapper, endWrapper, list.Count > 1);
        }

        public static string getJoinedStringListWithWrapperMin(IList<double> listVals, int precision, string separator = "  ;  ", string startWrapper = "min( ", string endWrapper = " )")
        {
            return getJoinedStringListWithWrapper(listVals, precision, separator, startWrapper, endWrapper, listVals.Count > 1);
        }

        public static string getJoinedStringListWithWrapperMax(IList<string> list, string separator = "  ;  ", string startWrapper = "max( ", string endWrapper = " )")
        {
            return getJoinedStringListWithWrapper(list, separator, startWrapper, endWrapper, list.Count > 1);
        }

        public static string getJoinedStringListWithWrapperMax(IList<double> listVals, int precision, string separator = "  ;  ", string startWrapper = "max( ", string endWrapper = " )")
        {
            return getJoinedStringListWithWrapper(listVals, precision, separator, startWrapper, endWrapper, listVals.Count > 1);
        }

        /// <summary>
        /// formats a non-integer value to string
        /// </summary>
        /// <param name="val">double value</param>
        /// <param name="precision">decimal precission</param>
        /// <returns></returns>
        internal static string pv(double val, int precision = 2)
        {
            double valRound = Math.Round(val, precision);
            string decimalPointZerosStr = "";
            for (int i = 0; i < precision; i++)
            {
                decimalPointZerosStr += "0";
            }

            /*
            if (!MainController.isDevModeForConstructor)
            {
                return String.Format("{0:0." + decimalPointZerosStr + "}", valRound);
            }
            else
            */
            {
                return String.Format("{0:0." + decimalPointZerosStr + "}", valRound).Replace(".", ",");
            }
        }

        /// <summary>
        /// formats a non-integer value to string
        /// </summary>
        /// <param name="val">PointF</param>
        /// <param name="precision">decimal precission</param>
        /// <returns></returns>
        internal static string pv(System.Drawing.PointF val, int precision = 2)
        {
            double xRound = Math.Round(val.X, precision);
            double yRound = Math.Round(val.Y, precision);
            string decimalPointZerosStr = "";
            for (int i = 0; i < precision; i++)
            {
                decimalPointZerosStr += "0";
            }

            /*
            if (!MainController.isDevModeForConstructor)
            {
                return "X:" + String.Format("{0:0." + decimalPointZerosStr + "}", xRound) + "; Y:" + String.Format("{0:0." + decimalPointZerosStr + "}", yRound);
            }
            else
            */
            {
                return "X:" + String.Format("{0:0." + decimalPointZerosStr + "}", xRound).Replace(".", ",") + "; Y:" + String.Format("{0:0." + decimalPointZerosStr + "}", yRound).Replace(".", ",");
            }
        }

        /// <summary>
        /// return trueRes or falseRes, depending on cond value
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="trueRes"></param>
        /// <param name="falseRes"></param>
        /// <returns></returns>
        internal static string eqSignFromCond(bool cond, string trueRes, string falseRes)
        {
            if (cond)
            {
                return trueRes;
            }
            else
            {
                return falseRes;
            }
        }

        public static IList<string> suplementEqStrListWhenCond(IList<string> eqStrListIn, bool condForAdding, string addStrToCurrLastOnList, string strToAddNew)
        {
            if (condForAdding)
            {
                string eqLastCurr = eqStrListIn.Last();
                eqStrListIn.RemoveAt(eqStrListIn.Count - 1);
                eqLastCurr += addStrToCurrLastOnList;
                eqStrListIn.Add(eqLastCurr);
                eqStrListIn.Add(strToAddNew);
            }
            return eqStrListIn;
        }

        #endregion

        #region DataTemplateSelector extension stuff

        public static T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        #endregion

    }
}

namespace RistekPluginSample.Converters
{
    public class StringsExtension
    {
        public static string getValueOrReturnKey(string resourceKey)
        {
            string _val = Strings.Strings.ResourceManager.GetString(resourceKey);
            if (_val != null)
            {
                return _val;
            }
            else
            {
                return resourceKey;
            }
        }
    }

    // see https://stackoverflow.com/questions/6145888/how-to-bind-an-enum-to-a-combobox-control-in-wpf
    // see https://stackoverflow.com/questions/58743/databinding-an-enum-property-to-a-combobox-in-wpf
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumerationExtension(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");

            EnumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                    return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) // or IXamlServiceProvider for UWP and WinUI
        {
            var enumValues = Enum.GetValues(EnumType);

            return (
              from object enumValue in enumValues
              select new EnumerationMember
              {
                  Value = enumValue,
                  Description = GetDescription(enumValue),
              }).ToArray();
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = EnumType
              .GetField(enumValue.ToString())
              .GetCustomAttributes(typeof(DescriptionAttribute), false)
              .FirstOrDefault() as DescriptionAttribute;


            return descriptionAttribute != null
              ? descriptionAttribute.Description
              : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string DescriptionResourceKey)
            : base(StringsExtension.getValueOrReturnKey(DescriptionResourceKey))
        {
            // do nothing
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class MyExtendedInfoAttributeAttribute : DescriptionAttribute
    {
        /*
        public MyExtendedInfoAttributeAttribute(string Description, string MyExtendedInfo1)
            : base(Description)
        {
            this.MyExtendedInfo1 = MyExtendedInfo1;
        }
        */
        // see https://stackoverflow.com/questions/569298/localizing-enum-descriptions-attributes
        public MyExtendedInfoAttributeAttribute(string DescriptionResourceKey, string MyExtendedInfo1ResourceKey)
            : base(StringsExtension.getValueOrReturnKey(DescriptionResourceKey))
        {
            this.MyExtendedInfo1 = StringsExtension.getValueOrReturnKey(MyExtendedInfo1ResourceKey);
        }

        public MyExtendedInfoAttributeAttribute(string DescriptionResourceKey, string MyExtendedInfo1ResourceKey, double MyDoubleValue)
            : base(StringsExtension.getValueOrReturnKey(DescriptionResourceKey))
        {
            this.MyExtendedInfo1 = StringsExtension.getValueOrReturnKey(MyExtendedInfo1ResourceKey);
            this.MyDoubleValue = MyDoubleValue;
        }

        public MyExtendedInfoAttributeAttribute(string DescriptionResourceKey, double MyDoubleValue)
            : base(StringsExtension.getValueOrReturnKey(DescriptionResourceKey))
        {
            this.MyDoubleValue = MyDoubleValue;
        }

        public string MyExtendedInfo1 { get; private set; }
        public double MyDoubleValue { get; private set; }
    }

    public class DoubleInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /*
            string _strIn = (string)value;
            string _valueNew = _strIn;
            _valueNew = _strIn.Replace(",", culture.NumberFormat.NumberDecimalSeparator);
            _valueNew = _strIn.Replace(".", culture.NumberFormat.NumberDecimalSeparator);
            double res = double.Parse(_valueNew);
            return res;
            */

            /*
            string _strIn = (string)value;
            string _valueNew = _strIn;
            _valueNew = _valueNew.Replace(",", culture.NumberFormat.NumberDecimalSeparator);
            _valueNew = _valueNew.Replace(".", culture.NumberFormat.NumberDecimalSeparator);
            DataTable dt = new DataTable();
            dt.Compute(_valueNew, "");
            object computationResult = dt.Compute(_valueNew, "");

            return computationResult;
            */
            throw new NotImplementedException();
        }
    }

    // see https://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum
    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }
}
