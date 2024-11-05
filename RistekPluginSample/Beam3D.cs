using Epx.Ristek.Data.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace RistekPluginSample
{
    public class Beam3D : Beam2D
    {
        public double DistanceYExistBeam { get; set; }
        public Point3D StartPoint3D { get; set; }
        public Point3D EndPoint3D { get; set; }

        public double Width { get; set; }
        public double Thickness { get; set; }



        public Beam3D(Member member) : base(member)
        {
            SetBeamDimensions(member);
            CalculateBeam3DPoints();
            CalculateBeamSlopeRadians();
            CalculateBeamSlopeDegrees();
            CalculateAngle90MinusBeamSlopeDegrees();
            CalculateAngle90MinusBeamSlopeRadians();

            CosOfBeamSlope = CalculateCosAngle(BeamSlopeRadians);
            SinOfBeamSlope = CalculateSinAngle(BeamSlopeRadians);
            CosOfBeamSlopeAngleNotCasted = CalculateCosAngle(Angle90MinusBeamSlopeRadians);
            SinOfBeamSlopeAngleNotCasted = CalculateSinAngle(Angle90MinusBeamSlopeRadians);
        }

        private void CalculateBeamSlopeDegrees()
        {
            BeamSlopeDegrees = BeamSlopeRadians * 180 / Math.PI;
        }

        private void CalculateAngle90MinusBeamSlopeDegrees()
        {
            Angle90MinusBeamSlopeDegrees = Constants.degrees90 - BeamSlopeDegrees;
        }

        private void CalculateAngle90MinusBeamSlopeRadians()
        {
            Angle90MinusBeamSlopeRadians = Angle90MinusBeamSlopeDegrees * Math.PI / 180;
        }



        private double CalculateCosAngle(double angleInRadians)
        {
            return Math.Cos(angleInRadians);
        }

        private double CalculateSinAngle(double angleInRadians)
        {
            return Math.Sin(angleInRadians);
        }

        private double CalculateTgAngle(double angleInRadians)
        {
            double angleInDegrees = angleInRadians * 180 / Math.PI;
            return Math.Tan(angleInRadians);
        }

        private void SetBeamDimensions(Member member)
        {
            Width = member.Width;
            Thickness = member.Thickness;
        }

        private void CalculateBeam3DPoints()
        {
            StartPoint3D = new Point3D(MemberStartX, DistanceY, MemberStartY);
            EndPoint3D = new Point3D(MemberEndX, DistanceY, MemberEndY);
        }

        protected void CalculateBeamSlopeRadians()
        {
            double deltaX = Math.Abs(EndPoint3D.X - StartPoint3D.X);
            double deltaZ = Math.Abs(EndPoint3D.Z - StartPoint3D.Z);

            if (IsMemberEndCombinedCut && Alignement == Member.MemberAlignment.Center)
            {
                deltaX = Math.Abs(LeftEdgeEndPointX - LeftEdgeStartPointX);
                deltaZ = Math.Abs(LeftEdgeEndPointY - LeftEdgeStartPointY);
            }

            BeamSlopeRadians = Math.Atan2(deltaZ, deltaX);
        }

    }
}
