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

        //private double verticalMoveForNewBeam;
        //private double verticalEavesZMove;
        //private double horizontalEavesXMove;
        //private double normalEavesYMove;
        //private double normalEavesXMove;
        //private double normalEavesZMove;
        //private double xMoveForNewBeam;
        //private double yMoveForNewBeam;
        //private double zMoveForNewBeam;
        private double Beam3DNo1ThicknessY;
        private double Beam3DNo1ThicknessX;
        //private Member.MemberAlignment newBeamAlignment;
        //private Member.MemberAlignment existBeamAlignment;
        public double xNew;
        public double yNew;
        public double zNew;

        public Model3DResult(Model3D model3D, UiData uiData)
        {
            _model3D = model3D;
            //_model3D.Member1.Alignment = ConvertToMemberAlignment(uiData.ExistBeamAlignement.Text);
            UiData = uiData;
            NewBeam = new Beam3D(new Member(), _model3D.IsRoofYDirection);
            //ConvertUIAlignement();
            PrepareBeamExtensions();
            //CalculateNewBeamMovesForAlignement();
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

        //private void CalculateNewBeamMovesForAlignement()
        //{
        //    if (_model3D.IsRoofYDirection)
        //    {
        //        verticalMoveForNewBeam = UiData.BeamHeightValue / 2 / _model3D.Beam3DNo1.CosOfBeamSlope;
        //        verticalEavesZMove = _model3D.Beam3DNo1.Width / _model3D.Beam3DNo1.CosOfBeamSlope;
        //        horizontalEavesXMove = _model3D.Beam3DNo1.Width / 2 / _model3D.Beam3DNo1.SinOfBeamSlope;

        //        normalEavesXMove = _model3D.Beam3DNo1.Width / 2 * _model3D.Beam3DNo1.CosOfBeamSlopeAngleNotCasted;
        //        normalEavesZMove = _model3D.Beam3DNo1.Width / 2 * _model3D.Beam3DNo1.SinOfBeamSlopeAngleNotCasted;

        //        xMoveForNewBeam = UiData.BeamHeightValue / 2 * _model3D.Beam3DNo1.CosOfBeamSlopeAngleNotCasted;
        //        zMoveForNewBeam = UiData.BeamHeightValue / 2 * _model3D.Beam3DNo1.SinOfBeamSlopeAngleNotCasted;
        //    }
        //    else
        //    {
        //        verticalMoveForNewBeam = UiData.BeamHeightValue / 2 / _model3D.Beam3DNo1.CosOfBeamSlope;
        //        verticalEavesZMove = _model3D.Beam3DNo1.Width / _model3D.Beam3DNo1.CosOfBeamSlope;
        //        horizontalEavesXMove = _model3D.Beam3DNo1.Width / 2 / _model3D.Beam3DNo1.SinOfBeamSlope;

        //        normalEavesYMove = _model3D.Beam3DNo1.Width / 2 * _model3D.Beam3DNo1.CosOfBeamSlopeAngleNotCasted;
        //        normalEavesZMove = _model3D.Beam3DNo1.Width / 2 * _model3D.Beam3DNo1.SinOfBeamSlopeAngleNotCasted;

        //        yMoveForNewBeam = UiData.BeamHeightValue / 2 * _model3D.Beam3DNo1.CosOfBeamSlopeAngleNotCasted;
        //        zMoveForNewBeam = UiData.BeamHeightValue / 2 * _model3D.Beam3DNo1.SinOfBeamSlopeAngleNotCasted;
        //    }

        //}

        //public Point3D DetermineBeamOrigin()
        //{
        //    if (UiData.IsRotatedToTheMainTruss)
        //    {
        //        return PrepareOriginForRotatedBeam(_model3D.Beam3DNo1.IsSelectedMemberLeftEdgeOnTop, newBeamAlignment, existBeamAlignment);
        //    }
        //    else
        //    {
        //        return PrepareOriginForNotRotatedBeam(_model3D.Beam3DNo1.IsSelectedMemberLeftEdgeOnTop, newBeamAlignment, existBeamAlignment);
        //    }
        //}

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

        //private void ConvertUIAlignement()
        //{
        //    newBeamAlignment = ConvertToMemberAlignmentNewBeam(UiData.NewBeamAlignement.SelectedItem.ToString());
        //    existBeamAlignment = GetBeamAlignment(UiData.ExistBeamAlignement.SelectedItem.ToString(), Strings.Strings.existBeamAlignement);

        //}

        //private Member.MemberAlignment GetBeamAlignment(string alignmentOption, string alignmentName)
        //{
        //    try
        //    {
        //        return ConvertToMemberAlignment(alignmentOption);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        MessageBox.Show($"{Strings.Strings.convertToMemberAlignementError} {alignmentName}: {ex.Message}");
        //        throw;
        //    }
        //}

        //private Member.MemberAlignment ConvertToMemberAlignmentNewBeam(string alignmentOption)
        //{
        //    string topEdgeTranslation = Strings.Strings.topEdge;
        //    string bottomEdgeTranslation = Strings.Strings.bottomEdge;
        //    string centerTranslation = Strings.Strings.center;

        //    switch (alignmentOption)
        //    {
        //        case var option when option == topEdgeTranslation:
        //            return Member.MemberAlignment.LeftEdge;
        //        case var option when option == bottomEdgeTranslation:
        //            return Member.MemberAlignment.RightEdge;
        //        case var option when option == centerTranslation:
        //            return Member.MemberAlignment.Center;
        //        default:
        //            throw new ArgumentException(Strings.Strings.unknownAlignementOption);
        //    }

        //}

        //private Member.MemberAlignment ConvertToMemberAlignment(string alignmentOption)
        //{
        //    string topEdgeTranslation = Strings.Strings.topEdge;
        //    string bottomEdgeTranslation = Strings.Strings.bottomEdge;
        //    string centerTranslation = Strings.Strings.center;

        //    if (UiData.IsSelectedMemberLeftEdgeOnTop)
        //    {
        //        switch (alignmentOption)
        //        {
        //            case var option when option == topEdgeTranslation:
        //                return Member.MemberAlignment.LeftEdge;
        //            case var option when option == bottomEdgeTranslation:
        //                return Member.MemberAlignment.RightEdge;
        //            case var option when option == centerTranslation:
        //                return Member.MemberAlignment.Center;
        //            default:
        //                throw new ArgumentException(Strings.Strings.unknownAlignementOption);
        //        }
        //    }
        //    else
        //    {
        //        switch (alignmentOption)
        //        {
        //            case var option when option == topEdgeTranslation:
        //                return Member.MemberAlignment.RightEdge;
        //            case var option when option == bottomEdgeTranslation:
        //                return Member.MemberAlignment.LeftEdge;
        //            case var option when option == centerTranslation:
        //                return Member.MemberAlignment.Center;
        //            default:
        //                throw new ArgumentException(Strings.Strings.unknownAlignementOption);
        //        }
        //    }


        //}

        //public Model3DNode.ModelPartAlignment ConvertToModelPartAlignment(string alignmentOption)
        //{
        //    string topEdgeTranslation = Strings.Strings.topEdge;
        //    string bottomEdgeTranslation = Strings.Strings.bottomEdge;
        //    string centerTranslation = Strings.Strings.center;

        //    if (UiData.IsSelectedMemberLeftEdgeOnTop)
        //    {
        //        switch (alignmentOption)
        //        {
        //            case var option when option == topEdgeTranslation:
        //                return Model3DNode.ModelPartAlignment.TopCenter;
        //            case var option when option == bottomEdgeTranslation:
        //                return Model3DNode.ModelPartAlignment.BottomCenter;
        //            case var option when option == centerTranslation:
        //                return Model3DNode.ModelPartAlignment.Center;
        //            default:
        //                throw new ArgumentException(Strings.Strings.unknownAlignementOption);
        //        }
        //    }
        //    else
        //    {
        //        switch (alignmentOption)
        //        {
        //            case var option when option == topEdgeTranslation:
        //                return Model3DNode.ModelPartAlignment.BottomCenter;
        //            case var option when option == bottomEdgeTranslation:
        //                return Model3DNode.ModelPartAlignment.TopCenter;
        //            case var option when option == centerTranslation:
        //                return Model3DNode.ModelPartAlignment.Center;
        //            default:
        //                throw new ArgumentException(Strings.Strings.unknownAlignementOption);
        //        }
        //    }

        //}

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

        //private Point3D PrepareOriginForNotRotatedBeam(bool isLeftEdgeOnTop, Member.MemberAlignment newBeamAlignment, Member.MemberAlignment existBeamAlignment)
        //{
        //    if (isLeftEdgeOnTop)
        //    {
        //        switch (existBeamAlignment)
        //        {
        //            case Member.MemberAlignment.RightEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew - UiData.BeamHeightValue / 2);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew + UiData.BeamHeightValue / 2);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                }

        //            case Member.MemberAlignment.LeftEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew - UiData.BeamHeightValue / 2);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }

        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew + UiData.BeamHeightValue / 2);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                }

        //            default:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);

        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew - UiData.BeamHeightValue / 2);
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);

        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);

        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove + UiData.BeamHeightValue / 2);

        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew + UiData.BeamHeightValue / 2);
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);

        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);

        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);

        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                }
        //        }
        //    }
        //    else
        //    {
        //        switch (existBeamAlignment)
        //        {
        //            case Member.MemberAlignment.RightEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew - UiData.BeamHeightValue / 2);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew + UiData.BeamHeightValue / 2);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                }

        //            case Member.MemberAlignment.LeftEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew - UiData.BeamHeightValue / 2);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew + UiData.BeamHeightValue / 2);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                }

        //            default:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew - UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - UiData.BeamHeightValue / 2);

        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove - UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint - UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew - UiData.BeamHeightValue / 2);
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove + UiData.BeamHeightValue / 2);

        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + UiData.BeamHeightValue / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove + UiData.BeamHeightValue / 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + UiData.BeamHeightValue / 2);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew + UiData.BeamHeightValue / 2);
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                }
        //        }
        //    }

        //}

        //private Point3D PrepareOriginForRotatedBeam(bool isLeftEdgeOnTop, Member.MemberAlignment newBeamAlignment, Member.MemberAlignment existBeamAlignment)
        //{
        //    if (isLeftEdgeOnTop)
        //    {
        //        switch (existBeamAlignment)
        //        {
        //            case Member.MemberAlignment.RightEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew + xMoveForNewBeam, yNew /*+ yMoveForNewBeam*/, zNew - zMoveForNewBeam);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove - xMoveForNewBeam, yNew, zNew + normalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                }

        //            case Member.MemberAlignment.LeftEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint + _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                }

        //            default:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam - normalEavesXMove, yNew, zNew + zMoveForNewBeam + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                }
        //        }
        //    }
        //    else
        //    {
        //        switch (existBeamAlignment)
        //        {
        //            case Member.MemberAlignment.RightEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove + xMoveForNewBeam, yNew, zNew + normalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                }

        //            case Member.MemberAlignment.LeftEdge:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint - _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                        else
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew - _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                }

        //            default:
        //                switch (newBeamAlignment)
        //                {
        //                    case Member.MemberAlignment.RightEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint - zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
        //                        }
        //                    case Member.MemberAlignment.LeftEdge:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + xMoveForNewBeam + normalEavesXMove, yNew, zNew + zMoveForNewBeam + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint + zMoveForNewBeam);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
        //                        }
        //                    default:
        //                        if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.RightEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove2LowestPoint, yNew, zNew - _model3D.Beam3DNo1.VerticalMove2LowestPoint);
        //                            }
        //                        }
        //                        else if (_model3D.Beam3DNo1.Alignement == Member.MemberAlignment.LeftEdge)
        //                        {
        //                            if (_model3D.Beam3DNo1.IsMemberEndHorizontalCut)
        //                            {
        //                                return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndVerticalCut)
        //                            {
        //                                return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
        //                            }
        //                            else if (_model3D.Beam3DNo1.IsMemberEndNormalCut)
        //                            {
        //                                return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
        //                            }
        //                            else
        //                            {
        //                                return new Point3D(xNew + _model3D.Beam3DNo1.HorizontalMove1LowestPoint, yNew, zNew + _model3D.Beam3DNo1.VerticalMove1LowestPoint);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return new Point3D(xNew, yNew, zNew);
        //                        }
        //                }
        //        }
        //    }
        //}
    }
}
