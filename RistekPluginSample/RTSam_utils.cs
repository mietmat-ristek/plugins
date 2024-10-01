using Epx.BIM;
using Epx.BIM.GeometryTools;
using Epx.BIM.Models;
using Epx.BIM.Plugins;
using Epx.Ristek.Data.Models;
using Epx.Ristek.Data.Plugins;
using Epx.Ristek.ParametricTrusses.PluginParametricTrusses;
using RistekPluginSample.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Media.Media3D;
//using BeamTrussToolRSTSamBase = Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss;
using BeamTrussToolRSTSamBase = RistekPluginSample.AdmSamplePlugin;
using ParametricTrussRTSam = Epx.Ristek.Data.Models.ParametricTruss;

namespace RistekPluginSample
{
    public class ParametricTrussMyHelper
    {

        #region base

        public BeamTrussToolRSTSamBase m_trussToolPassed { get; private set; }
        public ParametricTrussMyHelper(BeamTrussToolRSTSamBase _trussToolIntegrated)
        {
            m_trussToolPassed = _trussToolIntegrated;
        }

        #endregion

        #region methods

        /// <summary>
        /// Generates the parametric truss. Length is defined by <paramref name="origin"/> and <paramref name="directionPoint"/>.
        /// </summary>
        /// <param name="trussTool">The truss tool.</param>
        /// <param name="height">The height.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="directionPoint">The direction point.</param>
        /// <returns>ParametricTruss.</returns>
        public ParametricTrussRTSam GenerateParametricTruss(BeamTrussToolRSTSamBase trussTool, double height, Point3D origin, Point3D directionPoint)
        //public ParametricTrussRTSam GenerateParametricTruss(ref BeamTrussToolRSTSamBase trussTool, double height, Point3D origin, Point3D directionPoint)
        {
            if (trussTool == null)
            {
                // Choose the paramteric truss tool: "ParametricTrusses.PluginParametricTrusses" are new style, "ParametricTrusses" are old style. Please use new style.
                //trussTool = new Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss();
                // not needed actually, as m_trussToolPassed is not to be changed
                //trussTool = new BeamTrussToolRSTSamIntegrated(this);
                throw new NotImplementedException();
                //trussTool = new Epx.Ristek.ParametricTrusses.BeamTruss();
            }
            // Create parametric truss datamodel instance
            // wrong
            //ParametricTruss truss = new ParametricTruss() { Name = "Name" };
            // correct
            //ParametricTruss truss = new ParametricTruss("BeamTruss wind");
            //ParametricTrussRTSam truss = new ParametricTrussRTSam(Strings.Strings._beamTrussNodeName, this);
            // 20240328 Knaga
            //ParametricTrussRTSam truss = new ParametricTrussRTSam(Strings.Strings._beamTrussNodeName);
            ParametricTrussRTSam truss;
            if (!this.m_trussToolPassed.Flag_isRistekPlugin_isKnagaMode)
            {
                truss = new ParametricTrussRTSam(Strings.Strings._beamTrussNodeName);
            }
            else //if (this.m_trussToolPassed.Flag_isRistekPlugin_isKnagaMode)
            {
                truss = new ParametricTrussRTSam(Strings.Strings._beamTrussKnagaNodeName);
            }

            truss.AssemblyName = trussTool.AssemblyName;
            truss.FullClassName = trussTool.FullClassName;

            if (trussTool is Epx.Ristek.ParametricTrusses.PluginParametricTrusses.PluginBaseTruss pbt) // new style parametric truss tool
            {
                pbt.IsInitializationWithTruss = true;
                // set the datamodel instance to the tool
                // 20240626 adm workarout after 3Dt update 5.0.269 (not needed? unchecked)
                //pbt.InitializeMyTruss(truss, target: null); // target-node may be required for project level settings access, get from Project.GetTargetFolder() 
                pbt.InitializeMyTruss(truss, target: trussTool.getProjectObj().GetTargetFolder()); // target-node may be required for project level settings access, get from Project.GetTargetFolder() 
                // tmp? (juz zapomnialem dlaczego tmp, zostawic jak jest?)
                //pbt.InitializeMyTruss(truss, target: m_PluginTool.Master);
                //pbt.InitializeMyTruss(truss, target: m_trussToolPassed.Master);
                // tmp integrated (odkomentowac?) - zostawic, na nic negatywnie nie wplywa
                //pbt.IsInitializationWithTruss = false;

                /*
                // choose member arrangement type
                var tyypit = pbt.ParametricTrussTypes;
                // set the chosen type
                pbt.TrussType = tyypit[0];
                */
                trussTool.setTrussType(ref pbt, height, origin, directionPoint);
            }
            else // old style parametric truss tool, please use new style
            {
                trussTool.IsInitializationWithTruss = true;
                // set the datamodel instance to the tool
                trussTool.Initialize(truss, target: null, calculateCuttingLengths: false, isInEdit: false); // target-node may be required for project level settings access, get from Project.GetTargetFolder() 
                // tmp? (juz zapomnialem dlaczego tmp, zostawic jak jest?)
                //trussTool.Initialize(truss, target: m_PluginTool.Master, calculateCuttingLengths: false, isInEdit: false); 
                trussTool.IsInitializationWithTruss = false;

                // bo odkomentowac i obsluzyc (BeamTrussToolRTSam juz sie nie da rzutowac tak)
                throw new NotImplementedException();
                // old style tools need to be controlled more specifically for each type
                /*
                if ( trussTool is Epx.Ristek.ParametricTrusses.BeamTruss bt)
                {
                    // set member arrangement type
                    bt.WindTrussType = WindTrussType.WindTruss1;
                }
                else if (trussTool is Epx.Ristek.ParametricTrusses.SymmetricTruss ridgeTruss)
                {
                    ridgeTruss.MemberArrangementType = SymmetricTrussMemberArrangementType.Type1;
                }
                */
                // etc.
            }

            // set generic settings through truss tool "Common External Interface" functions
            double length = (directionPoint - origin).Length;
            //trussTool.SetChords(topChordWidth, bottomChordWidth);
            //trussTool.SetEavesType(eavesType, eavesInputType, createWedge, wedgeWidth, wedgeMaterial, wedgePlate);
            //trussTool.LoadsGenerator.SetLoads(_loadSnow, loadSnowType == 0 ? SnowLoadMagnitudeType.AtGround : SnowLoadMagnitudeType.AtRoof, loadDeadWeightTopChord, loadDeadWeightBottomChord, loadWind, barrierAtRoof);

            trussTool.SetDimensions(length, null, null, null, height, null, null, null, null, null);
            // adm working workarout
            trussTool.Height = height;

            // create the geometry and set it to the datamodel instance
            trussTool.CreateModelCustom(false);

            // truss location
            truss.Origin = origin;
            ////truss.Alignment = Epx.BIM.Models.Model3DNode.ModelPartAlignment.BottomCenter; // BottomLeft, BottomCenter, BottomRight
            //truss.Alignment = Epx.BIM.Models.Model3DNode.ModelPartAlignment.Center; // BottomLeft, BottomCenter, BottomRight
            //truss.AdjustOriginToAlignedPoint();
            // truss direction
            //truss.SetXAxis(directionPoint - origin, new Vector3D(0, 0, 1)); // the normal is usually the Z-axis of the active grid, often (0,0,1)

            // done
            return truss;
        }

        #endregion

    }

    /*
    public class ParametricTrussRTSam : ParametricTruss//, IModelDimensionController
    {
        // have to be public set for lldf data save/load (edit: probably not, or at least not enough)
        public ParametricTrussMyHelper m_ParametricTrussMyHelper { get; private set; }
        public ParametricTrussRTSam(string name, ParametricTrussMyHelper _ParametricTrussMyHelper)
            : base(name)
        {
            m_ParametricTrussMyHelper = _ParametricTrussMyHelper;
        }

        public ParametricTrussRTSam()
            : base()
        {

        }

        // doesnt work, is not called
        /*
        void IModelDimensionController.DimensionEdited(ModelDimensionLine dimensionLine, double newValue, DimensionEditType editType)
        {
            throw new NotImplementedException();
        }

        void IModelDimensionController.ContentElementEdited(ModelOverlayContentElement moce, object newValue)
        {
            throw new NotImplementedException();
        }
        * /
    }
    */

    public class ModelDimensionLineRTSam : ModelDimensionLine
    {
        public ModelDimensionLineRTSam(ParametricTruss parametricTruss) : base(parametricTruss)
        {

        }

    }

    public enum CustomStateEnum { _pre, _during, _post }
}
