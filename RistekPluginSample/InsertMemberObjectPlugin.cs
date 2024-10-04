using Epx.BIM;
using Epx.BIM.GeometryTools;
using Epx.BIM.Models;
using Epx.BIM.Plugins;
using Epx.Ristek.Data.Models;
//using Epx.Ristek.ParametricTrusses;
using Epx.Ristek.Data.Plugins;
//using Epx.Ristek.ParametricTrusses.PluginParametricTrusses;
//using Epx.BIM.BaseTools;
using RistekPluginSample.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Media.Media3D;
using BeamTrussToolRSTSamBaseInsertMember = RistekPluginSample.InsertMemberObjectPlugin;
using ParametricTrussRTSamInsertMember = Epx.Ristek.Data.Models.ParametricTruss;

namespace RistekPluginSample
{
    public class InsertMemberObjectPlugin : Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss,
        IModelViewPlugin,
        IRibbonViewPlugin,
        IRibbonDialogPlugin,
        ITemporaryCurrentTruss
    {
        #region base

        public BuildingNode BuildingNodeForThis
        {
            get
            {
                return (this.Master as BuildingNode);
            }
        }

        public InputPhaseRibbonControl m_InputPhaseRibbonControl { get; private set; }
        public static DataFieldsRawContainer DataFieldsRawObject { get; private set; }
        DataFieldsRawContainer m_DataFieldsRawObject_nonStatic;
        public DataFieldsRawContainer DataFieldsRawObject_nonStatic
        {
            get
            {
                return m_DataFieldsRawObject_nonStatic;
            }
            set
            {
                m_DataFieldsRawObject_nonStatic = value;
            }
        }

        public void BeamTrussToolRSTSamIntegratedCustom()
        {
            // init
            this.m_Flag_isRistekPlugin_isKnagaMode = false;

            DataRawDeserialize();
            initDataFieldsRaw();

            m_InputPhaseRibbonControl = new InputPhaseRibbonControl(this.RTSamPluginVersionStr);
            m_DataFieldsRawObject_nonStatic.IsRistekPluginInitializedFinal_technicalExternallySettedFlag = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleStructure"/> class.
        /// </summary>
        public InsertMemberObjectPlugin()
            : base()
        {
            m_resTrussStorage = null;
            m_resTrussInitialized = false;
            m_ShowDialogState = CustomStateEnum._pre;

            BeamTrussToolRSTSamIntegratedCustom();
        }

        private void initDataFieldsRaw()
        {
            if (DataFieldsRawObject == null)
            {
                DataFieldsRawObject = new DataFieldsRawContainer();
            }
            else
            {
                DataFieldsRawObject.IsRistekPluginInitializedFinal_technicalExternallySettedFlag = false;
            }
            DataFieldsRawObject_nonStatic = DataFieldsRawObject;
            DataFieldsRawObject_nonStatic.RemovePropiertyChangeEvents();
            //DataFieldsRawObject_nonStatic.RistekPluginMaster = this;
            DataFieldsRawObject_nonStatic.PropertyChanged += DataFieldsObjectPropertyChangedListener;

            // 20240328 Knaga
            if (!this.Flag_isRistekPlugin_isKnagaMode)
            {
                DataFieldsRawObject_nonStatic.AutoaddSupports = true;
            }
        }

        private void DataFieldsObjectPropertyChangedListener(object sender, PropertyChangedEventArgs e)
        {
            if (this.DataFieldsRawObject_nonStatic == null)
            { return; }

            if (this.Flag_isRistekPlugin_isDuringDeserialization)
            { return; }

            if (!DataFieldsRawObject_nonStatic.IsRistekPluginInitializedFinal_technicalExternallySettedFlag)
            { return; }

            if (e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOffsetPerpendicular_mm)
                || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseBottom_mm)
                || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseTop_mm)
                || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOutreachPerpPrimary_mm)
                || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOutreachPerpSeccondary_mm)
                || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.AutosetTrussParamsInEditMode)
                || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.PlaneAlignement)
                //|| e.PropertyName == nameof(DataFieldsRawObject_nonStatic.PlaneAlignementWrapperObj)
                )
            {
                if (!this.m_resTrussInitialized)
                { return; }

                bool _forceRegenerateMainTruss =
                    e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseBottom_mm)
                    || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseTop_mm)
                    || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOutreachPerpPrimary_mm)
                    || e.PropertyName == nameof(DataFieldsRawObject_nonStatic.BeamTrussOutreachPerpSeccondary_mm)
                    || (e.PropertyName == nameof(DataFieldsRawObject_nonStatic.AutosetTrussParamsInEditMode)
                        && DataFieldsRawObject_nonStatic.AutosetTrussParamsInEditMode
                        )
                    ;

                getBeamTrussLocationAdjustedSingletonOrFetched(_forceRegenerateMainTruss);
                // to not fire double inside getBeamTrussLocationAdjustedSingleton
                if (!_forceRegenerateMainTruss)
                {
                    // overlays doesnt work if not
                    this.m_resTrussStorage = this.m_resTrussStorage;
                    fireTrussFinalLocationAdjustmentChange();
                }
                handleBeamTrussInModel();
            }

            // after migration to Integrated and all changes have to refresh i preDialog or crashes (so is placed above)
            /*
            if (e.PropertyName == nameof(DataFieldsRawObject_nonStatic.AutosetTrussParamsInEditMode))
            {
                resetTrussRTSamParams();
            }
            */
        }

        public string RTSamPluginVersionStr
        {
            get
            {
                string versionBase = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string pluginVersionStr = String.Format("v{0} ({1})", versionBase, PreBuildParamsAutogeneratedClass.BuildDate);
#if DEBUG
                pluginVersionStr += " _ " + @"IN_DEBUG_MODE";
#endif

                return pluginVersionStr;
            }
        }

        #endregion

        #region compute flags

        private bool m_Flag_isRistekPlugin_isDuringDeserialization;
        public bool Flag_isRistekPlugin_isDuringDeserialization
        {
            get
            {
                return m_Flag_isRistekPlugin_isDuringDeserialization;
            }
            private set
            {
                m_Flag_isRistekPlugin_isDuringDeserialization = value;
            }
        }

        private bool m_Flag_isRistekPlugin_isKnagaMode;
        public bool Flag_isRistekPlugin_isKnagaMode
        {
            get
            {
                return m_Flag_isRistekPlugin_isKnagaMode;
            }
            private set
            {
                m_Flag_isRistekPlugin_isKnagaMode = value;
                this.DataFieldsRawObject_nonStatic.IsKnagaMode_technicalExternallySettedFlag = this.m_Flag_isRistekPlugin_isKnagaMode;
                if (m_Flag_isRistekPlugin_isKnagaMode == true)
                {
                    this.DataFieldsRawObject_nonStatic.AutoaddSupports = false;
                }
            }
        }

        public const bool __IS_RistekPlugin_endPreDialogWithEnter = true;

        #endregion

        #region de/serialize

        public void DataRawSerialize()
        {
            SerializableDocument _SerializableDocumentCurrent;
            _SerializableDocumentCurrent = new SerializableDocument();
            _SerializableDocumentCurrent.DataFieldsRawContainerObject = this.DataFieldsRawObject_nonStatic;
            MainController.SaveFile(_SerializableDocumentCurrent);
        }

        public void DataRawDeserialize()
        {
            this.Flag_isRistekPlugin_isDuringDeserialization = true;

            SerializableDocument _SerializableDocumentCurrent = new SerializableDocument();
            _SerializableDocumentCurrent.DataFieldsRawContainerObject = new DataFieldsRawContainer();
            //_SerializableDocumentCurrent.DataFieldsRawContainerObject.RistekPluginMaster = this;
            bool _fileOpened = MainController.OpenFile(out _SerializableDocumentCurrent, true);
            if (_fileOpened
                && _SerializableDocumentCurrent != null
                && _SerializableDocumentCurrent.DataFieldsRawContainerObject != null
                )
            {
                DataFieldsRawObject = _SerializableDocumentCurrent.DataFieldsRawContainerObject;
            }

            this.Flag_isRistekPlugin_isDuringDeserialization = false;
        }

        #endregion

        #region plugin engine overrides

        #region PluginTool

        public override void InitializePluginParameters(PluginTool initialPlugin)
        {
            base.InitializePluginParameters(initialPlugin);
        }

        /// <summary>
        /// Get accepted target type/selection for which plugin is available.
        /// </summary>
        /// <value>The type of the accepted master.</value>
        public override Type AcceptedMasterNodeType
        {
            get
            {
                return typeof(BuildingNode);
            }
        }

        /// <summary>
        /// Determines whether the specified node is a valid target node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns><c>true</c> if [is valid target] [the specified node]; otherwise, <c>false</c>.</returns>
        public override bool IsValidTarget(BaseDataNode node)
        {
            // Actual data of the actual target node can be checked here.
            return node is BuildingNode;
        }

        /*
        /// <summary>
        /// Custom1 is the Design View.
        /// </summary>
        public override List<PluginViewModes> AllowedViewModes => new List<PluginViewModes>() { PluginViewModes.View3D, PluginViewModes.Custom1 };
        */

        /// <summary>
        /// Name shown for user in drop down menu
        /// </summary>
        public override string NameForMenu
        {
            get
            {
                // Localized in the Resx Dictionary. The dictionary base name must be Strings.resx and it must be placed in a folder named Strings in the project root.
                return Strings.Strings.insert_member_object_plugin;
            }
        }

        /// <summary>
        /// Gets the menu tool tip.
        /// </summary>
        /// <value>The menu tool tip.</value>
        public override object MenuToolTip
        {
            get
            {
                return Strings.Strings._MenuToolTipInsertMemberObject;
            }
        }

        /// <summary>
        /// Input object storage method 1.
        /// </summary>
        private List<PluginInput> _preInputs = null;
        /// <summary>
        /// Gets the next pre-dialog input. There are two methods to keep track of the inputs inside the plugin:
        /// 1. Initialize a private list with all of the inputs once and return the one with the next index. The input
        /// value is stored in the input instance in the list.
        /// 2. Always create new inputs based on the index of the previousInput but store the value of the input
        /// in a private field when the input is validated in the IsPreDialogInputValid function.
        /// <see cref="IsInEditMode"/> should be checked, input can be different for create and edit modes.
        /// </summary>
        /// <param name="previousInput">The previous input.</param>
        /// <returns>PluginInput.</returns>
        public override PluginInput GetNextPreDialogInput(PluginInput previousInput)
        {
            if (this.IsInEditMode)
            {
                return null;
            }

            if (_preInputs == null)
            {
                _preInputs = new List<PluginInput>();

                {
                    PluginObjectInput trussInput_extended = new PluginObjectInput();
                    trussInput_extended.Index = 0;
                    trussInput_extended.Prompt = Strings.Strings._preInputs0_extended;
                    _preInputs.Add(trussInput_extended);
                }
            }
            // 20240328 Knaga
            if (_preInputs.Count == 1
                && previousInput != null
                && previousInput.Index == 0
                )
            {
                if (this.Flag_isRistekPlugin_isKnagaMode)
                {
                    PluginObjectInput trussInput0_repeated = new PluginObjectInput();
                    trussInput0_repeated.Index = this.PreDialogInputFirstMemeberOffset + 0;
                    trussInput0_repeated.Prompt = Strings.Strings._preInputs0_knaga;
                    _preInputs.Add(trussInput0_repeated);
                }
                {
                    PluginObjectInput trussInput1 = new PluginObjectInput();
                    trussInput1.Index = this.PreDialogInputFirstMemeberOffset + 1;
                    trussInput1.Prompt = Strings.Strings._preInputs1;
                    _preInputs.Add(trussInput1);
                }
                if (__IS_RistekPlugin_endPreDialogWithEnter)
                {
                    PluginObjectInput trussInput2 = new PluginObjectInput();
                    trussInput2.Index = this.PreDialogInputFirstMemeberOffset + 2;
                    trussInput2.Prompt = Strings.Strings._preInputs2;
                    _preInputs.Add(trussInput2);
                }
            }

            if (previousInput == null) return _preInputs != null ? _preInputs[0] : null;
            else if (previousInput.Index + 1 < _preInputs.Count) return _preInputs[previousInput.Index + 1];
            else return null;
        }

        // 20240328 Knaga
        private int PreDialogInputFirstMemeberOffset = 0;
        /// <summary>
        /// Determines whether the specified plugin input is valid.
        /// </summary>
        /// <param name="pluginInput">The plugin input.</param>
        /// <returns><c>true</c> if [is pre dialog input valid] [the specified plugin input]; otherwise, <c>false</c>.</returns>
        public override bool IsPreDialogInputValid(PluginInput pluginInput)
        {
            bool _wrongObjectMarker = true;

            // 20240328 Knaga
            if (pluginInput.Index == 0)
            {
                PluginObjectInput ppi = pluginInput as PluginObjectInput;
                if (ppi != null
                    && ppi.Object is Support
                    )
                {
                    if (pluginInput.Index != 0)
                    { throw new NotImplementedException(); }

                    this.Flag_isRistekPlugin_isKnagaMode = true;
                    this.PreDialogInputFirstMemeberOffset = 1;

                    pluginInput.Valid = true;
                    return true;
                }
                // mostly redundant
                else
                {
                    this.Flag_isRistekPlugin_isKnagaMode = false;
                    this.PreDialogInputFirstMemeberOffset = 0;
                }
            }

            if (pluginInput.Index == this.PreDialogInputFirstMemeberOffset + 0)
            {
                PluginObjectInput ppi = pluginInput as PluginObjectInput;
                if (ppi != null
                    && ppi.Object is Member
                    && !this.Flag_isRistekPlugin_isKnagaMode
                    && getParametricTrussFromPluginInput(pluginInput) != null
                    )
                {
                    pluginInput.Valid = true;
                    return true;
                }
                // 20240328 Knaga
                else if (ppi != null
                    && ppi.Object is Member
                    && this.Flag_isRistekPlugin_isKnagaMode
                    )
                {
                    if (!IsMemberAboveSupport(getKnagaMainSupport(), ppi.Object as Member))
                    {
                        pluginInput.Valid = false;
                        pluginInput.ErrorMessage = Strings.Strings._errorMembersNotAboveSupport + ": " + pluginInput.Prompt;
                        return false;
                    }
                    else
                    {
                        pluginInput.Valid = true;
                        return true;
                    }
                }
            }
            else if (pluginInput.Index == this.PreDialogInputFirstMemeberOffset + 1)
            {
                PluginObjectInput ppi = pluginInput as PluginObjectInput;
                if (ppi == null
                    || !(ppi.Object is Member)
                    || getParametricTrussFromPluginInput(pluginInput) == null
                    )
                {
                    // w sumie redundantne, moze nic nie byc
                    _wrongObjectMarker = true;
                }
                else
                {
                    ParametricTruss _pt0 = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                    ParametricTruss _pt1 = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + 1]);

                    Member _m0_ = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                    Member _m1_ = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 1]);

                    if (_pt0 == _pt1)
                    {
                        pluginInput.Valid = false;
                        pluginInput.ErrorMessage = Strings.Strings._errorSameTruss + ": " + pluginInput.Prompt;
                        return false;
                    }
                    else
                    {
                        if (!MyUtils.ArePararrelTrusses(_pt0, _pt1))
                        {
                            pluginInput.Valid = false;
                            pluginInput.ErrorMessage = Strings.Strings._errorTrussesNotParallel + ": " + pluginInput.Prompt;
                            return false;
                        }
                        // w sumie redundantne, moze nic nie byc
                        else if (!MyUtils.ArePararrelMembers(_m0_, _m1_)
                            //&& !this.Flag_isRistekPlugin_isKnagaMode
                            )
                        {
                            pluginInput.Valid = false;
                            pluginInput.ErrorMessage = Strings.Strings._errorMembersNotParallel + ": " + pluginInput.Prompt;
                            return false;
                        }
                        else
                        {
                            handleBeamTrussInModel();

                            pluginInput.Valid = true;
                            return true;
                        }
                    }
                }
            }
            else if (pluginInput.Index == this.PreDialogInputFirstMemeberOffset + 2)
            {
                _wrongObjectMarker = false;

                if (pluginInput is PluginKeyInput)
                {
                    PluginKeyInput _pluginKeyInputCasted = pluginInput as PluginKeyInput;
                    if (_pluginKeyInputCasted.InputKey == System.Windows.Input.Key.Enter)
                    {
                        pluginInput.Valid = true;
                        return true;
                    }
                }

                pluginInput.Valid = false;
                return false;
            }

            if (_wrongObjectMarker)
            {
                pluginInput.Valid = false;
                string _ErrorMessagePrev = pluginInput.Prompt;
                pluginInput.ErrorMessage = Strings.Strings._IncorrectInput + ": " + pluginInput.Prompt;
                return false;
            }
            // powinno wczesniej wrocic true
            else { throw new NotImplementedException(); }
        }

        CustomStateEnum m_ShowDialogState = CustomStateEnum._pre;
        public override bool ShowDialog(bool applyEnabled)
        {
            // is not PreDialog culmination
            if (!applyEnabled)
            {
                return base.ShowDialog(applyEnabled);
            }

            m_ShowDialogState = CustomStateEnum._during;
            if (!this.IsInEditMode
                // in not eq. "Edit Loads" Dialog (probably redundant, because of start of this method)
                && applyEnabled
                )
            {
                // workaround (trial and error)
                fireTrussFinalLocationAdjustmentChange();
                this.m_resTrussStorage = this.m_resTrussStorage;
                this.m_AlignmentDestination = this.MyTruss.Alignment;
                // todo check integrated not needed?
                addLoadsForTrussCurrent(true);
            }
            else if (this.IsInEditMode
                && applyEnabled
                )
            {
                // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
            }

            bool res = base.ShowDialog(applyEnabled);

            if (__IS_Alignment_revert_to_center)
            {
                this.MyTruss.Alignment = Model3DNode.ModelPartAlignment.Center;
                //_ptNewOrExisting.AdjustOriginToAlignedPoint();
            }

            m_ShowDialogState = CustomStateEnum._post;
            return res;
        }

        #endregion

        #region PluginBaseTruss

        /// <summary>
        /// Set the <see cref="PluginDataNode"/> for an editable plugin. If <see cref="PluginDataNode"/> is specified all other created nodes should be inserted
        /// under the <see cref="PluginDataNode"/>. If <see cref="PluginDataNode"/> is null, a oneway plugin is assumed. In this case return the list of the created nodes.
        /// The <see cref="PluginDataNode"/> or return values are inserted under the target node.
        /// If do/undo delegates are specified, they are used and the return value is ignored. Do/undo delegates are meant for
        /// a plugin which modifies the Master node.
        /// </summary>
        /// <param name="doDelegate">The do delegate.</param>
        /// <param name="undoDelegate">The undo delegate.</param>
        /// <param name="update3DNodes">The nodes which need to be updated in the model views.</param>
        /// <returns>The created nodes which are inserted under the target node. Cannot contain a <see cref="Plugins.IPluginNode"/> (the <see cref="PluginDataNode"/>).</returns>
        public override List<BaseDataNode> Excecute(out Action doDelegate, out Action undoDelegate, out List<BaseDataNode> update3DNodes)
        {
            Action _doDelegateBase;
            Action _undoDelegateBase;
            List<BaseDataNode> _update3DNodesBase;
            List<BaseDataNode> resBase;
            if (this.IsInEditMode)
            {
                //resBase = base.Excecute(out _doDelegateBase, out _undoDelegateBase, out _update3DNodesBase);
                // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
                _doDelegateBase = delegate
                {
                    BaseDataNode _parentObjectForTruss_ = this._originalMyTruss.Parent;
                    if (_parentObjectForTruss_ != null
                        && _parentObjectForTruss_.HasChild(this._originalMyTruss)
                        )
                    {
                        _parentObjectForTruss_.RemoveChild(this._originalMyTruss);
                    }
                    if (_parentObjectForTruss_ != null
                        && !_parentObjectForTruss_.HasChild(this.MyTruss)
                        )
                    {
                        this._originalMyTruss.Parent.AddChild(this.MyTruss);
                    }

                    // doesnt refresh in view automatically, changes position only after selecting and object by mouse
                    List<Support> _supportList = this.updateBeamTrussSupportsGlobal();
                };
                _undoDelegateBase = delegate
                {
                    BaseDataNode _parentObjectForTruss_ = this.MyTruss.Parent;
                    if (_parentObjectForTruss_ != null
                        && _parentObjectForTruss_.HasChild(this.MyTruss)
                        )
                    {
                        _parentObjectForTruss_.RemoveChild(this.MyTruss);
                    }
                    if (_parentObjectForTruss_ != null
                        && !_parentObjectForTruss_.HasChild(this._originalMyTruss)
                        )
                    {
                        _parentObjectForTruss_.AddChild(this._originalMyTruss);
                    }

                    // doesnt refresh in view automatically, changes position only after selecting and object by mouse
                    // some problems when many times changins length in edit before enter (and the doint undo)
                    List<Support> _supportList = this.updateBeamTrussSupportsGlobal(isForUndo: true);
                };
                _update3DNodesBase = new List<BaseDataNode>();
                //_update3DNodesBase = (new BaseDataNode[] { this.MyTruss }).ToList();
                resBase = new List<BaseDataNode>();
                //resBase = (new BaseDataNode[] { this.MyTruss }).ToList();
            }
            else
            {
                // 20240626 adm workarout after 3Dt update 5.0.269 - reversed
                // for trusses changed in post dialog by RSTSam controls (trial and error)
                (this.ModelViewNodes as List<BaseDataNode>).Where(x => MyUtils.IsSameOrSubclass(x.GetType(), typeof(Epx.Ristek.Data.Models.ParametricTruss))).ToList().ForEach(x => (this.ModelViewNodes as List<BaseDataNode>).Remove(x));

                _doDelegateBase = delegate { };
                _undoDelegateBase = delegate { };
                _update3DNodesBase = new List<BaseDataNode>();
                resBase = new List<BaseDataNode>();
                /*
                // 20240626 adm workarout after 3Dt update 5.0.269 (not needed really?)
                // for trusses changed in post dialog by RSTSam controls (trial and error)
                (this.ModelViewNodes as List<BaseDataNode>).Where(x => MyUtils.IsSameOrSubclass(x.GetType(), typeof(Epx.Ristek.Data.Models.ParametricTruss))).ToList().ForEach(x => (this.ModelViewNodes as List<BaseDataNode>).Remove(x));

                _doDelegateBase = delegate { throw new NotImplementedException(); };
                _undoDelegateBase = delegate { throw new NotImplementedException(); };
                _update3DNodesBase = null;
                resBase = null;
                resBase = restoreMyTrussParamsAfterAction(() => { return base.Excecute(out _doDelegateBase, out _undoDelegateBase, out _update3DNodesBase); }, this.m_resTrussStorage) as List<BaseDataNode>;
                */
            }

            // Trusses are a special case and do not need to be set as a "target folder" in the UI. Take the plugin Master as the target.
            //PlanarStructure targetTruss = Master as PlanarStructure;

            Action _doDelegateExtension = delegate { };
            Action _undoDelegateExtension_ = delegate { };
            List<BaseDataNode> _update3DNodesExtension = new List<BaseDataNode>();
            List<BaseDataNode> resExtension = new List<BaseDataNode>();
            if (!this.IsInEditMode
                //&& targetTruss != null
                )
            {
                //
                // Create the model.
                // If using input storage method 1 the user's input values are in the _preInputs and _postInputs lists, otherwise
                // they are in the _selectedObject, _firstPoint and _secondPoint fields.
                //

                PluginDataNode = null;

                BaseDataNode _newTruss_asBaseDataNodeToAdd = getBeamTrussLocationAdjustedSingletonOrFetched();

                this.DataRawSerialize();

                if (_newTruss_asBaseDataNodeToAdd == null)
                    resExtension = new List<BaseDataNode>(0);

                // Use delegates to handle the data model edits manually in special cases.
                // Perform the data edit action
                _doDelegateExtension = delegate
                {
                    // can not add - app crashes when trying to "Edit Frame"
                    //List<bool> _addedToNewBeamList = _modelViewDimensionsPernamentSotarege.Where(x => !_newTruss_asBaseDataNodeToAdd.Children.Contains(x)).Select(y => _newTruss_asBaseDataNodeToAdd.AddChild(y)).ToList();
                    BaseDataNode _parentObjectForNewBeam_ = getParentObjectForNewBeam();
                    if (m_getParentObjectForNewBeam_Parent != null
                        && !m_getParentObjectForNewBeam_Parent.HasChild(_parentObjectForNewBeam_)
                        )
                    {
                        m_getParentObjectForNewBeam_Parent.AddChild(_parentObjectForNewBeam_);
                    }
                    BaseDataNode _parentObjectForNewSupports_ = getParentObjectForNewSupports();
                    if (_parentObjectForNewBeam_ != null
                        && !_parentObjectForNewBeam_.HasChild(_parentObjectForNewSupports_)
                        )
                    {
                        _parentObjectForNewBeam_.AddChild(_parentObjectForNewSupports_);
                    }
                    List<bool> _addedToParentFolderList = _modelViewNodesPernamentSotarege.Where(x => !_parentObjectForNewBeam_.Children.Contains(x)).Select(y => _parentObjectForNewBeam_.AddChild(y)).ToList();
                    List<Support> _supportList = m_resTrussMyHelper.m_trussToolPassed.updateBeamTrussSupportsGlobal();
                    List<bool> _addedToSupportFolderList = _supportList.Where(x => !_parentObjectForNewSupports_.Children.Contains(x)).Select(y => _parentObjectForNewSupports_.AddChild(y)).ToList();
                    // for future RTSam truss edition
                    m_resTrussMyHelper.m_trussToolPassed.resetMyDataInTrussStorageObj(_supportList, true);
                };
                // Reverse the "do" operation
                _undoDelegateExtension_ = delegate
                {
                    BaseDataNode _parentObjectForNewSupports_ = getParentObjectForNewSupports();
                    List<bool> _removedSupportsList = m_resTrussMyHelper.m_trussToolPassed.updateBeamTrussSupportsGlobal().Where(x => _parentObjectForNewSupports_.Children.Contains(x)).Select(y => _parentObjectForNewSupports_.RemoveChild(y)).ToList();
                    if (_parentObjectForNewSupports_.Children.Count() == 0)
                    {
                        _parentObjectForNewSupports_.Parent.RemoveChild(_parentObjectForNewSupports_);
                    }
                    BaseDataNode _parentObjectForNewBeam_ = getParentObjectForNewBeam();
                    List<bool> _removedTrussesList = _modelViewNodesPernamentSotarege.Union(_modelViewDimensionsPernamentSotarege).Where(x => _parentObjectForNewBeam_.Children.Contains(x)).Select(y => _parentObjectForNewBeam_.RemoveChild(y)).ToList();
                    if (_parentObjectForNewBeam_.Children.Count() == 0)
                    {
                        _parentObjectForNewBeam_.Parent.RemoveChild(_parentObjectForNewBeam_);
                    }
                };

                if (Master.GetDescendantNodes<ParametricTruss>().Contains(_newTruss_asBaseDataNodeToAdd))
                {
                    resExtension = (new BaseDataNode[] { _newTruss_asBaseDataNodeToAdd }).ToList();
                }
                else
                {
                    // adm doesnt seems to change anything
                    //return null;
                    resExtension = (new BaseDataNode[] { _newTruss_asBaseDataNodeToAdd }).ToList();
                }
            }
            else
            {
                //
                // Delete and change the current model.
                //
                resExtension = new List<BaseDataNode>(0);
            }

            doDelegate = _doDelegateBase + _doDelegateExtension;
            undoDelegate = _undoDelegateBase + _undoDelegateExtension_;
            update3DNodes = _update3DNodesBase;
            update3DNodes.AddRange(_update3DNodesExtension);

            //resExtension.AddRange(updateBeamTrussSupportsGlobal());

            // adm integrated
            this.CreateLoads();

            List<BaseDataNode> res = new List<BaseDataNode>();
            res.AddRange(resBase);
            res.AddRange(resExtension);
            return res;
        }

        #endregion

        #region ParametricBaseTruss

        Model3DNode.ModelPartAlignment m_AlignmentDestination;
        Point3D m_initializationTrussOrigin;
        protected override void InitializeMyTrussProperties(ParametricTrussRTSamInsertMember initializationTruss)
        {
            Point3D _initializationTrussOriginOriginal_ = initializationTruss.Origin;
            if (this.IsInEditMode)
            // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
            {
                this.m_AlignmentDestination = initializationTruss.Alignment;
                this.m_initializationTrussOrigin = initializationTruss.AlignedStartPoint;
                initializationTruss.Origin = initializationTruss.AlignedStartPoint;
            }
            base.InitializeMyTrussProperties(initializationTruss);
        }

        #endregion

        #endregion

        #region IModelViewPlugin

        /// <summary>
        /// Gets or sets the plugin engine (model view) that is executing this plugin.
        /// </summary>
        /// <value>The plugin engine.</value>
        public new IPluginEngine PluginEngine
        {
            get
            {
                return base.PluginEngine;
            }
            set
            {
                base.PluginEngine = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the plugins dialog was closed as OK or Cancel.
        /// </summary>
        /// <value><c>true</c> if [dialog result]; otherwise, <c>false</c>.</value>
        public new bool DialogResult
        {
            get
            {
                return base.DialogResult;
            }
            set
            {
                base.DialogResult = value;
            }
        }
        /// <summary>
        /// The dialog has been closed by the engine.
        /// </summary>
        /// <param name="isCancel">if set to <c>true</c> [is cancel].</param>
        public new void DialogClosed(bool isCancel)
        {
            /*
            _dialog?.Close();
            _dialog = null;
            */
            base.DialogClosed(isCancel);
        }

        private List<BaseDataNode> _modelViewNodesPernamentSotarege = new List<BaseDataNode>();
        //private List<BaseDataNode> _modelViewNodes = new List<BaseDataNode>();
        private void modelViewNodesAddElem(BaseDataNode _newElem)
        {
            //_modelViewNodes.Add(_newElem);
            (this.ModelViewNodes as List<BaseDataNode>).Add(_newElem);
            _modelViewNodesPernamentSotarege.Add(_newElem);
        }
        /// <summary>
        /// Gets the nodes which are displayed in the model view.
        /// </summary>
        /// <value>The model view nodes.</value>
        //public IEnumerable<BaseDataNode> ModelViewNodes => _modelViewNodes;

        private List<ModelDimensionBase> _modelViewDimensionsPernamentSotarege = new List<ModelDimensionBase>();
        //private List<ModelDimensionBase> _modelViewDimensions = new List<ModelDimensionBase>();
        private void modelViewDimensionsAddElem(ModelDimensionBase _newElem)
        {
            //_modelViewDimensions.Add(_newElem);
            (ModelViewDimensions as List<ModelDimensionBase>).Add(_newElem);
            _modelViewDimensionsPernamentSotarege.Add(_newElem);
        }
        private void modelViewDimensionsClear()
        {
            //_modelViewDimensions.Clear();
            (ModelViewDimensions as List<ModelDimensionBase>).Clear();
            _modelViewDimensionsPernamentSotarege.Clear();
        }
        /// <summary>
        /// Gets the dimension lines to display in the model view.
        /// </summary>
        /// <value>The model view dimensions.</value>
        //public IEnumerable<ModelDimensionBase> ModelViewDimensions => _modelViewDimensions;

        /// <summary>
        /// The plugin has been cancelled by the engine.
        /// </summary>
        public new void CancelPlugin()
        {
            base.CancelPlugin();
            // bo cancel przed wygenerowaniem truss na etapie preDialog
            if (this.IsInEditMode)
            {
                this.updateBeamTrussSupportsGlobal(isForUndo: true);
            }
        }

        /// <summary>
        /// Resets the model view nodes. This tool can be reused so all references to old temp nodes must be cleared.
        /// </summary>
        private new void ResetModelViewNodes()
        {
            base.ResetModelViewNodes();
            _modelViewDimensions.Clear();
            _modelViewNodesPernamentSotarege.Clear();
        }

        /// <summary>
        /// The mouse was moved in the model.
        /// </summary>
        /// <param name="currentPoint">The current point.</param>
        public new void MouseMoved(Point3D currentPoint)
        {
            base.MouseMoved(currentPoint);
        }

        /// <summary>
        /// Gets the temporarily hidden nodes.
        /// </summary>
        /// <value>The hidden nodes.</value>
        //public IEnumerable<BaseDataNode> HiddenNodes => new List<BaseDataNode>();

        /// <summary>
        /// Gets the overlay content to display in the model view.
        /// </summary>
        public new IEnumerable<ModelOverlayContentElement> ModelViewOverlayContents
        {
            get
            {
                if (m_ShowDialogState == CustomStateEnum._pre)
                {
                    List<ModelOverlayContentElement> res = new List<ModelOverlayContentElement>();
                    return res;
                }
                else
                {
                    return base.ModelViewOverlayContents;
                }
            }
        }

        #endregion //IModelViewPlugin

        #region IRibbonViewPlugin

        public new RibbonGroup PreDialogRibbonTabGroup
        {
            get
            {
                return m_InputPhaseRibbonControl;
            }

        }

        public new object PreDialogRibbonViewModel
        {
            get
            {
                //return base.PreDialogRibbonViewModel;
                return this.DataFieldsRawObject_nonStatic;
            }

        }

        #endregion

        #region IRibbonDialogPlugin

        public new List<RibbonGroup> RibbonDialogTabGroups
        {
            get
            {
                //return base.RibbonDialogTabGroups;
                List<RibbonGroup> newGroup = new List<RibbonGroup>();
                newGroup.AddRange(base.RibbonDialogTabGroups);
                //  not working properly [when !__IS_RistekPlugin_endPreDialogWithEnter]?
                if (!__IS_RistekPlugin_endPreDialogWithEnter
                    //&& !this.IsInEditMode
                    )
                {
                    m_InputPhaseRibbonControl = new InputPhaseRibbonControl(null);
                    m_InputPhaseRibbonControl.DataContext = this.DataFieldsRawObject_nonStatic;
                    newGroup[3] = m_InputPhaseRibbonControl;
                    //newGroup.Add(m_InputPhaseRibbonControl);
                }
                return newGroup;
            }
        }

        #endregion

        #region ITemporaryCurrentTruss

        public new PlanarStructure GetCurrentTruss()
        {
            return base.GetCurrentTruss();
        }

        #endregion

        #region methods helpers creation

        // 20240626 adm workarout after 3Dt update 5.0.269 - reversed
        public const bool __IS_Alignment_revert_to_center = false;
        //public const bool __IS_Alignment_revert_to_center = true;

        Model3DNode.ModelPartAlignment getInitialModelPartAlignment()
        {
            Model3DNode.ModelPartAlignment _modelPartAlignment;
            PlaneAlignementEnum _planeAlignement = this.DataFieldsRawObject_nonStatic.PlaneAlignement;
            if (_planeAlignement == PlaneAlignementEnum.toAxis)
            {
                _modelPartAlignment = Model3DNode.ModelPartAlignment.Center;
            }
            else if (_planeAlignement == PlaneAlignementEnum.toTop)
            {
                _modelPartAlignment = Model3DNode.ModelPartAlignment.CenterLeft;
            }
            else if (_planeAlignement == PlaneAlignementEnum.toBottom)
            {
                _modelPartAlignment = Model3DNode.ModelPartAlignment.CenterRight;
            }
            else { throw new NotImplementedException(); }
            return _modelPartAlignment;
        }

        Model3DNode.ModelPartAlignment revertModelPartAlignment(Model3DNode.ModelPartAlignment _in)
        {
            Model3DNode.ModelPartAlignment _modelPartAlignment;
            if (_in == Model3DNode.ModelPartAlignment.Center)
            {
                _modelPartAlignment = Model3DNode.ModelPartAlignment.Center;
            }
            else if (_in == Model3DNode.ModelPartAlignment.CenterRight)
            {
                _modelPartAlignment = Model3DNode.ModelPartAlignment.CenterLeft;
            }
            else if (_in == Model3DNode.ModelPartAlignment.CenterLeft)
            {
                _modelPartAlignment = Model3DNode.ModelPartAlignment.CenterRight;
            }
            else { throw new NotImplementedException(); }
            return _modelPartAlignment;
        }

        // form now only for PluginInput around Member
        protected Member getTrussMemberFromPluginInput_(PluginInput _pluginInput)
        {
            Member res = null;
            if (getParametricTrussFromPluginInput(_pluginInput) != null)
            {
                res = (_pluginInput as PluginObjectInput).Object as Member;
            }
            return res;
        }

        // form now only for PluginInput around Member
        protected ParametricTruss getParametricTrussFromPluginInput(PluginInput _pluginInput)
        {
            ParametricTruss res = null;
            if (_pluginInput is PluginObjectInput)
            {
                /*
                if ((_pluginInput as PluginObjectInput).Object is Member
                    && (_pluginInput as PluginObjectInput).Object.HasParent<MembersGroupNode>()
                    )
                {
                    MembersGroupNode _membersGroupNode = (_pluginInput as PluginObjectInput).Object.Parent as MembersGroupNode;
                    if (_membersGroupNode.HasParent<ParametricTruss>())
                    {
                        res = _membersGroupNode.Parent as ParametricTruss;
                    }
                }
                */
                BaseDataNode _objCurr = (_pluginInput as PluginObjectInput).Object;
                while (_objCurr != null
                    && !(_objCurr is ParametricTruss)
                    && _objCurr.Parent != null
                    )
                {
                    _objCurr = _objCurr.Parent;
                }

                if (_objCurr is ParametricTruss)
                {
                    res = _objCurr as ParametricTruss;
                }
            }
            return res;
        }

        protected ParametricTruss getParametricTrussFromPluginInput(Member member)
        {
            ParametricTruss res = null;
            {
                BaseDataNode _objCurr = member;
                while (_objCurr != null
                    && !(_objCurr is ParametricTruss)
                    && _objCurr.Parent != null
                    )
                {
                    _objCurr = _objCurr.Parent;
                }

                if (_objCurr is ParametricTruss)
                {
                    res = _objCurr as ParametricTruss;
                }
            }
            return res;
        }

        // 20240702
        protected double getOffsetFromSideBorderThicknessAxisPlane(ParametricTruss pt_i)
        {
            //double res = m_i.Thickness / 2.0;
            double res = (pt_i.Thickness * pt_i.BundleCount + pt_i.BundleGap * (pt_i.BundleCount - 1)) / 2.0;
            return res;
        }

        // (_intersectionPointSupport, _intersectionPointMember)
        protected (Point3D, Point3D)? GetMemberAboveSupportAxesVirtualIntersectionPointsPair(Support support, Member member)
        {
            Vector3D _referenceVerticalNormalInGlobalRaw = new Vector3D(0, 0, 1);
            Vector3D _referenceVerticalNormalInLocal = support.Geometry3D.GlobalToLocal.Transform(_referenceVerticalNormalInGlobalRaw);

            List<Surface3D> _supportTopSurfacesList = support.Geometry3D.Surfaces.Where(
                x => MyUtils.CompareWithTollerance(x.Normal.X, _referenceVerticalNormalInLocal.X) == 0
                && MyUtils.CompareWithTollerance(x.Normal.Y, _referenceVerticalNormalInLocal.Y) == 0
                && MyUtils.CompareWithTollerance(x.Normal.Z, _referenceVerticalNormalInLocal.Z) == 0
                ).ToList();
            Surface3D _supportTopSurface = _supportTopSurfacesList.FirstOrDefault();
            if (_supportTopSurface == null)
            {
                return null;
            }
            ParametricTruss _pt0 = getParametricTrussFromPluginInput(member);

            if (_pt0 == null)
            {
                return null;
            }

            Point3D _member_startPoint3D = _pt0.LocalToGlobal.Transform(GeometryMath.ToPoint3D(member.StartPoint));
            Point3D _member_endPoint3D = _pt0.LocalToGlobal.Transform(GeometryMath.ToPoint3D(member.EndPoint));

            Point line_start1 = GeometryMath.ToPoint(support.StartPoint);
            Point line_end1 = GeometryMath.ToPoint(support.EndPoint);
            Point segment_start2 = GeometryMath.ToPoint(_member_startPoint3D);
            Point segment_end2 = GeometryMath.ToPoint(_member_endPoint3D);
            Point? _intersectionPoint = MyUtils.FindIntersection(line_start1, line_end1, segment_start2, segment_end2);

            if (MyUtils.CompareWithTollerance(MyUtils.PointsDistanceAbsolute(segment_start2, segment_end2), 0.0) == 0)
            {
                return null;
            }
            else if (_intersectionPoint.HasValue)
            {
                double in_x = _intersectionPoint.Value.X;
                double in_y = _intersectionPoint.Value.Y;

                Point3D _intersectionPointMember;
                {
                    Point3D line_startPoint = _member_startPoint3D;
                    Point3D line_endPoint = _member_endPoint3D;
                    double _member_z_ = MyUtils.CalculateZ_forPointOnLine(line_startPoint, line_endPoint, in_x, in_y);
                    _intersectionPointMember = new Point3D(in_x, in_y, _member_z_);
                }

                Point3D _intersectionPointSupportTopSurface;
                /*
                {
                    Point3D line_startPoint = support.StartPoint;
                    Point3D line_endPoint = support.EndPoint;
                    double _support_z = MyUtils.CalculateZ_forPointOnLine(line_startPoint, line_endPoint, in_x, in_y);
                    Point3D _intersectionPointSupportAxis = new Point3D(in_x, in_y, _support_z);
                }
                */
                {
                    Point3D point = _intersectionPointMember;
                    Point3D origin = support.Geometry3D.LocalToGlobal.Transform(_supportTopSurface.EdgeLines[0].StartPoint);
                    Vector3D normal = _referenceVerticalNormalInGlobalRaw;
                    // ADM GeometryMath.ProjectPointToPlane3D seems to be not working properly
                    //Vector3D xAxis = new Vector3D(0, 0, 0);
                    //_intersectionPointSupportTopSurface = GeometryMath.ProjectPointToPlane3D(point, origin, normal, xAxis);
                    _intersectionPointSupportTopSurface = MyUtils.CastPointOnPlane(point, origin, normal);
                }

                return (_intersectionPointSupportTopSurface, _intersectionPointMember);
            }
            else
            {
                return null;
            }
        }

        protected bool IsMemberAboveSupport(Support support, Member member)
        {
            (Point3D, Point3D)? _intersectionPointsPair = GetMemberAboveSupportAxesVirtualIntersectionPointsPair(support, member);
            if (_intersectionPointsPair == null)
            {
                return false;
            }
            else
            {
                double _support_z = _intersectionPointsPair.Value.Item1.Z;
                double _member_z_ = _intersectionPointsPair.Value.Item2.Z;
                bool _isMemberAboveSupport = MyUtils.CompareWithTollerance(_member_z_, _support_z) > 0;
                return _isMemberAboveSupport;
            }
        }

        public ParametricTrussRTSamInsertMember GenerateBeamTrussNewBase(ParametricTrussInsertMemberObjectMyHelper trussMyHelper, out (Vector3D, Vector3D, Vector3D) newTrussPlainTupleNormalizeNot)
        {
            double _memberAlignmentTolerance = 4 * 1e-6;

            // 20240328 Knaga
            Support _knagaMode_support = null;
            Member _knagaMode_mA_forSupportLimes = null;
            (Point3D, Point3D)? _knagaMode_mA_intersectionPointsPair = null;
            Member _knagaMode_mB_forSupportLimes_ = null;
            (Point3D, Point3D)? _knagaMode_mB_intersectionPointsPair_ = null;
            if (this.Flag_isRistekPlugin_isKnagaMode)
            {
                _knagaMode_support = getKnagaMainSupport();
                //_knagaMode_mA_forSupportLimes = (_preInputs[this.PreDialogInputFirstMemeberOffset + 0] as PluginObjectInput).Object as Member;
                _knagaMode_mA_forSupportLimes = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                _knagaMode_mA_intersectionPointsPair = GetMemberAboveSupportAxesVirtualIntersectionPointsPair(_knagaMode_support, _knagaMode_mA_forSupportLimes);
                //_knagaMode_mB_forSupportLimes_ = (_preInputs[this.PreDialogInputFirstMemeberOffset + 1] as PluginObjectInput).Object as Member;
                _knagaMode_mB_forSupportLimes_ = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 1]);
                _knagaMode_mB_intersectionPointsPair_ = GetMemberAboveSupportAxesVirtualIntersectionPointsPair(_knagaMode_support, _knagaMode_mB_forSupportLimes_);
            }

            // todo integrated check
            /*
            ParametricTruss _pt0 = getParametricTrussFromPluginInput(_preInputs[0]);
            // bo cancel przed wygenerowaniem pluginu na etapie ribbon
            if (_pt0 == null)
            {
                //trussMyHelper = null;
                newTrussPlainTupleNormalizeNot = (new Vector3D(), new Vector3D(), new Vector3D()); return null;
            }

            Member _m0_ = getTrussMemberFromPluginInput_(_preInputs[0]);
            (List<Member>, List<Point>) _pt0_pointsOnAxesList_for_m0 = (new List<Member>(), new List<Point>());
            foreach (Member _memberCurr in _pt0.GetMembers())
            {
                if (MyUtils.ArePararrelMembers(_m0_, _memberCurr, _memberAlignmentTolerance)
                    && MyUtils.AreMembersColinear(_m0_, _memberCurr, _memberAlignmentTolerance)
                    )
                {
                    _pt0_pointsOnAxesList_for_m0.Item1.Add(_memberCurr);
                    _pt0_pointsOnAxesList_for_m0.Item2.Add(_memberCurr.StartPoint);
                    _pt0_pointsOnAxesList_for_m0.Item2.Add(_memberCurr.EndPoint);
                }
            }
            _pt0_pointsOnAxesList_for_m0.Item2 = _pt0_pointsOnAxesList_for_m0.Item2.Distinct().ToList();
            bool _orientPairFromLowerToHigherLocalY = true;
            double _maxDistance_p0;
            (Point, Point) _pt0_pointsOnAxesTupleFarthest_p0 = MyUtils.FindFarthestPoints(_pt0_pointsOnAxesList_for_m0.Item2, out _maxDistance_p0, _orientPairFromLowerToHigherLocalY);

            ParametricTruss _pt1 = getParametricTrussFromPluginInput(_preInputs[1]);
            // bo cancel przed wygenerowaniem pluginu na etapie ribbon
            if (_pt1 == null)
            {
                //trussMyHelper = null;
                newTrussPlainTupleNormalizeNot = (new Vector3D(), new Vector3D(), new Vector3D()); return null;
            }

            Member _m1_ = getTrussMemberFromPluginInput_(_preInputs[1]);
            (List<Member>, List<Point>) _pt1_pointsOnAxesList_for_m1 = (new List<Member>(), new List<Point>());
            foreach (Member _memberCurr in _pt1.GetMembers())
            {
                if (MyUtils.ArePararrelMembers(_m1_, _memberCurr, _memberAlignmentTolerance)
                    && MyUtils.AreMembersColinear(_m1_, _memberCurr, _memberAlignmentTolerance)
                    )
                {
                    _pt1_pointsOnAxesList_for_m1.Item1.Add(_memberCurr);
                    _pt1_pointsOnAxesList_for_m1.Item2.Add(_memberCurr.StartPoint);
                    _pt1_pointsOnAxesList_for_m1.Item2.Add(_memberCurr.EndPoint);
                }
            }
            _pt1_pointsOnAxesList_for_m1.Item2 = _pt1_pointsOnAxesList_for_m1.Item2.Distinct().ToList();
            double _maxDistance_p1;
            (Point, Point) _pt1_pointsOnAxesTupleFarthest_p1 = MyUtils.FindFarthestPoints(_pt1_pointsOnAxesList_for_m1.Item2, out _maxDistance_p1, _orientPairFromLowerToHigherLocalY);
            */
            bool _orientPairFromLowerToHigherLocalY = true;
            //List<(ParametricTruss, Member, (Point, Point))> reachPointExtracionList = new List<(ParametricTrussRTSam, Member, (Point, Point))>();
            //List<(ParametricTruss, double, (Point, Point))> reachPointExtracionList_inLocal = new List<(ParametricTrussRTSam, double, (Point, Point))>();
            List<double> _ptNewLengthAdditionalKnagaAdjustmentList = new List<double>(2);
            List<(double, (Point3D, Point3D))> reachPointExtracionList_inGlobal_ = new List<(double, (Point3D, Point3D))>();
            foreach (int i in new int[] { 0, 1 })
            {
                _ptNewLengthAdditionalKnagaAdjustmentList.Add(0);

                double _m_i_thicknessAdjustment;
                (Point3D, Point3D) _ptCurr_pointsOnAxesTupleFarthest_inGlobal_p_;
                if (!this.Flag_isRistekPlugin_isKnagaMode)
                {

                    ParametricTruss _ptCurr = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + i]);
                    // bo cancel przed wygenerowaniem pluginu na etapie ribbon
                    if (_ptCurr == null)
                    {
                        //trussMyHelper = null;
                        newTrussPlainTupleNormalizeNot = (new Vector3D(), new Vector3D(), new Vector3D()); return null;
                    }

                    Member _m_i = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + i]);
                    // 20240702
                    //_m_i_thicknessAdjustment = _m_i.Thickness / 2.0;
                    _m_i_thicknessAdjustment = getOffsetFromSideBorderThicknessAxisPlane(_ptCurr);
                    (List<Member>, List<Point>) _pt_pointsOnAxesList_for_mCurr = (new List<Member>(), new List<Point>());
                    /*
                    _pt_pointsOnAxesList_for_mCurr.Item1.Add(_m_i);
                    _pt_pointsOnAxesList_for_mCurr.Item2.Add(_m_i.StartPoint);
                    _pt_pointsOnAxesList_for_mCurr.Item2.Add(_m_i.EndPoint);
                    */
                    List<Member> choosenTrussMembers = _ptCurr.GetMembers();
                    foreach (Member _memberCurr in choosenTrussMembers)
                    {
                        if (true
                            //&& _memberCurr != _m_i
                            && MyUtils.ArePararrelMembers(_m_i, _memberCurr, _memberAlignmentTolerance)
                            && MyUtils.AreMembersColinear(_m_i, _memberCurr, _memberAlignmentTolerance)
                            )
                        {
                            foreach (Point _pointCurr in new Point[] { _memberCurr.StartPoint, _memberCurr.EndPoint })
                            {
                                _pt_pointsOnAxesList_for_mCurr.Item1.Add(_memberCurr);
                                Point _pointCurrCasted = MyUtils.CastPointOnLine(_m_i.StartPoint, _m_i.EndPoint, _pointCurr);
                                Point _pointCurrUse = MyUtils.CompareWithTollerance(MyUtils.PointsDistanceAbsolute(_pointCurrCasted, _pointCurr), 0.0, _memberAlignmentTolerance) == 0
                                    ? _pointCurr
                                    : _pointCurrCasted
                                    ;
                                _pt_pointsOnAxesList_for_mCurr.Item2.Add(_pointCurrUse);
                            }
                        }
                    }
                    _pt_pointsOnAxesList_for_mCurr.Item2 = _pt_pointsOnAxesList_for_mCurr.Item2.Distinct().ToList();
                    double _maxDistance_pCurr;
                    (Point, Point) _ptCurr_pointsOnAxesTupleFarthest_inLocal_p = MyUtils.FindFarthestPoints(_pt_pointsOnAxesList_for_mCurr.Item2, out _maxDistance_pCurr, _orientPairFromLowerToHigherLocalY);
                    //reachPointExtracionList.Add((_ptCurr, _m_i, _ptCurr_pointsOnAxesTupleFarthest_p));
                    //reachPointExtracionList_inLocal.Add((_ptCurr, _m_i_thicknessAdjustment, _ptCurr_pointsOnAxesTupleFarthest_inLocal_p));
                    _ptCurr_pointsOnAxesTupleFarthest_inGlobal_p_ = (
                        _ptCurr.LocalToGlobal.Transform(GeometryMath.ToPoint3D(_ptCurr_pointsOnAxesTupleFarthest_inLocal_p.Item1)),
                        _ptCurr.LocalToGlobal.Transform(GeometryMath.ToPoint3D(_ptCurr_pointsOnAxesTupleFarthest_inLocal_p.Item2))
                        );
                }
                else //if (this.Flag_isRistekPlugin_isKnagaMode)
                {
                    // 20240702
                    //Member _m_i = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + i]);
                    //double _m_i_thicknessAdjustment_realForKnagaSide = _m_i.Thickness / 2.0;
                    ParametricTruss _pt_i = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + i]);
                    double _m_i_thicknessAdjustment_realForKnagaSide = getOffsetFromSideBorderThicknessAxisPlane(_pt_i);
                    _ptNewLengthAdditionalKnagaAdjustmentList[i] = _m_i_thicknessAdjustment_realForKnagaSide;

                    if (i == 0)
                    {
                        _m_i_thicknessAdjustment = 0;
                        _ptCurr_pointsOnAxesTupleFarthest_inGlobal_p_ = (
                            _knagaMode_mA_intersectionPointsPair.Value.Item1,
                            _knagaMode_mB_intersectionPointsPair_.Value.Item1
                            );
                    }
                    else if (i == 1)
                    {
                        _m_i_thicknessAdjustment = 0;
                        _ptCurr_pointsOnAxesTupleFarthest_inGlobal_p_ = (
                            _knagaMode_mA_intersectionPointsPair.Value.Item2,
                            _knagaMode_mB_intersectionPointsPair_.Value.Item2
                            );
                    }
                    else { throw new NotImplementedException(); }
                }
                reachPointExtracionList_inGlobal_.Add((_m_i_thicknessAdjustment, _ptCurr_pointsOnAxesTupleFarthest_inGlobal_p_));
            }
            //ParametricTruss _pt0 = reachPointExtracionList_inLocal[0].Item1;
            //Member _m0_ = reachPointExtracionList[0].Item2;
            //double _m0_thicknessAdjustment = reachPointExtracionList_inLocal[0].Item2;
            double _m0_thicknessAdjustment = reachPointExtracionList_inGlobal_[0].Item1;
            //(Point, Point) _pt0_pointsOnAxesTupleFarthest_inLocal_p0 = reachPointExtracionList_inLocal[0].Item3;
            //ParametricTruss _pt1 = reachPointExtracionList_inLocal[1].Item1;
            //Member _m1_ = reachPointExtracionList[1].Item1;
            //double _m1_thicknessAdjustment_ = reachPointExtracionList_inLocal[1].Item2;
            double _m1_thicknessAdjustment_ = reachPointExtracionList_inGlobal_[1].Item1;
            //(Point, Point) _pt1_pointsOnAxesTupleFarthest_inLocal_p1 = reachPointExtracionList_inLocal[1].Item3;

            /*
            string _messageString;
            if (CurrentUICulture.ThreeLetterISOLanguageName == "pol")
            {
                _messageString = String.Format("Wskazano równolegle belki należące do równoległych wiązarów. Maksymalny zasięg współliniowych prętów obejmuje:\ndla wiązara pierwszego \"{0}\": {1} z punktami skrajnymi {2} oraz {3} odległymi o {4};\ndla wiązara drugiego \"{5}\": {6} z punktami skrajnymi {7} oraz {8} odległymi o {9}.",
                    _pt0.Name, String.Join(", ", _pt0_pointsOnAxesList_for_m0.Item1.Select(x => "\"" + x.Name + "\"").ToList()), MyUtils.FormatPoint3DWithPrecision(MyUtils.TransformLocalToGlobal(_pt0, _pt0_pointsOnAxesTupleFarthest_p0.Item1)), MyUtils.FormatPoint3DWithPrecision(MyUtils.TransformLocalToGlobal(_pt0, _pt0_pointsOnAxesTupleFarthest_p0.Item2)), String.Format("{0:0.00}", _maxDistance_p0),
                    _pt1.Name, String.Join(", ", _pt1_pointsOnAxesList_for_m1.Item1.Select(x => "\"" + x.Name + "\"").ToList()), MyUtils.FormatPoint3DWithPrecision(MyUtils.TransformLocalToGlobal(_pt1, _pt1_pointsOnAxesTupleFarthest_p1.Item1)), MyUtils.FormatPoint3DWithPrecision(MyUtils.TransformLocalToGlobal(_pt1, _pt1_pointsOnAxesTupleFarthest_p1.Item2)), String.Format("{0:0.00}", _maxDistance_p1)
                    );
            }
            else
            {
                _messageString = String.Format("Parallel members in parallel trusses are selected.");
            }
            MessageBox.Show(_messageString);
            */

            //Point3D _newPlainPoint1 = _pt0.LocalToGlobal.Transform(GeometryMath.ToPoint3D(_pt0_pointsOnAxesTupleFarthest_inLocal_p0.Item1));
            //Point3D _newPlainPoint2 = _pt0.LocalToGlobal.Transform(GeometryMath.ToPoint3D(_pt0_pointsOnAxesTupleFarthest_inLocal_p0.Item2));
            //Point3D _newPlainPoint3 = _pt1.LocalToGlobal.Transform(GeometryMath.ToPoint3D(_pt1_pointsOnAxesTupleFarthest_inLocal_p1.Item2));
            Point3D newPlainPointStartFirstTruss = reachPointExtracionList_inGlobal_[0].Item2.Item1;
            Point3D newPlainPointEndFirstTruss = reachPointExtracionList_inGlobal_[0].Item2.Item2;
            Point3D newPlainPointEndSecondTruss = reachPointExtracionList_inGlobal_[1].Item2.Item2;

            Vector3D pt0_p1_to_p2_vectorNormalized = (newPlainPointEndFirstTruss - newPlainPointStartFirstTruss);
            pt0_p1_to_p2_vectorNormalized.Normalize();
            Vector3D pt0_p2_to_p1_vectorNormalized_ = (newPlainPointStartFirstTruss - newPlainPointEndFirstTruss);
            pt0_p2_to_p1_vectorNormalized_.Normalize();
            Point3D newPlainPointStartFirstTruss_Used = newPlainPointStartFirstTruss;
            // origin point offset seted in fireTrussFinalLocationAdjustmentChange
            //Point3D _newPlainPoint1_Used = MyUtils.TranslatePoint(_newPlainPoint1, pt0_p1_to_p2_vectorNormalized, DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseBottom_mm);
            //Point3D _newPlainPoint2_Used_ = _newPlainPoint2;
            // 20240328 Knaga
            Point3D newPlainPointEndFirstTruss_Used = MyUtils.TranslatePoint(newPlainPointEndFirstTruss, pt0_p2_to_p1_vectorNormalized_,
                DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOffsetLengwiseBottom_mm() + _ptNewLengthAdditionalKnagaAdjustmentList[0]
                + DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOffsetLengwiseTop_mm() + _ptNewLengthAdditionalKnagaAdjustmentList[1]
                );

            (Vector3D, Vector3D, Vector3D) _plainTup = MyUtils.DeterminePlaneAxes(newPlainPointStartFirstTruss_Used, newPlainPointEndFirstTruss_Used, newPlainPointEndSecondTruss);
            newTrussPlainTupleNormalizeNot = _plainTup;

            Vector3D planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(newPlainPointStartFirstTruss, newPlainPointEndFirstTruss, newPlainPointEndSecondTruss);
            planeNormalToFutureBeamTruss.Normalize();

            //double _elevation_t = _m0_.Width / 2.0;

            double _elevation_t = 0;
            Point3D newPlainOriginPointElevatedUp = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(newTrussPlainTupleNormalizeNot.Item1, newTrussPlainTupleNormalizeNot.Item2, newTrussPlainTupleNormalizeNot.Item3, newPlainPointStartFirstTruss_Used, _elevation_t, getReferenceEnforcementDirectionVector(), MyUtils.EnforceElevationVectorEnum.enforcePositive);
            Point3D newPlainDestinationPointElevatedUp_ = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(newTrussPlainTupleNormalizeNot.Item1, newTrussPlainTupleNormalizeNot.Item2, newTrussPlainTupleNormalizeNot.Item3, newPlainPointEndFirstTruss_Used, _elevation_t, getReferenceEnforcementDirectionVector(), MyUtils.EnforceElevationVectorEnum.enforcePositive);

            Vector3D _futureBeamTrussVectorPlane = MyUtils.CalculateNormal(planeNormalToFutureBeamTruss, newPlainPointStartFirstTruss, newPlainPointEndFirstTruss);
            _futureBeamTrussVectorPlane.Normalize();
            //Point3D _newPlainOriginPointElevatedUp_andMovedToPartner = MyUtils.TranslatePoint(_newPlainOriginPointElevatedUp, _futureBeamTrussVectorPlane, _m0_.Thickness / 2.0);
            //Point3D _newPlainDestinationPointElevatedUp_andMovedToPartner_ = MyUtils.TranslatePoint(_newPlainDestinationPointElevatedUp_, _futureBeamTrussVectorPlane, _m0_.Thickness / 2.0);
            Point3D _newPlainOriginPointElevatedUp_andMovedToPartner = MyUtils.TranslatePoint(newPlainOriginPointElevatedUp, _futureBeamTrussVectorPlane, _m0_thicknessAdjustment);
            Point3D _newPlainDestinationPointElevatedUp_andMovedToPartner_ = MyUtils.TranslatePoint(newPlainDestinationPointElevatedUp_, _futureBeamTrussVectorPlane, _m0_thicknessAdjustment);

            double _distanceBeetwenTrusses;

            if (!this.Flag_isRistekPlugin_isKnagaMode)
            {
                // not working for every case
                /*
                Vector3D xaxis1 = _pt0.XAxis;
                Vector3D yaxis1 = _pt0.YAxis;
                Vector3D zaxis1 = _pt0.ZAxis;
                Point3D origin1 = _newPlainPoint1;
                Vector3D xaxis2 = _pt1.XAxis;
                Vector3D yaxis2 = _pt1.YAxis;
                Vector3D zaxis2 = _pt1.ZAxis;
                // to_do check
                //Point3D origin2 = MyUtils.TransformLocalToGlobal(_pt1, _pt1_pointsOnAxesTupleFarthest_p1.Item1);
                Point3D origin2 = _pt1.LocalToGlobal.Transform(new Point3D(_pt1_pointsOnAxesTupleFarthest_p1.Item1.X, _pt1_pointsOnAxesTupleFarthest_p1.Item1.Y, 0));
                _distanceBeetwenTrusses = MyUtils.DistanceBetweenParallelPlanes(
                    xaxis1, yaxis1, zaxis1, origin1,
                    xaxis2, yaxis2, zaxis2, origin2
                    );
                */

                //_distanceBeetwenTrusses = MyUtils.PointDistanceToTrussPlane(_pt1, _newPlainPoint1);
                ParametricTruss _pt1 = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + 1]);
                _distanceBeetwenTrusses = MyUtils.PointDistanceToTrussPlane(_pt1, newPlainPointStartFirstTruss);
            }
            else //if (this.Flag_isRistekPlugin_isKnagaMode)
            {
                _distanceBeetwenTrusses = MyUtils.PointsDistanceAbsolute(_knagaMode_mA_intersectionPointsPair.Value.Item1, _knagaMode_mA_intersectionPointsPair.Value.Item2);
            }


            ParametricTrussRTSamInsertMember _ptNew;
            {
                //ParametricBaseTruss trussTool = null;
                //double height = _distanceBeetwenTrusses - _m0_.Thickness / 2.0 - _m1_.Thickness / 2.0;

                var test1 = DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOutreachPerpPrimary_mm();
                var test2 = DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOutreachPerpSeccondary_mm();

                double height = _distanceBeetwenTrusses
                    - _m0_thicknessAdjustment + DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOutreachPerpPrimary_mm()
                    - _m1_thicknessAdjustment_ + DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOutreachPerpSeccondary_mm()
                    ;
                Point3D origin = _newPlainOriginPointElevatedUp_andMovedToPartner;
                Point3D directionPoint = _newPlainDestinationPointElevatedUp_andMovedToPartner_;

                //trussMyHelper = new ParametricTrussMyHelper(this);
                _ptNew = trussMyHelper.GenerateParametricTruss(this, height, origin, directionPoint);

                _ptNew.SetXAxis(directionPoint - origin, planeNormalToFutureBeamTruss);

                _ptNew.Alignment = Epx.BIM.Models.Model3DNode.ModelPartAlignment.Center; // BottomLeft, BottomCenter, BottomRight
                _ptNew.AdjustOriginToAlignedPoint();

                _ptNew.Origin = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(newTrussPlainTupleNormalizeNot.Item1, newTrussPlainTupleNormalizeNot.Item2, newTrussPlainTupleNormalizeNot.Item3, _ptNew.Origin, _ptNew.Thickness / 2.0, getReferenceEnforcementDirectionVector(), MyUtils.EnforceElevationVectorEnum.enforceNegative);
            }

            return _ptNew;
        }

        ParametricTruss m_resTrussStorageLastFireHandleBeamTrussInModel = null;
        public void handleBeamTrussInModel()
        {
            ParametricTruss _resTrussStorageLast = m_resTrussStorageLastFireHandleBeamTrussInModel;
            ParametricTruss _resTrussStorageNew = getBeamTrussLocationAdjustedSingletonOrFetched();
            if ((_resTrussStorageLast == null
                    || _resTrussStorageNew != _resTrussStorageLast
                    )
                )
            {
                if (_resTrussStorageLast != null)
                {
                    /*
                    // todo check integrated
                    if (!ModelViewNodes.Contains(_resTrussStorageLast)
                        || !ModelViewNodes.Contains(_resTrussStorageLast)
                        )
                    { throw new NotImplementedException(); }
                    */

                    if (ModelViewNodes.Contains(_resTrussStorageLast))
                    {
                        (this.ModelViewNodes as List<BaseDataNode>).Remove(_resTrussStorageLast);
                    }
                    if (_modelViewNodesPernamentSotarege.Contains(_resTrussStorageLast))
                    {
                        _modelViewNodesPernamentSotarege.Remove(_resTrussStorageLast);
                    }
                }

                ParametricTruss _baseDataNodeToAdd = _resTrussStorageNew;
                if (_baseDataNodeToAdd != null
                    && !Master.GetDescendantNodes<ParametricTruss>().Contains(_baseDataNodeToAdd)
                    )
                {
                    modelViewNodesAddElem(_baseDataNodeToAdd);
                    modelViewDimensionsClear();
                    List<ModelDimensionLineRTSam> _dimLines = m_resTrussMyHelper.m_trussToolPassed.getBeamTrussModelDimensionLinesSingletonList_andAlign();
                    _dimLines.ForEach(x => modelViewDimensionsAddElem(x)); // Add dimension line to show it in 3D view. When the dimension is edited in the model the system calls DimensionEdited() function.
                    PluginEngine?.PluginUpdate3D(false); // tell plugin system to update the 3D view
                }

                m_resTrussStorageLastFireHandleBeamTrussInModel = _baseDataNodeToAdd;
            }
        }

        //internal BeamTrussToolRTSam m_resTrussTool = null;
        ParametricTrussInsertMemberObjectMyHelper m_resTrussMyHelper = null;
        //internal ParametricTrussRTSam m_resTrussStorage = null;
        internal ParametricTrussRTSamInsertMember m_resTrussStorageHidden;
        internal ParametricTrussRTSamInsertMember m_resTrussStorage
        {
            get
            {
                if (!this.IsInEditMode
                    && this.m_ShowDialogState == CustomStateEnum._pre
                    )
                {
                    return m_resTrussStorageHidden;
                }
                else
                {
                    return this.MyTruss;
                }
            }
            set
            {
                m_resTrussStorageHidden = value;

                if (value == null)
                { return; }

                //this.ResetModelViewNodes();
                /*
                (this.ModelViewNodes as List<BaseDataNode>).Where(x => MyUtils.IsSameOrSubclass(x.GetType(), typeof(Epx.Ristek.Data.Models.ParametricTruss))).ToList().ForEach(x => (this.ModelViewNodes as List<BaseDataNode>).Remove(x));
                this.SetMyTruss(value);
                (this.ModelViewNodes as List<BaseDataNode>).Add(this.MyTruss);
                this._originalMyTruss = this.MyTruss;
                this.InitializeMyTruss(this.MyTruss, null);
                this.PluginEngine?.PluginUpdate3D(false); // tell plugin system to update the 3D view
                */
                // 20240626 adm workarout after 3Dt update 5.0.269 - reversed
                this.InitializeMyTruss(value, null);
                /*
                initializeMyTrussCustom(value,
                    recoverThisTrussToolImportantGeomValues: true
                    );
                */
                /*
                (this.ModelViewNodes as List<BaseDataNode>).Where(x => MyUtils.IsSameOrSubclass(x.GetType(), typeof(Epx.Ristek.Data.Models.ParametricTruss))).ToList().ForEach(x => (this.ModelViewNodes as List<BaseDataNode>).Remove(x));
                this._MyTruss = value;
                this.SetMyTruss(value);
                this._originalMyTruss = this._MyTruss;
                (this.ModelViewNodes as List<BaseDataNode>).Add(this._MyTruss);
                this.PluginEngine?.PluginUpdate3D(false); // tell plugin system to update the 3D view
                this.InitializeMyTruss(this._MyTruss, null);
                this.TrussType = this.TrussType;
                */
            }
        }

        public override Epx.Ristek.ParametricTrusses.PluginParametricTrusses.GeometryGeneratorBase TrussType
        {
            get
            {
                return base.TrussType;
            }
            set
            {
                base.TrussType = value;
            }
        }

        protected bool m_resTrussInitialized = false;
        internal ParametricTrussRTSamInsertMember m_resTrussStorage_RawReferential = null;
        internal (Vector3D, Vector3D, Vector3D) m_newTrussPlainTupleNormalizeNot;
        //  orFetched, because of m_resTrussStorage internals
        public ParametricTruss getBeamTrussLocationAdjustedSingletonOrFetched(bool forceRegenerateMainTruss = false)
        {
            if (this.m_resTrussStorage == null
                || !this.m_resTrussInitialized
                || forceRegenerateMainTruss
                )
            {
                if (!this.IsInEditMode)
                {

                    //BeamTrussToolRSTSamBase m_trussToolTmp = new BeamTrussToolRSTSamBase();
                    BeamTrussToolRSTSamBaseInsertMember m_trussToolTmp = this;
                    ParametricTrussInsertMemberObjectMyHelper _resTrussMyHelperTmp = new ParametricTrussInsertMemberObjectMyHelper(m_trussToolTmp);
                    (Vector3D, Vector3D, Vector3D) _newTrussPlainTupleNormalizeNotTmp;
                    ParametricTrussRTSamInsertMember _ptNewRawReferential = GenerateBeamTrussNewBase(_resTrussMyHelperTmp, out _newTrussPlainTupleNormalizeNotTmp);

                    m_resTrussStorage_RawReferential = _ptNewRawReferential;

                    ParametricTrussInsertMemberObjectMyHelper _resTrussMyHelper = new ParametricTrussInsertMemberObjectMyHelper(this);
                    ParametricTrussRTSamInsertMember _ptNew = GenerateBeamTrussNewBase(_resTrussMyHelper, out this.m_newTrussPlainTupleNormalizeNot);
                    this.m_resTrussMyHelper = _resTrussMyHelper;
                    this.m_resTrussStorage = _ptNew;
                    //this.SetMyTruss(m_resTrussStorage);
                    this.m_resTrussInitialized = true;

                    fireTrussFinalLocationAdjustmentChange();
                }
                else
                {
                    this.m_resTrussInitialized = true;
                }
            }

            return this.m_resTrussStorage;
        }

        public void fireTrussFinalLocationAdjustmentChange()
        {
            Epx.Ristek.Data.Models.ParametricTruss _ptNewOrExisting = this.m_resTrussStorage;
            if (false
                //|| _ptNewOrExisting == null
                || !this.m_resTrussInitialized
                )
            { return; }

            // todo tmp? podmienic na docelowe
            PlaneAlignementEnum _planeAlignement = this.DataFieldsRawObject_nonStatic.PlaneAlignement;

            (System.Windows.Media.Media3D.Vector3D, System.Windows.Media.Media3D.Vector3D, System.Windows.Media.Media3D.Vector3D) _newTrussPlainTupleNormalizeNot = this.m_newTrussPlainTupleNormalizeNot;

            Epx.Ristek.Data.Models.ParametricTruss _ptRawReferential = this.m_resTrussStorage_RawReferential;
            Member _ptTrussMemberAny = _ptNewOrExisting.GetMembers().First();
            // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
            //double _elevation_basicToAxis = _ptTrussMemberAny.Thickness / 2.0;
            double _elevation_basicToAxis;
            // workaround (trial and error) (for integrated)
            if (_planeAlignement == PlaneAlignementEnum.toTop)
            {
                if (m_ShowDialogState == CustomStateEnum._pre)
                {
                    _elevation_basicToAxis = _ptTrussMemberAny.Thickness / 2.0;
                }
                else
                {
                    _elevation_basicToAxis = _ptTrussMemberAny.Thickness;
                }
            }
            else if (_planeAlignement == PlaneAlignementEnum.toBottom)
            {
                if (m_ShowDialogState == CustomStateEnum._pre)
                {
                    // 20240328 Knaga
                    //_elevation_basicToAxis = -_ptTrussMemberAny.Thickness / 2.0;
                    _elevation_basicToAxis = _ptTrussMemberAny.Thickness / 2.0;
                }
                else
                {
                    _elevation_basicToAxis = 0;
                }
            }
            else if (_planeAlignement == PlaneAlignementEnum.toAxis)
            {
                _elevation_basicToAxis = _ptTrussMemberAny.Thickness / 2.0;
            }
            else { throw new NotImplementedException(); }
            Point3D _OriginPointPlaneAlignToAxis = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(_newTrussPlainTupleNormalizeNot.Item1, _newTrussPlainTupleNormalizeNot.Item2, _newTrussPlainTupleNormalizeNot.Item3, _ptRawReferential.Origin, _elevation_basicToAxis, getReferenceEnforcementDirectionVector(), MyUtils.EnforceElevationVectorEnum.enforcePositive);

            int _planeAlignementMultipier;
            Model3DNode.ModelPartAlignment _modelPartAlignment = getInitialModelPartAlignment();
            if (_planeAlignement == PlaneAlignementEnum.toAxis)
            {
                _planeAlignementMultipier = 0;
                //_modelPartAlignment = Model3DNode.ModelPartAlignment.Center;
            }
            else if (_planeAlignement == PlaneAlignementEnum.toTop)
            {
                _planeAlignementMultipier = -1;
                //_modelPartAlignment = Model3DNode.ModelPartAlignment.CenterLeft;
            }
            else if (_planeAlignement == PlaneAlignementEnum.toBottom)
            {
                _planeAlignementMultipier = +1;
                //_modelPartAlignment = Model3DNode.ModelPartAlignment.CenterRight;
            }
            else { throw new NotImplementedException(); }
            //Member _m0_ = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
            //double _elevation_basicToPlane = (_m0_.Width / 2.0 - _ptTrussMemberAny.Thickness / 2.0) * _planeAlignementMultipier;
            double _m0_memberMainAnalog_Width;
            if (!this.Flag_isRistekPlugin_isKnagaMode)
            {
                Member _m0_ = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                _m0_memberMainAnalog_Width = _m0_.Width;
            }
            else
            {
                Support _knagaMode_support = getKnagaMainSupport();
                _m0_memberMainAnalog_Width = _knagaMode_support.Width;
            }
            double _elevation_basicToPlane = (_m0_memberMainAnalog_Width / 2.0 - _ptTrussMemberAny.Thickness / 2.0) * _planeAlignementMultipier;
            // 20240328 Knaga
            //Point3D _OriginPointPlaneAlignBasic = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(_newTrussPlainTupleNormalizeNot.Item1, _newTrussPlainTupleNormalizeNot.Item2, _newTrussPlainTupleNormalizeNot.Item3, _OriginPointPlaneAlignToAxis, _elevation_basicToPlane, getReferenceEnforcementDirectionVector(), Math.Sign(_elevation_basicToPlane) < 0);
            Point3D _OriginPointPlaneAlignBasic = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(_newTrussPlainTupleNormalizeNot.Item1, _newTrussPlainTupleNormalizeNot.Item2, _newTrussPlainTupleNormalizeNot.Item3, _OriginPointPlaneAlignToAxis, _elevation_basicToPlane, getReferenceEnforcementDirectionVector(), MyUtils.EnforceElevationVectorEnum.sideSwitch_whenReferenceResultPositive);
            // 20240328 Knaga
            double _m_0_thicknessAdjustment_realForKnagaSide = 0;
            if (this.Flag_isRistekPlugin_isKnagaMode)
            {
                // 20240702
                //Member _m_i = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                //_m_0_thicknessAdjustment_realForKnagaSide = _m_i.Thickness / 2.0;
                ParametricTruss _pt_i = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                _m_0_thicknessAdjustment_realForKnagaSide = getOffsetFromSideBorderThicknessAxisPlane(_pt_i);
            }

            // for final alignment adjustment
            _ptNewOrExisting.Origin = _OriginPointPlaneAlignBasic;
            _ptNewOrExisting.Alignment = _modelPartAlignment;
            _ptNewOrExisting.AdjustOriginToAlignedPoint();
            double _AlignedStartPoint_To__OriginPointPlaneAlignToAxis_Distance = MyUtils.PointsDistanceAbsolute(_ptNewOrExisting.Origin, _OriginPointPlaneAlignToAxis);
            if (_planeAlignement != PlaneAlignementEnum.toAxis
                && MyUtils.CompareWithTollerance(_AlignedStartPoint_To__OriginPointPlaneAlignToAxis_Distance, _m0_memberMainAnalog_Width / 2.0 - _ptTrussMemberAny.Thickness, 0.01) != 0
                )
            {
                if (_ptNewOrExisting.Alignment == Model3DNode.ModelPartAlignment.CenterRight)
                {
                    _ptNewOrExisting.Alignment = Model3DNode.ModelPartAlignment.CenterLeft;
                }
                else if (_ptNewOrExisting.Alignment == Model3DNode.ModelPartAlignment.CenterLeft)
                {
                    _ptNewOrExisting.Alignment = Model3DNode.ModelPartAlignment.CenterRight;
                }
                else { throw new NotImplementedException(); }
                _ptNewOrExisting.AdjustOriginToAlignedPoint();
            }

            double _finalElevationAdjustmentVal = this.DataFieldsRawObject_nonStatic.BeamTrussOffsetPerpendicular_mm;
            // 20240328 Knaga
            //Point3D _OriginPointPlaneAlignElevationFinal = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(_newTrussPlainTupleNormalizeNot.Item1, _newTrussPlainTupleNormalizeNot.Item2, _newTrussPlainTupleNormalizeNot.Item3, _OriginPointPlaneAlignBasic, _finalElevationAdjustmentVal, getReferenceEnforcementDirectionVector(), Math.Sign(_finalElevationAdjustmentVal) < 0);
            Point3D _OriginPointPlaneAlignElevationFinal = MyUtils.ElevateOriginGlobalWithForcedDirectionZ(_newTrussPlainTupleNormalizeNot.Item1, _newTrussPlainTupleNormalizeNot.Item2, _newTrussPlainTupleNormalizeNot.Item3, _OriginPointPlaneAlignBasic, _finalElevationAdjustmentVal, getReferenceEnforcementDirectionVector(), MyUtils.EnforceElevationVectorEnum.sideSwitch_whenReferenceResultPositive);

            Vector3D pt0_p1_to_p2_vectorNormalized = _ptNewOrExisting.XAxis;
            pt0_p1_to_p2_vectorNormalized.Normalize();
            Point3D _OriginPointPlaneAlignElevationFinal_LengwiseOffseted = MyUtils.TranslatePoint(_OriginPointPlaneAlignElevationFinal, pt0_p1_to_p2_vectorNormalized,
                DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOffsetLengwiseBottom_mm() + _m_0_thicknessAdjustment_realForKnagaSide
                );

            // 20240328 Knaga
            Vector3D pt0_to_pt1_vectorNormalized_ = _ptNewOrExisting.YAxis;
            pt0_to_pt1_vectorNormalized_.Normalize();
            Point3D _OriginPointPlaneAlignElevationFinal_OutreachOffseted_ = MyUtils.TranslatePoint(_OriginPointPlaneAlignElevationFinal_LengwiseOffseted, -pt0_to_pt1_vectorNormalized_, DataFieldsRawObject_nonStatic.getModeAdjusted_BeamTrussOutreachPerpPrimary_mm());

            // not working, destination point with offet set in GenerateBeamTrussNewBase
            /*
            double lengthNew = m_resTrussTool.Length - DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseBottom_mm - DataFieldsRawObject_nonStatic.BeamTrussOffsetLengwiseTop_mm;
            Point3D _DestinationPoint_LengwiseOffseted_ = MyUtils.TranslatePoint(_OriginPointPlaneAlignElevationFinal_LengwiseOffseted, pt0_p1_to_p2_vectorNormalized, lengthNew);
            // not working
            //m_resTrussTool.SetDimensions(lengthNew, null, null, null, m_resTrussTool.Height, null, null, null, null, null);
            */

            // not needed
            /*
            Vector3D _planeNormalBeamTruss = MyUtils.CalculateNormal(MyUtils.ConvertToPoint3D(m_newTrussPlainTupleNormalizeNot.Item1), MyUtils.ConvertToPoint3D(m_newTrussPlainTupleNormalizeNot.Item2), MyUtils.ConvertToPoint3D(m_newTrussPlainTupleNormalizeNot.Item3));
            _planeNormalBeamTruss.Normalize();
            _ptNewOrExisting.SetXAxis(_DestinationPoint_LengwiseOffseted_ - _OriginPointPlaneAlignElevationFinal_LengwiseOffseted, _planeNormalBeamTruss);
            */

            //_ptNewOrExisting.Origin = _OriginPointPlaneAlignElevationFinal;
            // 20240328 Knaga
            //_ptNewOrExisting.Origin = _OriginPointPlaneAlignElevationFinal_LengwiseOffseted;
            _ptNewOrExisting.Origin = _OriginPointPlaneAlignElevationFinal_OutreachOffseted_;
            // already used previous
            //_ptNewOrExisting.AdjustOriginToAlignedPoint();

            this.PluginEngine?.PluginUpdate3D(true); // tell plugin system to update the 3D view
        }

        BaseDataNode m_getParentObjectForNewBeam_Parent;
        BaseDataNode m_getParentObjectForNewBeam;
        public BaseDataNode getParentObjectForNewBeam()
        {
            if (m_getParentObjectForNewBeam == null)
            {
                ParametricTruss _pt0 = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);

                string _folderName = Strings.Strings._beamTrussesFolderNameInBuilding;

                int foldersCount = 0;
                BaseDataNode _parentCurr = _pt0.Parent;
                while (!(_parentCurr.HasChild<ModelFolderNode>()
                        && _parentCurr.GetChildByName(_folderName) != null
                        )
                    && !(_parentCurr is ModelFolderNode
                        && foldersCount > 0
                        )
                    && _parentCurr.Parent != null
                    && _parentCurr != this.BuildingNodeForThis
                    && !(_parentCurr is PluginStorageNode)
                    )
                {
                    if (_parentCurr is ModelFolderNode)
                    {
                        ++foldersCount;
                    }

                    _parentCurr = _parentCurr.Parent;
                }

                if (_parentCurr.GetChildByName(_folderName) != null)
                {
                    m_getParentObjectForNewBeam = _parentCurr.GetChildByName(_folderName);
                }
                else
                {
                    ModelFolderNode _newFolder = new ModelFolderNode(_folderName);
                    //this.modelViewNodesAddElem(_newFolder); // show in 3D view while plugin is running

                    m_getParentObjectForNewBeam = _newFolder;
                    m_getParentObjectForNewBeam_Parent = _parentCurr;
                }
            }
            return m_getParentObjectForNewBeam;
        }

        BaseDataNode m_getParentObjectForNewSupports;
        public BaseDataNode getParentObjectForNewSupports()
        {
            if (m_getParentObjectForNewSupports == null)
            {
                m_getParentObjectForNewBeam = getParentObjectForNewBeam();

                string _folderName = Strings.Strings._supportsFolderName;

                if (m_getParentObjectForNewBeam.GetChildByName(_folderName) != null)
                {
                    m_getParentObjectForNewSupports = m_getParentObjectForNewBeam.GetChildByName(_folderName);
                }
                else
                {
                    ModelFolderNode _newFolder = new ModelFolderNode(_folderName);
                    m_getParentObjectForNewBeam.AddChild(_newFolder);

                    m_getParentObjectForNewSupports = _newFolder;
                }
            }
            return m_getParentObjectForNewSupports;
        }

        #endregion

        // =======================================

        #region _various, just for lerning

        // just for lerning
        protected override void InitializeInternals()
        {
            base.InitializeInternals();
        }

        // just for lerning
        public override void InitializeFromNode(IPluginNode selectedTemplateNode, IEnumerable<IPluginNode> allTemplateNodes)
        {
            base.InitializeFromNode(selectedTemplateNode, allTemplateNodes);
        }

        // just for lerning
        public override void SetUserDefaultSettings()
        {
            base.SetUserDefaultSettings();
        }

        #endregion

        #region ParametricTrussModelController

        public override void DimensionEdited(ModelDimensionLine dimensionLine, double newValue, DimensionEditType editType)
        {
            base.DimensionEdited(dimensionLine, newValue, editType);
        }

        #endregion

        #region ParametricBaseTruss

        public override void CreateModel(bool createLoads = true)
        {
            base.CreateModel(createLoads);

            bool _updateTrussOpenings =
                !this.IsInEditMode
                && this.m_ShowDialogState == CustomStateEnum._pre
                ;
            bool _updateSupports =
                this.IsInEditMode
                && this.m_ShowDialogState == CustomStateEnum._post
                ;
            resetTrussRTSamParams(updateTrussOpenings: _updateTrussOpenings, updateSupports: _updateSupports);

            if (true
                && this.IsInEditMode
                && !this.m_CreateModelCustomActual
                && this.m_ShowDialogState == CustomStateEnum._during
                )
            {
                // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
                this._MyTruss.Alignment = this.m_AlignmentDestination;
                this._MyTruss.Origin = this.m_initializationTrussOrigin;
            }

        }

        bool m_CreateModelCustomActual = false;
        public void CreateModelCustom(bool createLoads = true)
        {
            m_CreateModelCustomActual = true;
            this.CreateModel(createLoads);
            m_CreateModelCustomActual = false;
        }

        #endregion

        #region methods

        public void setTrussType(ref Epx.Ristek.ParametricTrusses.PluginParametricTrusses.PluginBaseTruss pbt, double height, Point3D origin, Point3D directionPoint)
        {
            double _ptLength = (directionPoint - origin).Length;
            setTrussType(ref pbt, height, _ptLength);

        }

        public void setTrussType(ref Epx.Ristek.ParametricTrusses.PluginParametricTrusses.PluginBaseTruss pbt, double height, double ptLength)
        {
            if (!this.m_DataFieldsRawObject_nonStatic.AutosetTrussParamsInEditMode)
            {
                return;
            }

            // choose member arrangement type
            var tyypit = pbt.ParametricTrussTypes;

            int _numberOfOpeningsUsed = (int)Math.Round(
                // 20240626
                //(ptLength / (height * 1.8)) + 0.5
                (ptLength / (height * 1.8)) - 0.5
                , 0);

            if (_numberOfOpeningsUsed > 1)
            {
                //Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8 _trussType_casted8 = pbt.ParametricTrussTypes.Where(x => x.TrussType is Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8).FirstOrDefault() as Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8;
                //Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8 _trussType_casted8 = tyypit.Where(x => x.TrussType is Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8).FirstOrDefault() as Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8;
                Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8 _trussType_casted8 = tyypit[2] as Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss8;

                _numberOfOpeningsUsed = Math.Max(1, _numberOfOpeningsUsed);
                _numberOfOpeningsUsed = Math.Min(12, _numberOfOpeningsUsed);
                _trussType_casted8.NumberOfOpenings = _numberOfOpeningsUsed;

                // set the chosen type
                pbt.TrussType = _trussType_casted8;
            }
            else
            {
                Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss1 _trussType_casted1 = tyypit[0] as Epx.Ristek.ParametricTrusses.PluginParametricTrusses.BeamTruss1;

                pbt.TrussType = _trussType_casted1;
            }
        }

        // inEditMode doesnt do anything by now
        public void addLoadsForTrussCurrent(bool _resetLoadsForced = false)
        {
            ParametricTrussRTSamInsertMember _ptUsed = this.m_ParametricTrussRTSamCurrent;
            InsertMemberObjectPlugin _trussTool = this;

            // m_resetLoadsFlag not used
            //bool _resetLoadsUsed = m_resetLoadsFlag || _resetLoadsForced;
            bool _resetLoadsUsed = _resetLoadsForced;

            // loads generated manually
            /*
            {
                // wyniesienie paska powyzej truss
                double _distributedLoadOffset = 500;
                List<DistributedLoad> _dlList = new List<DistributedLoad>();
                {
                    DistributedLoad load = new DistributedLoad("qk,ri");
                    load.StartPoint = new Point(_trussTool.Length, -_distributedLoadOffset);
                    load.EndPoint = new Point(0, -_distributedLoadOffset);
                    _dlList.Add(load);
                }
                {
                    DistributedLoad load = new DistributedLoad("qk,le");
                    load.StartPoint = new Point(0, _trussTool.Height + _distributedLoadOffset);
                    load.EndPoint = new Point(_trussTool.Length, _trussTool.Height + _distributedLoadOffset);
                    _dlList.Add(load);
                }

                double _windLoadOffset = 1000;
                List<WindLoad> _wlList_ = new List<WindLoad>();
                {
                    WindLoad load = new WindLoad("We,ri");
                    load.StartPoint = new Point(_trussTool.Length, -_windLoadOffset);
                    load.EndPoint = new Point(0, -_windLoadOffset);
                    _wlList_.Add(load);
                }
                {
                    WindLoad load = new WindLoad("We,le");
                    load.StartPoint = new Point(0, _trussTool.Height + _windLoadOffset);
                    load.EndPoint = new Point(_trussTool.Length, _trussTool.Height + _windLoadOffset);
                    _wlList_.Add(load);
                }

                _ptUsed.LoadsNode.RemoveAllChildren();

                foreach (DistributedLoad _dlCurr in _dlList)
                {
                    _dlCurr.LoadDurationClass = Epx.Ristek.Data.Settings.DesignCodes.DesignCode.LoadDurationClassEnum.MidTerm;
                }
                foreach (DistributedLoad _dlCurr in _dlList.Union(_wlList_))
                {
                    _dlCurr.Magnitude1Begin = 0.5;
                    _dlCurr.UseTrussSpacingForWidth = false;
                    // szerokosc zbeirania obciazen, zeby zamienic na liniowe
                    _dlCurr.EffectiveWidth = 1000;
                    //_ptNew.AddChild(_dlCurr);
                    _ptUsed.LoadsNode.AddChild(_dlCurr);
                }
            }
            */

            // loads with autogenerator
            // by trial and error
            if (_resetLoadsUsed
                )
            {
                // after integrated - always false
                bool inEditMode = !_resetLoadsForced;
                //if (!inEditMode)
                {
                    this.SetDefaultLoads(_ptUsed, inEditMode);
                }
                if (!inEditMode)
                {
                    _ptUsed.LoadsNode.RemoveAllChildren();
                }

                this.LoadsGenerator.LoadTools
                    .Where(x =>
                        x.LoadName == "We,ri"
                        || x.LoadName == "We,le"
                        || x.LoadName == "qk,ri"
                        || x.LoadName == "qk,le"
                    ).ToList()
                    .ForEach(x =>
                    {
                        if (!x.AddLoad
                            )
                        {
                            x.Magnitude = 0.5;
                            x.EffectiveWidth = 1000;
                            x.AddLoad = true;
                        }
                    });

                if (!inEditMode)
                {
                    this.CreateLoads();
                }
            }
        }

        public Support getKnagaMainSupport()
        {
            Support res = null;

            if (this.Flag_isRistekPlugin_isKnagaMode)
            {
                Support _knagaMode_support = (_preInputs[0] as PluginObjectInput).Object as Support;
                res = _knagaMode_support;
            }

            return res;
        }

        List<Support> m_getBeamTrussSupportsGlobal;
        public List<Support> updateBeamTrussSupportsGlobal(bool doNotRecover = true, bool isForUndo = false)
        {
            double _supportThickness = 24;

            if (m_getBeamTrussSupportsGlobal == null || !doNotRecover)
            {
                List<Support> resList;
                if (doNotRecover)
                {
                    resList = new List<Support>();

                    if (!this.DataFieldsRawObject_nonStatic.AutoaddSupports)
                    {
                        // do nothing
                    }
                    else
                    {

                        Support _support1 = new Support(Strings.Strings._Support1);
                        _support1.StructuralJointType = Support.SupportJointType.RotationPossible;
                        // musi byc na tym etapie
                        _support1.Thickness = _supportThickness;
                        // musi byc na tym etapie zeby edycja nie przestawiala (albo i nie, ale conajmniej nie preszkadza)
                        _support1.Alignment = Epx.BIM.Models.Model3DNode.ModelPartAlignment.TopLeft;
                        resList.Add(_support1);



                        Support _support2 = new Support(Strings.Strings._Support2);
                        _support2.StructuralJointType = Support.SupportJointType.RollingJoint;
                        _support2.Thickness = _supportThickness;
                        _support2.Alignment = Model3DNode.ModelPartAlignment.TopRight;
                        resList.Add(_support2);

                    }

                    foreach (Support _supportCurr in resList)
                    {
                        createOrReplaceInStringAttributes_support_guidOriginal(_supportCurr);
                        createOrReplaceInStringAttributes_support_originalName(_supportCurr);
                    }
                }
                else
                {
                    if (this.MySupports.Count == 2)
                    {
                        // _support2 is present only for edit, when new truss is to be longer after edit
                        resList = this.MySupports;
                    }
                    // all the same as this.MySupports
                    /*
                    else if (this.m_ParametricTrussRTSamCurrent.GetSupports(Support.WithLimitStateEnum.All).Count == 2)
                    {
                        resList = this.m_ParametricTrussRTSamCurrent.GetSupports(Support.WithLimitStateEnum.All);
                    }
                    else if (this.m_ParametricTrussRTSamCurrent.GetAllSupports().Count == 2)
                    {
                        resList = this.m_ParametricTrussRTSamCurrent.GetAllSupports();
                    }
                    */
                    else
                    {
                        resList = recoverFromMyData_Supports();
                    }
                }

                m_getBeamTrussSupportsGlobal = resList;
            }
            m_getBeamTrussSupportsGlobal = m_getBeamTrussSupportsGlobal.Where(x => x != null).ToList();

            //Epx.Ristek.ParametricTrusses.ParametricBaseTruss _resTrussTool = this;
            double _referencjeTrussLength;
            //ParametricTruss _ptNew = GenerateBeamTrussNewBase(ref _resTrussTool, out _newTrussPlainTupleNormalizeNot);
            ParametricTrussRTSamInsertMember _ptFetched;
            if (!isForUndo)
            {
                InsertMemberObjectPlugin _resTrussTool = this;

                _ptFetched = this.m_ParametricTrussRTSamCurrent;
                _referencjeTrussLength = _resTrussTool.Length;
            }
            else
            {
                _ptFetched = this.m_ParametricTrussRTSamOriginal;
                _referencjeTrussLength = _ptFetched.XAxis.Length;
            }

            // working version - adding to global
            //BaseDataNode _supportFolder = getParentObjectForNewBeam();
            //Point3D _pOrigin = _ptFetched.Origin;
            Point3D _pOrigin = _ptFetched.Origin;
            Vector3D _xDir = _ptFetched.XAxis; _xDir.Normalize();
            //Vector3D xVector = _xDir * _resTrussTool.Length;
            Vector3D xVector = _xDir * (_referencjeTrussLength - 1.0);
            Vector3D yDir = _ptFetched.YAxis;
            Vector3D zDir = _ptFetched.ZAxis; zDir.Normalize();

            // adding to local - big, only first one added is handled properly
            /*
            ////BaseDataNode _supportFolder = new Epx.BIM.Models.ModelFolderNode("Supports");
            ////_ptNew.AddChild(_supportFolder);
            //BaseDataNode _supportFolder = getParentObjectForNewBeam();
            Point3D _pOrigin = new Point3D();
            Vector3D xVector = new Vector3D(1, 0, 0) * _resTrussTool.Length;
            Vector3D yDir = new Vector3D(0, 1, 0);
            Vector3D zDir = new Vector3D(0, 0, 1);
            */

            double _sideExtension = _ptFetched.Thickness * 3.0 / 2.0;


            // crashes on plugin init when trying to edit after 3dtr model save
            //if (m_getBeamTrussSupportsGlobal.Exists(x => x.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support1))
            if (m_getBeamTrussSupportsGlobal.Exists(x => x?.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support1))
            {
                //Support _support1 = new Support(Strings.Strings._Support1);
                //Support _support1 = m_getBeamTrussSupportsGlobal[0];
                //Support _support1 = m_getBeamTrussSupportsGlobal.Where(x => x.Name == Strings.Strings._Support1).FirstOrDefault();
                //Support _support1 = m_getBeamTrussSupportsGlobal.Where(x => x.StringAttributes.ContainsKey(__FIELD_STR_support_originalName) && x.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support1).FirstOrDefault();
                Support _support1 = m_getBeamTrussSupportsGlobal.Where(x => x.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support1).FirstOrDefault();

                // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
                //_support1.SetAlignedStartPoint(_pOrigin + zDir * _sideExtension, yDir);
                //_support1.SetAlignedEndPoint(_pOrigin - zDir * _sideExtension, yDir);
                Point3D _AlignedStartPointRaw = _pOrigin + zDir * _sideExtension;
                Point3D _AlignedEndPointRaw_ = _pOrigin - zDir * _sideExtension;
                if (this.IsInEditMode)
                {
                    _AlignedStartPointRaw += -zDir * _ptFetched.Thickness / 2.0;
                    _AlignedEndPointRaw_ += zDir * _ptFetched.Thickness / 2.0;
                }
                _support1.SetAlignedStartPoint(_AlignedStartPointRaw, yDir);
                _support1.SetAlignedEndPoint(_AlignedEndPointRaw_, yDir);
            }

            //if (m_getBeamTrussSupportsGlobal.Exists(x => x.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support2))
            if (m_getBeamTrussSupportsGlobal.Exists(x => x?.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support2))
            {
                //Support _support2 = new Support(Strings.Strings._Support2);
                //Support _support2 = m_getBeamTrussSupportsGlobal[1];
                Support _support2 = m_getBeamTrussSupportsGlobal.Where(x => x.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support2).FirstOrDefault();

                // workaround (trial and error) in _doDelegateBase, InitializeMyTrussProperties, CreateModel, fireTrussFinalLocationAdjustmentChange and other
                //_support2.SetAlignedStartPoint(_pOrigin + xVector + zDir * _sideExtension, yDir);
                //_support2.SetAlignedEndPoint(_pOrigin + xVector - zDir * _sideExtension, yDir);
                Point3D _AlignedStartPointRaw = _pOrigin + xVector + zDir * _sideExtension;
                Point3D _AlignedEndPointRaw_ = _pOrigin + xVector - zDir * _sideExtension;
                if (this.IsInEditMode)
                {
                    _AlignedStartPointRaw += -zDir * _ptFetched.Thickness / 2.0;
                    _AlignedEndPointRaw_ += zDir * _ptFetched.Thickness / 2.0;
                }
                _support2.SetAlignedStartPoint(_AlignedStartPointRaw, yDir);
                _support2.SetAlignedEndPoint(_AlignedEndPointRaw_, yDir);
            }


            // generates some problem for do/undo
            if (!doNotRecover)
            {
                List<BaseDataNode> _ModelViewNodesDeepCopy = new List<BaseDataNode>(this.ModelViewNodes);
                (this.ModelViewNodes as List<BaseDataNode>).Clear();
                (this.ModelViewNodes as List<BaseDataNode>).AddRange(m_getBeamTrussSupportsGlobal);

                List<(Support, BaseDataNode)> _tupList = m_getBeamTrussSupportsGlobal.Select(x =>
                {
                    BaseDataNode _parent = x.Parent;
                    _parent.RemoveChild(x);
                    return (x, _parent);
                }).ToList();
                this.PluginEngine?.PluginUpdate3D(false);

                _tupList.ForEach(x =>
                {
                    BaseDataNode _parent = x.Item2;
                    _parent.AddChild(x.Item1);
                });
                if (isForUndo)
                {
                    // generates more problem for do/undo
                    //this.PluginEngine?.PluginUpdate3D(false);
                }

                (this.ModelViewNodes as List<BaseDataNode>).Clear();
                (this.ModelViewNodes as List<BaseDataNode>).AddRange(_ModelViewNodesDeepCopy);
            }

            return m_getBeamTrussSupportsGlobal;
        }

        ModelDimensionLineRTSam m_dimensionLineHeightStorage_;
        ModelDimensionLineRTSam m_dimensionLineWidthStorage;
        // only helper for preDialog stage
        public List<ModelDimensionLineRTSam> getBeamTrussModelDimensionLinesSingletonList_andAlign()
        {
            ParametricTrussRTSamInsertMember _resTruss = this.m_ParametricTrussRTSamCurrent;
            InsertMemberObjectPlugin _trussTool = this;

            if (m_dimensionLineHeightStorage_ == null)
            {
                //ModelDimensionLine dimLine = new ModelDimensionLine(_resTrussStorage);
                ModelDimensionLineRTSam dimLine = new ModelDimensionLineRTSam(_resTruss);

                dimLine.Name = Strings.Strings._DimensionHeight;

                m_dimensionLineHeightStorage_ = dimLine;
            }
            if (_resTruss != null)
            {
                ModelDimensionLine dimLine = m_dimensionLineHeightStorage_;
                dimLine.Origin = _resTruss.Origin;
                Point3D _transformedPointH = _resTruss.LocalToGlobal.Transform(GeometryMath.ToPoint3D(new Point(0, _trussTool.Height)));
                dimLine.XAxis = new Vector3D(_transformedPointH.X - dimLine.Origin.X, _transformedPointH.Y - dimLine.Origin.Y, _transformedPointH.Z - dimLine.Origin.Z);
                dimLine.YAxis = _resTruss.XAxis;
                dimLine.ZAxis = _resTruss.ZAxis;
            }

            if (m_dimensionLineWidthStorage == null
                )
            {
                //ModelDimensionLine dimLine = new ModelDimensionLine(_resTrussStorage);
                ModelDimensionLineRTSam dimLine = new ModelDimensionLineRTSam(_resTruss);
                //dimLine.IsDimensionEditable = true;

                dimLine.Name = Strings.Strings._DimensionLength;

                m_dimensionLineWidthStorage = dimLine;

            }
            if (_resTruss != null)
            {
                ModelDimensionLine dimLine = m_dimensionLineWidthStorage;
                dimLine.Origin = _resTruss.Origin;
                Point3D _transformedPointW = _resTruss.LocalToGlobal.Transform(GeometryMath.ToPoint3D(new Point(_trussTool.Length, 0)));
                dimLine.XAxis = new Vector3D(_transformedPointW.X - dimLine.Origin.X, _transformedPointW.Y - dimLine.Origin.Y, _transformedPointW.Z - dimLine.Origin.Z);
                dimLine.YAxis = _resTruss.YAxis;
                dimLine.ZAxis = _resTruss.ZAxis;
            }

            List<ModelDimensionLineRTSam> resList = new List<ModelDimensionLineRTSam>();
            if (m_dimensionLineHeightStorage_ != null)
            {
                resList.Add(m_dimensionLineHeightStorage_);
            }
            if (m_dimensionLineWidthStorage != null)
            {
                resList.Add(m_dimensionLineWidthStorage);
            }

            return resList;
        }

        // w strone w ktorej ma byc top
        protected Vector3D? getReferenceEnforcementDirectionVector()
        {
            if (this.Flag_isRistekPlugin_isKnagaMode)
            {
                Support _knagaMode_support = getKnagaMainSupport();
                Member _knagaMode_mA_forSupportLimes = getTrussMemberFromPluginInput_(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);
                (Point3D, Point3D)? _knagaMode_mA_intersectionPointsPair = GetMemberAboveSupportAxesVirtualIntersectionPointsPair(_knagaMode_support, _knagaMode_mA_forSupportLimes);
                Point3D _knagaPoint = _knagaMode_mA_intersectionPointsPair.Value.Item1;
                ParametricTruss _referenceTruss = getParametricTrussFromPluginInput(_preInputs[this.PreDialogInputFirstMemeberOffset + 0]);

                Vector3D _knagaModeReferenceEnforcementDirectionVector = _knagaPoint - _referenceTruss.DirectionXminYLine.MidPoint;
                return _knagaModeReferenceEnforcementDirectionVector;
            }
            else //if (!this.Flag_isRistekPlugin_isKnagaMode)
            {
                return new Vector3D(0, 0, 1);
            }
        }

        #endregion

        #region methods helpers

        public const string __FIELD_STR_trussStorageFieldName = "RTSam_MyTechnicalAdditionalInfo1";
        protected string getRecordInMyDataInTrussStorageStrVal(ParametricTrussRTSamInsertMember ptUsed)
        {
            if (ptUsed.StringAttributes.ContainsKey(__FIELD_STR_trussStorageFieldName))
            {
                return ptUsed.StringAttributes[__FIELD_STR_trussStorageFieldName];
            }
            else
            {
                return "";
            }
        }

        protected void setRecordInMyDataInTrussStorageStrVal(ParametricTrussRTSamInsertMember ptUsed, string newStr)
        {
            BaseDataNode node = ptUsed;
            string _key = __FIELD_STR_trussStorageFieldName;
            string _val = newStr;
            createOrReplaceStringAttribute(node, _key, _val);
        }

        public void updateOrCreateRecordInMyDataInTrussStorageObj(List<(string, string)> keyValTupList, ParametricTrussRTSamInsertMember forSelectedTrussOnly = null)
        {
            var ptList = new List<ParametricTrussRTSamInsertMember>();
            if (forSelectedTrussOnly != null)
            {
                ptList.Add(forSelectedTrussOnly);
            }
            else
            {
                if (this.m_ParametricTrussRTSamCurrent != null)
                {
                    ptList.Add(this.m_ParametricTrussRTSamCurrent);
                }
                if (this._originalMyTruss != null)
                {
                    ptList.Add(this._originalMyTruss);
                }
            }

            // adm z sensem powtarzac dla obu? nie lepiej skopiowac jedno do drugiej? or something ; poki co wersja nizej dziala
            foreach (ParametricTrussRTSamInsertMember ptUsed in ptList)
            {
                List<string> _recordListOld = new List<string>();
                List<string> _recordListToUpdate = new List<string>();
                foreach (string _myRecordPottentialCurr in getRecordInMyDataInTrussStorageStrVal(ptUsed).Split('|'))
                {
                    if (_myRecordPottentialCurr.Contains(":")
                        && keyValTupList != null
                        && (keyValTupList.Exists(x => x.Item1 == _myRecordPottentialCurr.Split(':')[0]))
                        )
                    {
                        _recordListToUpdate.Add(_myRecordPottentialCurr);
                    }
                    else if (String.IsNullOrEmpty(_myRecordPottentialCurr))
                    {
                        // do nothing
                    }
                    else
                    {
                        _recordListOld.Add(_myRecordPottentialCurr);
                    }
                }
                List<string> _recordListNew = keyValTupList.Select(x => x.Item1 + ":" + x.Item2).ToList();
                List<string> _recordListFinal = new List<string>();
                _recordListFinal.AddRange(_recordListOld);
                _recordListFinal.AddRange(_recordListNew);
                string _guidStoredStr = String.Join("|", _recordListFinal);

                setRecordInMyDataInTrussStorageStrVal(ptUsed, _guidStoredStr);
            }
        }
        public void updateOrCreateRecordInMyDataInTrussStorageObj(string _key, string _value, ParametricTrussRTSamInsertMember forSelectedTrussOnly = null)
        {
            updateOrCreateRecordInMyDataInTrussStorageObj(new (string, string)[] { (_key, _value) }.ToList(), forSelectedTrussOnly);
        }

        public IEnumerable<string> recoverFromMyDataFieldAny(string _key, ParametricTrussRTSamInsertMember ptUsed)
        {
            string _idStr = _key;
            //ParametricTrussRTSam ptUsed = this.m_ParametricTrussRTSamCurrent;

            if (ptUsed != null
                && getRecordInMyDataInTrussStorageStrVal(ptUsed).Contains(_idStr) == true)
            {
                return getRecordInMyDataInTrussStorageStrVal(ptUsed).Split('|').Where(x => x.Split(':')[0] == _idStr).Select(x => x.Split(':')[1]).ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        public void resetMyDataInTrussStorageObj(List<Support> supportList, bool isNewlyCreatedByRTSamPlugin)
        {
            List<(string, string)> _keyValTupList = new List<(string, string)>();
            _keyValTupList.AddRange(supportList.Select(x => (__FIELD_STR_support_guidOriginal, x.UniqueID.ToString())));
            _keyValTupList.Add((__FIELD_STR_isTrussNewlyCreated, isNewlyCreatedByRTSamPlugin.ToString()));
            updateOrCreateRecordInMyDataInTrussStorageObj(_keyValTupList, null);
        }

        public const string __FIELD_STR_isTrussNewlyCreated = "RTSam_isTrussNewlyCreated";
        // not usefull anymore
        public bool? recoverFromMyData_isNewlyCreatedByRTSamPlugin()
        {
            string _idStr = __FIELD_STR_isTrussNewlyCreated;
            ParametricTrussRTSamInsertMember ptUsed = this.m_ParametricTrussRTSamCurrent;
            IEnumerable<string> _valueStr = recoverFromMyDataFieldAny(_idStr, ptUsed);
            if (_valueStr.Count() > 0)
            {
                return Boolean.Parse(_valueStr.FirstOrDefault());
            }
            else
            {
                return null;
            }
        }

        public const string __FIELD_STR_support_guidOriginal = "RTSam_support_guidOriginal";
        public List<Support> recoverFromMyData_Supports()
        {
            List<Support> res = new List<Support>(2);
            res.Add(null);
            res.Add(null);

            string _idStr = __FIELD_STR_support_guidOriginal;
            ParametricTrussRTSamInsertMember ptUsed = this.m_ParametricTrussRTSamCurrent;
            IEnumerable<string> _valueStrList = recoverFromMyDataFieldAny(_idStr, ptUsed);
            foreach (string _guidStrCurr in _valueStrList)
            {
                Support _objRaw = ptUsed.ProjectRoot.GetDescendantNodes<Support>().Where(x => x.StringAttributes.ContainsKey(__FIELD_STR_support_guidOriginal) && x.StringAttributes[__FIELD_STR_support_guidOriginal] == _guidStrCurr).FirstOrDefault();
                if (_objRaw != null)
                {
                    createOrReplaceInStringAttributes_support_guidOriginal(_objRaw);
                    //if (_objRaw.Name == Strings.Strings._Support1)
                    if (_objRaw.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support1)
                    {
                        res[0] = _objRaw;
                    }
                    //else if (_objRaw.Name == Strings.Strings._Support2)
                    else if (_objRaw.StringAttributes[__FIELD_STR_support_originalName] == Strings.Strings._Support2)
                    {
                        res[1] = _objRaw;
                    }
                }
                else //if (_objRaw == null)
                {
                    if (!this.IsInEditMode
                        && !this.DataFieldsRawObject_nonStatic.AutoaddSupports
                        )
                    { throw new NotImplementedException(); }
                }
            }

            return res;
        }

        public void createOrReplaceStringAttribute(BaseDataNode node, string _key, string _val)
        {
            Dictionary<string, string> _nodeCurr_StringAttributesCopy = node.StringAttributes.ToDictionary(entry => entry.Key,
                                   entry => entry.Value);
            if (_nodeCurr_StringAttributesCopy.ContainsKey(_key))
            {
                _nodeCurr_StringAttributesCopy.Remove(_key);
            }
            _nodeCurr_StringAttributesCopy.Add(_key, _val);
            node.StringAttributes = _nodeCurr_StringAttributesCopy;
        }

        public void createOrReplaceInStringAttributes_support_guidOriginal(Support support)
        {
            createOrReplaceStringAttribute(support, __FIELD_STR_support_guidOriginal, support.UniqueID.ToString());
        }

        public const string __FIELD_STR_support_originalName = "RTSam_support_originalName";
        public void createOrReplaceInStringAttributes_support_originalName(Support support)
        {
            BaseDataNode node = support;
            string _key = __FIELD_STR_support_originalName;
            string _val = support.Name;
            createOrReplaceStringAttribute(node, _key, _val);
        }

        protected void resetTrussRTSamParams(bool updateTrussOpenings = false, bool updateSupports = false, bool isForUndo = false)
        {
            if (updateTrussOpenings)
            {
                Epx.Ristek.ParametricTrusses.PluginParametricTrusses.PluginBaseTruss pbt = this;
                double height = base.Height;
                double ptLength = base.Length;
                this.setTrussType(ref pbt, height, ptLength);
            }
            {
                //this.addLoadsForTrussCurrent(false);
                if (updateSupports)
                {
                    List<Support> _supportList = this.updateBeamTrussSupportsGlobal(false, isForUndo);
                    resetMyDataInTrussStorageObj(_supportList, false);
                }
            }
        }

        public Project getProjectObj()
        {
            BaseDataNode _project = this.Master;
            while (_project != null && !(_project is Project))
            {
                _project = _project.Parent;
            }
            return _project as Project;
        }

        // 20240626 adm workarout after 3Dt update 5.0.269 - reversed (method not used currently)
        internal void initializeMyTrussCustom(ParametricTrussRTSamInsertMember value, bool recoverThisTrussToolImportantGeomValues)
        {
            restoreMyTrussParamsAfterAction(() =>
            {
                // 20240626 adm workarout after 3Dt update 5.0.269
                //this.InitializeMyTruss(value, target: getProjectObj().GetTargetFolder());
                return null;
            },
            //this.MyTruss
            recoverThisTrussToolImportantGeomValues
                ? value
                : null
                );

        }

        internal object restoreMyTrussParamsAfterAction(Func<object> lambda, ParametricTrussRTSamInsertMember recoverTrussImportantGeomValues)
        {
            double? _workaroundStorage_Length = null;
            Dictionary<object, IEnumerable<string>> _storageParamNamesDict = new Dictionary<object, IEnumerable<string>>();

            _storageParamNamesDict.Add(this, new string[] {
                "Height",
                "TrussType",
                "NumberOfOpenings",
            });
            if (recoverTrussImportantGeomValues != null)
            {
                _workaroundStorage_Length = recoverTrussImportantGeomValues.XAxis.Length;
                _storageParamNamesDict.Add(recoverTrussImportantGeomValues, new string[] {
                    "Origin",
                    "XAxis",
                    "YAxis",
                    "ZAxis",
                    "Alignment",
                });
            }

            Dictionary<object, List<(string, object)>> _storageParamsStoredDict = new Dictionary<object, List<(string, object)>>();
            foreach (KeyValuePair<object, IEnumerable<string>> _pairCurr in _storageParamNamesDict)
            {
                _storageParamsStoredDict.Add(
                    _pairCurr.Key,
                    _pairCurr.Value.Select(x => (x, MyUtils.GetPropertyValue(_pairCurr.Key, x))).ToList()
                    );
            }

            object res = lambda();

            if (recoverTrussImportantGeomValues != null)
            {
                this.Length = _workaroundStorage_Length.Value;
                _storageParamsStoredDict.Keys.ToList().ForEach(x => _storageParamsStoredDict[x].ForEach(y => MyUtils.SetPropertyValue(x, y.Item1, y.Item2)));

            }

            return res;
        }

        #endregion

        #region objects retrieved

        public ParametricTrussRTSamInsertMember m_ParametricTrussRTSamCurrent
        {
            get
            {
                PlanarStructure res = null;
                //res = this.GetCurrentTruss();
                if (res == null)
                {
                    res = this.MyTruss;
                }
                return res as ParametricTrussRTSamInsertMember;
            }
        }

        public ParametricTrussRTSamInsertMember m_ParametricTrussRTSamOriginal
        {
            get
            {
                PlanarStructure res = null;
                //res = this.GetCurrentTruss();
                if (res == null)
                {
                    res = this._originalMyTruss;
                }
                return res as ParametricTrussRTSamInsertMember;
            }
        }

        public ParametricTrussRTSamInsertMember m_ParametricTrussRTSamFromModelNodes
        {
            get
            {
                PlanarStructure res = this.ModelViewNodes.Where(x => x is ParametricTrussRTSamInsertMember).FirstOrDefault() as ParametricTrussRTSamInsertMember;
                //res = this.GetCurrentTruss();
                if (res == null)
                {
                    //res = this.MyTruss;
                }
                return res as ParametricTrussRTSamInsertMember;
            }
        }

        #endregion

    }
}
