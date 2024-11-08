using Epx.BIM.Models;
using Epx.Ristek.Data.Models;
using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace RistekPluginSample
{
    public class Model3DResult
    {
        public Beam3D NewBeam { get; set; }
        public UiData UiData { get; set; }
        private Model3D _model3D { get; set; }

        public double BeamStartExtensionX { get; set; }
        public double BeamStartExtensionY { get; set; }
        public double BeamEndExtensionX { get; set; }
        public double BeamEndExtensionY { get; set; }
        public double NewBeamOriginY { get; set; }
        public double NewBeamOriginX { get; set; }
        public double DistanceBeetweenSelectedBeamsX { get; set; }
        public double DistanceBeetweenSelectedBeamsY { get; set; }

        private double Beam3DNo1ThicknessY;
        private double Beam3DNo1ThicknessX;

        public double xNew;
        public double yNew;
        public double zNew;

        public Model3DResult(Model3D model3D, UiData uiData)
        {
            _model3D = model3D;
            UiData = uiData;
            NewBeam = new Beam3D(new Member(), _model3D.IsRoofYDirection);
            PrepareBeamExtensions();

            if (_model3D.IsRoofYDirection)
            {
                DistanceBeetweenSelectedBeamsY = _model3D.DistanceBeetweenSelectedBeams;
                Beam3DNo1ThicknessY = _model3D.Beam3DNo1.Thickness;
            }
            else
            {
                DistanceBeetweenSelectedBeamsX = _model3D.DistanceBeetweenSelectedBeams;
                Beam3DNo1ThicknessX = _model3D.Beam3DNo1.Thickness;
            }
        }

        private void PrepareBeamExtensions()
        {
            if (_model3D.IsRoofYDirection)
            {
                BeamStartExtensionY = UiData.BeamStartExtensionValue;
                BeamEndExtensionY = UiData.BeamStartExtensionValue;
            }
            else
            {
                BeamStartExtensionX = UiData.BeamStartExtensionValue;
                BeamEndExtensionX = UiData.BeamStartExtensionValue;
            }

        }

        public void CalculateNewBeamPointsForCastDistance()
        {
            double deltaX = _model3D.Beam3DNo1.EndPoint3D.X - _model3D.Beam3DNo1.StartPoint3D.X;
            double deltaY = _model3D.Beam3DNo1.EndPoint3D.Y - _model3D.Beam3DNo1.StartPoint3D.Y;
            double deltaZ = _model3D.Beam3DNo1.EndPoint3D.Z - _model3D.Beam3DNo1.StartPoint3D.Z;

            if (_model3D.Beam3DNo1.IsMemberEndCombinedCut && _model3D.Beam3DNo1.Alignement == Member.MemberAlignment.Center)
            {
                deltaX = Math.Abs(_model3D.Member1.LeftGeometryEdgeLine.EndPoint.X - _model3D.Member1.LeftGeometryEdgeLine.StartPoint.X);
                deltaZ = Math.Abs(_model3D.Member1.LeftGeometryEdgeLine.EndPoint.Y - _model3D.Member1.LeftGeometryEdgeLine.StartPoint.Y);
            }

            double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

            double ux = deltaX / deltaTotal;
            double uy = deltaY / deltaTotal;

            yNew = _model3D.Beam3DNo1.StartPoint3D.Y + uy * UiData.BeamInsertionDistanceValue;
            xNew = _model3D.Beam3DNo1.StartPoint3D.X + ux * UiData.BeamInsertionDistanceValue;
            zNew = _model3D.Beam3DNo1.StartPoint3D.Z + UiData.BeamInsertionDistanceValue / deltaTotal * deltaZ;

            NewBeam.StartPoint3D = new Point3D(xNew, yNew, zNew);
            NewBeam.EndPoint3D = new Point3D(xNew, _model3D.Member2.PartCSToGlobal.OffsetY, zNew);
        }

        public void CalculateNewBeamPointsForDistanceAlongTheBeam()
        {
            double deltaX = _model3D.Beam3DNo1.EndPoint3D.X - _model3D.Beam3DNo1.StartPoint3D.X;
            double deltaY = _model3D.Beam3DNo1.EndPoint3D.Y - _model3D.Beam3DNo1.StartPoint3D.Y;
            double deltaZ = _model3D.Beam3DNo1.EndPoint3D.Z - _model3D.Beam3DNo1.StartPoint3D.Z;

            if (_model3D.Beam3DNo1.IsMemberEndCombinedCut && _model3D.Member1.Alignment == Member.MemberAlignment.Center)
            {
                deltaX = Math.Abs(_model3D.Member1.LeftGeometryEdgeLine.EndPoint.X - _model3D.Member1.LeftGeometryEdgeLine.StartPoint.X);
                deltaZ = Math.Abs(_model3D.Member1.LeftGeometryEdgeLine.EndPoint.Y - _model3D.Member1.LeftGeometryEdgeLine.StartPoint.Y);
            }

            double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));

            double ux = deltaX / deltaTotal;
            double uy = deltaY / deltaTotal;
            double uz = deltaZ / deltaTotal;

            xNew = _model3D.Beam3DNo1.StartPoint3D.X + ux * UiData.BeamInsertionDistanceValue;
            yNew = _model3D.Beam3DNo1.StartPoint3D.Y + uy * UiData.BeamInsertionDistanceValue;
            zNew = _model3D.Beam3DNo1.StartPoint3D.Z + uz * UiData.BeamInsertionDistanceValue;

            NewBeam.StartPoint3D = new Point3D(xNew, yNew, zNew);
            NewBeam.EndPoint3D = new Point3D(xNew, _model3D.Member2.PartCSToGlobal.OffsetY, zNew);
        }

        public void SetBeamLocationWithExtensions(Member3D beam)
        {
            Point3D newStartPoint3DWithExtension;
            Point3D newEndPoint3DWithExtension;

            NewBeamOriginY = beam.Origin.Y;
            NewBeamOriginX = beam.Origin.X;


            Vector3D planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(_model3D.Beam3DNo1.StartPoint3D, _model3D.Beam3DNo1.EndPoint3D, _model3D.Beam3DNo2.EndPoint3D);
            Point3D startPoint3Dm0 = _model3D.Beam3DNo1.StartPoint3D;
            Point3D startPoint3Dm1 = _model3D.Beam3DNo2.StartPoint3D;
            Point3D endPoint3Dm0 = _model3D.Beam3DNo1.EndPoint3D;
            Point3D endPoint3Dm1 = _model3D.Beam3DNo2.EndPoint3D;

            if (_model3D.Beam3DNo1.IsMemberEndCombinedCut && _model3D.Beam3DNo1.Alignement == Member.MemberAlignment.Center)
            {
                startPoint3Dm0.X = _model3D.Member1StartPointXEdgeLeft;
                startPoint3Dm0.Z = _model3D.Member1StartPointZEdgeLeft;
                endPoint3Dm0.X = _model3D.Member1EndPointXEdgeLeft;
                endPoint3Dm0.Z = _model3D.Member1EndPointZEdgeLeft;
                endPoint3Dm1.X = _model3D.Member2EndPointXEdgeLeft;
                endPoint3Dm1.Z = _model3D.Member2EndPointZEdgeLeft;

                planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(startPoint3Dm0, endPoint3Dm0, endPoint3Dm1);
            }
            planeNormalToFutureBeamTruss.Normalize(); ;

            bool isMinusDirection = _model3D.IsRoofYDirection ?
                startPoint3Dm0.Y > startPoint3Dm1.Y :
                startPoint3Dm0.X > startPoint3Dm1.X;

            if (isMinusDirection)
            {
                newStartPoint3DWithExtension = new Point3D(NewBeamOriginX + BeamStartExtensionX - Beam3DNo1ThicknessX / 2, NewBeamOriginY - Beam3DNo1ThicknessY / 2 + BeamStartExtensionY, beam.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(NewBeamOriginX - BeamEndExtensionX - DistanceBeetweenSelectedBeamsX + Beam3DNo1ThicknessX / 2, NewBeamOriginY + Beam3DNo1ThicknessY / 2 - DistanceBeetweenSelectedBeamsY - BeamEndExtensionY, beam.Origin.Z);

                if (UiData.IsRotatedToTheMainTruss)
                {
                    if (UiData.IsSelectedMemberLeftEdgeOnTop)
                    {
                        beam.SetAlignedStartPoint(newStartPoint3DWithExtension, planeNormalToFutureBeamTruss);
                        beam.SetAlignedEndPoint(newEndPoint3DWithExtension, planeNormalToFutureBeamTruss);
                    }
                    else
                    {
                        beam.SetAlignedStartPoint(newStartPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                        beam.SetAlignedEndPoint(newEndPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                    }

                }
                else
                {
                    beam.SetAlignedStartPoint(newStartPoint3DWithExtension, new Vector3D(0, 0, 1));
                    beam.SetAlignedEndPoint(newEndPoint3DWithExtension, new Vector3D(0, 0, 1));
                }

            }
            else
            {
                newStartPoint3DWithExtension = new Point3D(NewBeamOriginX - BeamStartExtensionX + Beam3DNo1ThicknessX / 2, NewBeamOriginY + Beam3DNo1ThicknessY / 2 - BeamStartExtensionY, beam.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(NewBeamOriginX + BeamEndExtensionX + DistanceBeetweenSelectedBeamsX - Beam3DNo1ThicknessX / 2, NewBeamOriginY - Beam3DNo1ThicknessY / 2 + DistanceBeetweenSelectedBeamsY + BeamEndExtensionY, beam.Origin.Z);

                if (UiData.IsRotatedToTheMainTruss)
                {
                    if (UiData.IsSelectedMemberLeftEdgeOnTop)
                    {
                        beam.SetAlignedStartPoint(newStartPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                        beam.SetAlignedEndPoint(newEndPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                    }
                    else
                    {
                        beam.SetAlignedStartPoint(newStartPoint3DWithExtension, planeNormalToFutureBeamTruss);
                        beam.SetAlignedEndPoint(newEndPoint3DWithExtension, planeNormalToFutureBeamTruss);
                    }
                }
                else
                {
                    beam.SetAlignedStartPoint(newStartPoint3DWithExtension, new Vector3D(0, 0, 1));
                    beam.SetAlignedEndPoint(newEndPoint3DWithExtension, new Vector3D(0, 0, 1));
                }
            }
        }

        public void SetBeamLocationWithExtensions(PlanarStructure planarStructure, Member member)
        {
            Point3D newStartPoint3DWithExtension;
            Point3D newEndPoint3DWithExtension;

            NewBeamOriginY = planarStructure.Origin.Y;
            NewBeamOriginX = planarStructure.Origin.X;


            Vector3D planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(_model3D.Beam3DNo1.StartPoint3D, _model3D.Beam3DNo1.EndPoint3D, _model3D.Beam3DNo2.EndPoint3D);
            Point3D startPoint3Dm0 = _model3D.Beam3DNo1.StartPoint3D;
            Point3D startPoint3Dm1 = _model3D.Beam3DNo2.StartPoint3D;
            Point3D endPoint3Dm0 = _model3D.Beam3DNo1.EndPoint3D;
            Point3D endPoint3Dm1 = _model3D.Beam3DNo2.EndPoint3D;

            if (_model3D.Beam3DNo1.IsMemberEndCombinedCut && _model3D.Beam3DNo1.Alignement == Member.MemberAlignment.Center)
            {
                startPoint3Dm0.X = _model3D.Member1StartPointXEdgeLeft;
                startPoint3Dm0.Z = _model3D.Member1StartPointZEdgeLeft;
                endPoint3Dm0.X = _model3D.Member1EndPointXEdgeLeft;
                endPoint3Dm0.Z = _model3D.Member1EndPointZEdgeLeft;
                endPoint3Dm1.X = _model3D.Member2EndPointXEdgeLeft;
                endPoint3Dm1.Z = _model3D.Member2EndPointZEdgeLeft;

                planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(startPoint3Dm0, endPoint3Dm0, endPoint3Dm1);
            }
            planeNormalToFutureBeamTruss.Normalize(); ;

            bool isMinusDirection = _model3D.IsRoofYDirection ?
                startPoint3Dm0.Y > startPoint3Dm1.Y :
                startPoint3Dm0.X > startPoint3Dm1.X;

            if (isMinusDirection)
            {
                newStartPoint3DWithExtension = new Point3D(NewBeamOriginX + BeamStartExtensionX - Beam3DNo1ThicknessX / 2, NewBeamOriginY - Beam3DNo1ThicknessY / 2 + BeamStartExtensionY, planarStructure.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(NewBeamOriginX - BeamEndExtensionX - DistanceBeetweenSelectedBeamsX + Beam3DNo1ThicknessX / 2, NewBeamOriginY + Beam3DNo1ThicknessY / 2 - DistanceBeetweenSelectedBeamsY - BeamEndExtensionY, planarStructure.Origin.Z);

                if (UiData.IsRotatedToTheMainTruss)
                {
                    if (UiData.IsSelectedMemberLeftEdgeOnTop)
                    {
                        planarStructure.SetAlignedStartPoint(newStartPoint3DWithExtension, planeNormalToFutureBeamTruss);
                        planarStructure.SetAlignedEndPoint(newEndPoint3DWithExtension, planeNormalToFutureBeamTruss);
                    }
                    else
                    {
                        planarStructure.SetAlignedStartPoint(newStartPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                        planarStructure.SetAlignedEndPoint(newEndPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                    }

                }
                else
                {
                    planarStructure.SetAlignedStartPoint(newStartPoint3DWithExtension, new Vector3D(0, 0, 1));
                    planarStructure.SetAlignedEndPoint(newEndPoint3DWithExtension, new Vector3D(0, 0, 1));
                }

            }
            else
            {
                newStartPoint3DWithExtension = new Point3D(NewBeamOriginX - BeamStartExtensionX + Beam3DNo1ThicknessX / 2, NewBeamOriginY + Beam3DNo1ThicknessY / 2 - BeamStartExtensionY, planarStructure.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(NewBeamOriginX + BeamEndExtensionX + DistanceBeetweenSelectedBeamsX - Beam3DNo1ThicknessX / 2, NewBeamOriginY - Beam3DNo1ThicknessY / 2 + DistanceBeetweenSelectedBeamsY + BeamEndExtensionY, planarStructure.Origin.Z);

                if (UiData.IsRotatedToTheMainTruss)
                {
                    if (UiData.IsSelectedMemberLeftEdgeOnTop)
                    {
                        planarStructure.SetAlignedStartPoint(newStartPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                        planarStructure.SetAlignedEndPoint(newEndPoint3DWithExtension, -planeNormalToFutureBeamTruss);
                    }
                    else
                    {
                        planarStructure.SetAlignedStartPoint(newStartPoint3DWithExtension, planeNormalToFutureBeamTruss);
                        planarStructure.SetAlignedEndPoint(newEndPoint3DWithExtension, planeNormalToFutureBeamTruss);
                    }
                }
                else
                {
                    planarStructure.SetAlignedStartPoint(newStartPoint3DWithExtension, new Vector3D(0, 0, 1));
                    planarStructure.SetAlignedEndPoint(newEndPoint3DWithExtension, new Vector3D(0, 0, 1));
                }
            }

            member.AlignedStartPoint = new Point(newStartPoint3DWithExtension.Y, 0);
            member.AlignedEndPoint = new Point(newEndPoint3DWithExtension.Y, 0);

            planarStructure.AddMember(member, true);
            planarStructure.UpdateMemberCuts(member, true);
        }

        public Model3DNode.ModelPartAlignment ConvertToModelPartAlignmentNewBeam(string alignmentOption)
        {
            string topEdgeTranslation = Strings.Strings.topEdge;
            string bottomEdgeTranslation = Strings.Strings.bottomEdge;
            string centerTranslation = Strings.Strings.center;

            if (UiData.IsRotatedToTheMainTruss)
            {
                switch (alignmentOption)
                {
                    case var option when option == topEdgeTranslation:
                        return Model3DNode.ModelPartAlignment.BottomCenter;
                    case var option when option == bottomEdgeTranslation:
                        return Model3DNode.ModelPartAlignment.TopCenter;
                    case var option when option == centerTranslation:
                        return Model3DNode.ModelPartAlignment.Center;
                    default:
                        throw new ArgumentException(Strings.Strings.unknownAlignementOption);
                }
            }
            else
            {
                switch (alignmentOption)
                {
                    case var option when option == topEdgeTranslation:
                        return Model3DNode.ModelPartAlignment.TopCenter;
                    case var option when option == bottomEdgeTranslation:
                        return Model3DNode.ModelPartAlignment.BottomCenter;
                    case var option when option == centerTranslation:
                        return Model3DNode.ModelPartAlignment.Center;
                    default:
                        throw new ArgumentException(Strings.Strings.unknownAlignementOption);
                }
            }
        }
    }
}
