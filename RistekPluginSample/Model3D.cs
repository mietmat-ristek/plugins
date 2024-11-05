using Epx.Ristek.Data.Models;
using System.Collections.Generic;

namespace RistekPluginSample
{
    public class Model3D
    {
        public Beam3D Beam3DNo1 { get; set; }
        public Beam3D Beam3DNo2 { get; set; }

        public Member Member1 { get; set; }
        public Member Member2 { get; set; }

        public double Member1StartPointXEdgeLeft { get; set; }
        public double Member1StartPointZEdgeLeft { get; set; }
        public double Member1EndPointXEdgeLeft { get; set; }
        public double Member1EndPointZEdgeLeft { get; set; }
        public double Member2EndPointXEdgeLeft { get; set; }
        public double Member2EndPointZEdgeLeft { get; set; }       

        public double SelectedBeamLength { get; set; }


        public Model3D(Member member1, Member member2)
        {
            Member1 = member1;
            Member2 = member2;
            Beam3DNo1 = new Beam3D(member1);
            Beam3DNo2 = new Beam3D(member2);
            SelectedBeamLength = member1.Length;
            SetPointsForLeftEdge();
        }     

        public void SetPointsForLeftEdge()
        {
            Member1StartPointXEdgeLeft = Beam3DNo1.LeftEdgeStartPointX;
            Member1StartPointZEdgeLeft = Beam3DNo1.LeftEdgeStartPointY;
            Member1EndPointXEdgeLeft = Beam3DNo1.LeftEdgeEndPointX;
            Member1EndPointZEdgeLeft = Beam3DNo1.LeftEdgeEndPointY;
            Member2EndPointXEdgeLeft = Beam3DNo2.LeftEdgeEndPointX;
            Member2EndPointZEdgeLeft = Beam3DNo2.LeftEdgeEndPointY;
        }
    }
}
