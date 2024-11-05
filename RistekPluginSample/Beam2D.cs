using Epx.Ristek.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace RistekPluginSample
{
    public class Beam2D
    {
        public bool IsSelectedMemberLeftEdgeOnTop { get; set; }
        public bool IsMemberEndVerticalCut { get; set; }
        public bool IsMemberEndHorizontalCut { get; set; }
        public bool IsMemberEndNormalCut { get; set; }
        public bool IsMemberEndCombinedCut { get; set; }
        public Point TheLowestStartPointForCombinedCuttedBeam { get; set; }
        public Point TheHighestStartPointForCombinedCuttedBeam { get; set; }

        public double LeftEdgeStartPointX { get; set; }
        public double LeftEdgeStartPointY { get; set; }
        public double LeftEdgeEndPointX { get; set; }
        public double LeftEdgeEndPointY { get; set; }

        protected double RightEdgeStartPointX { get; set; }
        protected double RightEdgeStartPointY { get; set; }
        protected double RightEdgeEndPointX { get; set; }
        protected double RightEdgeEndPointY { get; set; }

        protected double MiddleEdgeStartPointX { get; set; }
        protected double MiddleEdgeStartPointY { get; set; }
        protected double MiddleEdgeEndPointX { get; set; }
        protected double MiddleEdgeEndPointY { get; set; }

        public double HorizontalMove1LowestPoint { get; set; }
        public double HorizontalMove1HighestPoint { get; set; }
        public double VerticalMove1LowestPoint { get; set; }
        public double VerticalMove1HighestPoint { get; set; }

        public double HorizontalMove2LowestPoint { get; set; }
        public double HorizontalMove2HighestPoint { get; set; }
        public double VerticalMove2LowestPoint { get; set; }
        public double VerticalMove2HighestPoint { get; set; }

        public double MemberStartX { get; set; }
        public double MemberStartY { get; set; }
        public double MemberEndX { get; set; }
        public double MemberEndY { get; set; }

        public double MemberLength { get; set; }
        public double DistanceY { get; set; }

        public double BeamSlopeRadians { get; set; }
        public double BeamSlopeDegrees { get; set; }
        public double Angle90MinusBeamSlopeDegrees { get; set; }
        public double Angle90MinusBeamSlopeRadians { get; set; }

        public double CosOfBeamSlope { get; set; }
        public double SinOfBeamSlope { get; set; }
        public double CosOfBeamSlopeAngleNotCasted { get; set; }
        public double SinOfBeamSlopeAngleNotCasted { get; set; }

        public Member.MemberAlignment Alignement { get; set; }


        public Beam2D(Member member)
        {
            SetMemberLength(member);
            SetEdgePoints(member);
            CheckIsSelectedObjectLeftEdgeOnTop(member);
            SetMemberEndCut(member);
            SetMiddlePointsForCombinedEaves(member);
            SetAlignement(member);
            SetBorderPoints();
            DistanceY = member.PartCSToGlobal.OffsetY;
            if (IsMemberEndCombinedCut)
            {
                SetMovesForCombinedCutMiddlePoint();
            }

          
        }
               
        protected void SetMemberLength(Member member)
        {
            MemberLength = member.Length;
        }

        protected void SetEdgePoints(Member member)
        {

            LeftEdgeStartPointX = Math.Round(member.LeftGeometryEdgeLine.StartPoint.X, 0);
            LeftEdgeStartPointY = Math.Round(member.LeftGeometryEdgeLine.StartPoint.Y, 0);
            LeftEdgeEndPointX = Math.Round(member.LeftGeometryEdgeLine.EndPoint.X, 0);
            LeftEdgeEndPointY = Math.Round(member.LeftGeometryEdgeLine.EndPoint.Y, 0);

            RightEdgeStartPointX = Math.Round(member.RightGeometryEdgeLine.StartPoint.X, 0);
            RightEdgeStartPointY = Math.Round(member.RightGeometryEdgeLine.StartPoint.Y, 0);
            RightEdgeEndPointX = Math.Round(member.RightGeometryEdgeLine.EndPoint.X, 0);
            RightEdgeEndPointY = Math.Round(member.RightGeometryEdgeLine.EndPoint.Y, 0);

            MiddleEdgeStartPointX = Math.Round(member.MiddleCuttedLineWithBooleanCuts.StartPoint.X, 0);
            MiddleEdgeStartPointY = Math.Round(member.MiddleCuttedLineWithBooleanCuts.StartPoint.Y, 0);
            MiddleEdgeEndPointX = Math.Round(member.MiddleCuttedLineWithBooleanCuts.EndPoint.X, 0);
            MiddleEdgeEndPointY = Math.Round(member.MiddleCuttedLineWithBooleanCuts.EndPoint.Y, 0);
        }

        protected void CheckIsSelectedObjectLeftEdgeOnTop(Member member)
        {
            IsSelectedMemberLeftEdgeOnTop = member.StartPoint.X < member.EndPoint.X;
        }

        protected void SetAlignement(Member member)
        {
            Alignement = member.Alignment;
        }

        protected void SetMemberEndCut(Member member)
        {
            if (Math.Round(LeftEdgeStartPointX, 2) == Math.Round(RightEdgeStartPointX, 2))
            {
                IsMemberEndVerticalCut = true;
            }
            else if (LeftEdgeStartPointY == RightEdgeStartPointY)
            {
                IsMemberEndHorizontalCut = true;
            }
            else if (member.Geometry.Count > 5)
            {
                IsMemberEndCombinedCut = true;
            }
            else
            {
                IsMemberEndNormalCut = true;
            }
        }

        protected void SetMiddlePointsForCombinedEaves(Member member)
        {
            if (IsSelectedMemberLeftEdgeOnTop)
            {
                TheLowestStartPointForCombinedCuttedBeam = member.MyMemberCuts
                                      .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                                      .OrderBy(point => point.Y)
                                      .ThenBy(point => point.X)
                                      .FirstOrDefault();

                TheHighestStartPointForCombinedCuttedBeam = member.MyMemberCuts
                    .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                   .OrderByDescending(point => point.Y)
                   .ThenByDescending(point => point.X)
                   .FirstOrDefault();
            }
            else
            {
                TheLowestStartPointForCombinedCuttedBeam = member.MyMemberCuts
                  .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                  .OrderBy(point => point.Y)
                  .OrderByDescending(point => point.X)
                  .FirstOrDefault();

                TheHighestStartPointForCombinedCuttedBeam = member.MyMemberCuts
                    .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                   .OrderByDescending(point => point.Y)
                   .ThenBy(point => point.X)
                   .FirstOrDefault();
            }
        }

        protected void SetMovesForCombinedCutMiddlePoint()
        {

            if (IsSelectedMemberLeftEdgeOnTop)
            {
                HorizontalMove1LowestPoint = Math.Abs(RightEdgeStartPointX - TheLowestStartPointForCombinedCuttedBeam.X);
                VerticalMove1LowestPoint = Math.Abs(RightEdgeStartPointY - TheLowestStartPointForCombinedCuttedBeam.Y);
                HorizontalMove1HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.X - LeftEdgeEndPointX);
                VerticalMove1HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.Y - LeftEdgeEndPointY);

                HorizontalMove2LowestPoint = Math.Abs(LeftEdgeStartPointX - TheLowestStartPointForCombinedCuttedBeam.X);
                VerticalMove2LowestPoint = Math.Abs(LeftEdgeStartPointY - TheLowestStartPointForCombinedCuttedBeam.Y);
                HorizontalMove2HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.X - RightEdgeEndPointX);
                VerticalMove2HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.Y - RightEdgeEndPointY);
            }
            else
            {
                HorizontalMove1LowestPoint = Math.Abs(LeftEdgeStartPointX - TheLowestStartPointForCombinedCuttedBeam.X);
                VerticalMove1LowestPoint = Math.Abs(LeftEdgeStartPointY - TheLowestStartPointForCombinedCuttedBeam.Y);
                HorizontalMove1HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.X - RightEdgeEndPointX);
                VerticalMove1HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.Y - RightEdgeEndPointY);

                HorizontalMove2LowestPoint = Math.Abs(RightEdgeStartPointX - TheLowestStartPointForCombinedCuttedBeam.X);
                VerticalMove2LowestPoint = Math.Abs(RightEdgeStartPointY - TheLowestStartPointForCombinedCuttedBeam.Y);
                HorizontalMove2HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.X - LeftEdgeEndPointX);
                VerticalMove2HighestPoint = Math.Abs(TheHighestStartPointForCombinedCuttedBeam.Y - LeftEdgeEndPointY);
            }

        }

        protected void SetBorderPoints()
        {
            if (Alignement == Member.MemberAlignment.LeftEdge)
            {
                MemberStartX = Math.Round(LeftEdgeStartPointX, 0);
                MemberStartY = Math.Round(LeftEdgeStartPointY, 0);
                MemberEndX = Math.Round(LeftEdgeEndPointX, 0);
                MemberEndY = Math.Round(LeftEdgeEndPointY, 0);
            }
            else if (Alignement == Member.MemberAlignment.RightEdge)
            {
                MemberStartX = Math.Round(RightEdgeStartPointX, 0);
                MemberStartY = Math.Round(RightEdgeStartPointY, 0);
                MemberEndX = Math.Round(RightEdgeEndPointX, 0);
                MemberEndY = Math.Round(RightEdgeEndPointY, 0);
            }
            else
            {
                if (IsMemberEndCombinedCut)
                {
                    MemberStartX = Math.Round(TheLowestStartPointForCombinedCuttedBeam.X, 0);
                    MemberStartY = Math.Round(TheLowestStartPointForCombinedCuttedBeam.Y, 0);
                    MemberEndX = Math.Round(TheHighestStartPointForCombinedCuttedBeam.X, 0);
                    MemberEndY = Math.Round(TheHighestStartPointForCombinedCuttedBeam.Y, 0);
                }
                else
                {
                    MemberStartX = Math.Round(MiddleEdgeStartPointX, 0);
                    MemberStartY = Math.Round(MiddleEdgeStartPointY, 0);
                    MemberEndX = Math.Round(MiddleEdgeEndPointX, 0);
                    MemberEndY = Math.Round(MiddleEdgeEndPointY, 0);
                }
            }

        }
    }
}
