using Epx.BIM;
using Epx.BIM.Models;
using Epx.BIM.Models.Steel;
using Epx.BIM.Models.Timber;
using Epx.BIM.Plugins;
using Epx.Ristek.Data.Models;
using Epx.Ristek.Data.Plugins;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RistekPluginSample
{
    public class InsertMemberPlugin : PluginTool, IModelViewPlugin, IModelDimensionController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleStructure"/> class.
        /// </summary>
        public InsertMemberPlugin()
            : base()
        {
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
        /// Custom1 is the Design View.
        /// </summary>
        public override List<PluginViewModes> AllowedViewModes => new List<PluginViewModes>() { PluginViewModes.View3D, PluginViewModes.Custom1 };

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
                // Localization can be done this way or through Resx dictionaries.
                return Strings.Strings.insertMemberBeetweenTwoExistRoofElements;
            }
        }


        /// <summary>
        /// Determines whether the specified node is a valid target node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns><c>true</c> if [is valid target] [the specified node]; otherwise, <c>false</c>.</returns>
        public override bool IsValidTarget(BaseDataNode node)
        {
            return node != null;
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
            //Input object storage method 1.
            if (_preInputs == null)
            {
                _preInputs = new List<PluginInput>();

                PluginObjectInput objectInput1 = new PluginObjectInput();
                objectInput1.Index = 0;
                objectInput1.Prompt = Strings.Strings.selectTheFirstBeam;
                _preInputs.Add(objectInput1);


                PluginObjectInput objectInput2 = new PluginObjectInput();
                objectInput2.Index = 1;
                objectInput2.Prompt = Strings.Strings.selectTheSecondBeam;
                _preInputs.Add(objectInput2);
            }
            else if (_preInputs.Count > 1 && _preInputs[0].Valid && _preInputs[1].Valid)
            {
                //m0 = GetTrussMemberFromPluginInput_(_preInputs[0]);
                //m1 = GetTrussMemberFromPluginInput_(_preInputs[1]);          

                //_model3D = new Model3D(m0, m1, isRoofYDirection);
            }

            if (previousInput == null) return _preInputs != null ? _preInputs[0] : null;
            else if (previousInput.Index + 1 < _preInputs.Count) return _preInputs[previousInput.Index + 1];
            else return null;
        }

        /// <summary>
        /// Determines whether the specified plugin input is valid.
        /// </summary>
        /// <param name="pluginInput">The plugin input.</param>
        /// <returns><c>true</c> if [is pre dialog input valid] [the specified plugin input]; otherwise, <c>false</c>.</returns>
        public override bool IsPreDialogInputValid(PluginInput pluginInput)
        {
            PluginObjectInput pluginObjectInput1 = pluginInput as PluginObjectInput;

            if (pluginInput.Index == 0 && pluginObjectInput1.Object is Member)
            {
                pluginInput.Valid = true;
                return true;
            }
            else if (pluginInput.Index == 1)
            {
                PluginObjectInput pluginObjectInput2 = pluginInput as PluginObjectInput;
                if (pluginObjectInput2 == null || !(pluginObjectInput2.Object is Member))
                {
                    pluginInput.Valid = false;
                    pluginInput.ErrorMessage = Strings.Strings.wrongInputType;
                    return false;
                }
                else
                {
                    m0 = GetTrussMemberFromPluginInput_(_preInputs[0]);
                    m1 = GetTrussMemberFromPluginInput_(_preInputs[1]);                  

                    isRoofYDirection = IsRoofYDirection(m0, m1);
                    isRoofXDirection = IsRoofXDirection(m0, m1);

                    if (isRoofYDirection)
                    {
                        areCollinear = AreCollinearAxisY(m0, m1);
                    }
                    else if (isRoofXDirection)
                    {
                        areCollinear = AreCollinearAxisX(m0, m1);
                    }

                    if ((pluginObjectInput2.Point - (_preInputs[0] as PluginObjectInput).Point).Length < 1)
                    {
                        pluginInput.Valid = false;
                        pluginInput.ErrorMessage = Strings.Strings.pointsCannotBeTheSame;
                        return false;
                    }
                    else if (areCollinear)
                    {
                        pluginInput.Valid = false;
                        pluginInput.ErrorMessage = Strings.Strings.beamsCannotBeCollinear;
                        return false;
                    }
                    else
                    {
                        pluginInput.Valid = true;
                        return true;
                    }
                }
            }
            else
            {
                pluginInput.Valid = false;
                pluginInput.ErrorMessage = Strings.Strings.wrongInputType;
                return false;
            }
        }

        private bool IsRoofYDirection(Member m0, Member m1)
        {
            double distYm0Center = m0.PartCSToGlobal.OffsetY;
            double distYm1Center = m1.PartCSToGlobal.OffsetY;

            isRoofYDirection = distYm0Center != distYm1Center;

            return isRoofYDirection;
        }

        private bool IsRoofXDirection(Member m0, Member m1)
        {
            double distXm0Center = m0.PartCSToGlobal.OffsetX;
            double distXm1Center = m1.PartCSToGlobal.OffsetX;

            isRoofXDirection = distXm0Center != distXm1Center;

            return isRoofXDirection;
        }

        private bool AreCollinearAxisX(Member m0, Member m1)
        {
            double distXm0Center = m0.PartCSToGlobal.OffsetX;
            double distXm1Center = m1.PartCSToGlobal.OffsetX;

            areCollinear = distXm0Center - distXm1Center == 0;

            return areCollinear;
        }

        private bool AreCollinearAxisY(Member m0, Member m1)
        {
            double distYm0Center = m0.PartCSToGlobal.OffsetY;
            double distYm1Center = m1.PartCSToGlobal.OffsetY;

            areCollinear = distYm0Center - distYm1Center == 0;

            return areCollinear;
        }

        private System.Windows.Window _dialog;
        private Member m0;
        private Member m1;

        private Model3D _model3D;
        private Model3DResult _model3DResult;
        private UiData _uIData;


        private bool isRoofYDirection;
        private bool isRoofXDirection;
        private bool areCollinear;


        public bool DialogResult { get; set; }
        public IPluginEngine PluginEngine { get; set; }

        private List<BaseDataNode> _modelViewNodes = new List<BaseDataNode>();

        public IEnumerable<BaseDataNode> ModelViewNodes => _modelViewNodes;

        public IEnumerable<BaseDataNode> HiddenNodes => new List<BaseDataNode>();

        private List<ModelDimensionBase> _modelViewDimensions = new List<ModelDimensionBase>();

        public IEnumerable<ModelDimensionBase> ModelViewDimensions => _modelViewDimensions;
        public IEnumerable<ModelOverlayContentElement> ModelViewOverlayContents => new List<ModelOverlayContentElement>();

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="applyEnabled">if set to <c>true</c> [apply enabled].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// 

        public override bool ShowDialog(bool applyEnabled)
        {
            _dialog = new System.Windows.Window();
            _dialog.Owner = Application.Current.MainWindow;
            _dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _dialog.SizeToContent = SizeToContent.WidthAndHeight;
            _dialog.ResizeMode = ResizeMode.NoResize;
            _dialog.Title = NameForMenu;
            _dialog.Cursor = Cursors.Arrow;



            _uIData = new UiData(m0,m1,isRoofYDirection);

            CreateFinalButtonRow(_uIData.MainStack);

            _dialog.Content = _uIData.MainStack;


            this.DialogResult = _dialog.ShowDialog().Value;
            return this.DialogResult;
        }


        private void CreateFinalButtonRow(StackPanel mainStack)
        {
            var buttonStack = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var generateButton = new Button() { Content = Strings.Strings.drawBeam, Width = 80, Margin = new Thickness(0, 0, 4, 0) };
            generateButton.Click += OnCreateButtonClicked;

            buttonStack.Children.Add(generateButton);
            mainStack.Children.Add(buttonStack);
        }

        private TimberBeam _timberBeam;
        private TimberMember3D _timberMember;
        private Batten _batten;
        private Purlin _purlin;
        private BucklingSupport _bucklingSupport;
        private StrongBack _strongBack;
        private Support _support;


        private SteelBeam _steelBeam;
        private SteelBeam3D _steelBeam3D;
        private MetalWebStructure _metalWebStructure;

        private PlanarBeam _planarBeam;

        private ModelFolderNode _supportFolder;

        bool hasError = true;
        public void OnCreateButtonClicked(object sender, RoutedEventArgs args)
        {
            if (!NecessaryDataValidated())
            {
                return;
            }
            _uIData.CreateValuesFromTextBox();

            m0.Alignment = ConvertToMemberAlignment(_uIData.ExistBeamAlignement.Text);
            m1.Alignment = ConvertToMemberAlignment(_uIData.ExistBeamAlignement.Text);
            _model3D = new Model3D(m0, m1, isRoofYDirection);           
            _model3DResult = new Model3DResult(_model3D, _uIData);

            if (!_uIData.IsNewBeamMultiplyed)
            {
                CreateSingleBeam();
            }
            else
            {
                int multipleMemberCount = (int)Math.Ceiling((_uIData.horizontalCastLength - _uIData.BeamInsertionDistanceValue) / _uIData.BeamMultiplySpacingValue);
                for (int i = 0; i < multipleMemberCount; i++)
                {
                    _uIData.BeamInsertionDistanceValue += _uIData.BeamMultiplySpacingValue;
                    CreateSingleBeam();
                }
            }

            _dialog.DialogResult = true;
        }

        private bool NecessaryDataValidated()
        {
            if (_uIData.BeamThicknessTextBox.BorderBrush == Brushes.Red || _uIData.BeamHeightTextBox.BorderBrush == Brushes.Red)
            {
                hasError = true;
            }
            else
            {
                hasError = false;
            }

            if (hasError)
            {
                MessageBox.Show(Strings.Strings.fillInRequiredFields, Strings.Strings.warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                _dialog.Activate();
                return false;
            }

            return true;
        }

        private void CreateSingleBeam()
        {
            if (_uIData.BeamType.Text == Constants.PlanarBeam)
            {
                _planarBeam = new PlanarBeam(Constants.PlanarBeam, true);
                Member member = new Member("Member");

                _planarBeam.UIName = Constants.PlanarBeam;
                _planarBeam.Alignment = _model3DResult.ConvertToModelPartAlignmentNewBeam(_uIData.NewBeamAlignement.Text);

                _planarBeam.Thickness = _uIData.BeamThicknessValue;

                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                //_planarBeam.Origin = _model3DResult.DetermineBeamOrigin();
                _planarBeam.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                member.Thickness = _planarBeam.Thickness;
                member.SetWidth(_uIData.BeamHeightValue);

                _planarBeam.MyMember.Width = _uIData.BeamHeightValue;
                _planarBeam.MyMember.SizeY = _uIData.BeamHeightValue;

                _model3DResult.SetBeamLocationWithExtensions(_planarBeam, member);
                PluginEngine?.PluginUpdate3D(true);

            }
            else if (_uIData.BeamType.Text == Constants.SteelBeam3D)
            {
                _steelBeam3D = new SteelBeam3D(Constants.SteelBeam3D);

                _steelBeam3D.AssemblyName = this.AssemblyName;
                _steelBeam3D.FullClassName = this.FullClassName;

                _steelBeam3D.Width = _uIData.BeamThicknessValue;
                _steelBeam3D.Height = _uIData.BeamHeightValue;


                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _steelBeam3D.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_steelBeam3D);
                _steelBeam3D.SectionGroupCode = "HEA";
                _steelBeam3D.SectionName = "HEA 400";
            }
            else if (_uIData.BeamType.Text == Constants.TimberBeam)
            {
                _timberBeam = new TimberBeam(Constants.TimberBeam);

                _timberBeam.AssemblyName = this.AssemblyName;
                _timberBeam.FullClassName = this.FullClassName;
                _timberBeam.Alignment = _model3DResult.ConvertToModelPartAlignmentNewBeam(_uIData.NewBeamAlignement.Text);

                _timberBeam.Width = _uIData.BeamHeightValue;
                _timberBeam.Thickness = _uIData.BeamThicknessValue;


                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _timberBeam.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_timberBeam);
            }
            else if (_uIData.BeamType.Text == Constants.BucklingSupport)
            {
                _bucklingSupport = new BucklingSupport(Constants.BucklingSupport);

                _bucklingSupport.AssemblyName = this.AssemblyName;
                _bucklingSupport.FullClassName = this.FullClassName;

                _bucklingSupport.Width = _uIData.BeamHeightValue;
                _bucklingSupport.Thickness = _uIData.BeamThicknessValue;


                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _bucklingSupport.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_bucklingSupport);
            }
            else if (_uIData.BeamType.Text == Constants.Support)
            {
                _support = new Support(Constants.Support);

                _support.AssemblyName = this.AssemblyName;
                _support.FullClassName = this.FullClassName;

                _support.Width = _uIData.BeamHeightValue;
                _support.Thickness = _uIData.BeamThicknessValue;


                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _support.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_support);
            }
            else if (_uIData.BeamType.Text == Constants.SteelBeam)
            {
                _steelBeam = new SteelBeam(Constants.SteelBeam);

                _steelBeam.AssemblyName = this.AssemblyName;
                _steelBeam.FullClassName = this.FullClassName;

                _steelBeam.Width = _uIData.BeamThicknessValue;
                _steelBeam.Height = _uIData.BeamHeightValue;


                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _steelBeam.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_steelBeam);
                _steelBeam.SectionGroupCode = "HEA";
                _steelBeam.SectionName = "HEA 400";
            }

            else if (_uIData.BeamType.Text == Constants.TimberMember)
            {
                _timberMember = new TimberMember3D(Constants.TimberMember);

                _timberMember.AssemblyName = this.AssemblyName;
                _timberMember.FullClassName = this.FullClassName;

                _timberMember.Width = _uIData.BeamHeightValue;
                _timberMember.Thickness = _uIData.BeamThicknessValue;


                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _timberMember.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_timberMember);
            }
            else if (_uIData.BeamType.Text == Constants.Batten)
            {
                _batten = new Batten(Constants.Batten);

                _batten.AssemblyName = this.AssemblyName;
                _batten.FullClassName = this.FullClassName;

                _batten.Width = _uIData.BeamThicknessValue;
                _batten.Thickness = _uIData.BeamHeightValue;

                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _batten.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_batten);
            }
            else if (_uIData.BeamType.Text == Constants.Purlin)
            {
                _purlin = new Purlin(Constants.Purlin);

                _purlin.AssemblyName = this.AssemblyName;
                _purlin.FullClassName = this.FullClassName;

                _purlin.Width = _uIData.BeamThicknessValue;
                _purlin.Thickness = _uIData.BeamHeightValue;

                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _purlin.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_purlin);
            }
            else if (_uIData.BeamType.Text == Constants.StrongBack)
            {
                _strongBack = new StrongBack(Constants.StrongBack);

                _strongBack.AssemblyName = this.AssemblyName;
                _strongBack.FullClassName = this.FullClassName;

                _strongBack.Width = _uIData.BeamThicknessValue;
                _strongBack.Thickness = _uIData.BeamHeightValue;

                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _strongBack.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);

                _model3DResult.SetBeamLocationWithExtensions(_strongBack);
            }
            else if (_uIData.BeamType.Text == Constants.MetalWebStructure)
            {
                _metalWebStructure = new MetalWebStructure(Constants.MetalWebStructure, true);
                Member member = new Member("Member");
                MetalWeb metalWeb = new MetalWeb("MetalWeb");

                _metalWebStructure.AssemblyName = this.AssemblyName;
                _metalWebStructure.FullClassName = this.FullClassName;

                _metalWebStructure.Thickness = _uIData.BeamHeightValue;
                if (_uIData.BeamDistanceType.Text == Constants.horizontalCast)
                {
                    _model3DResult.CalculateNewBeamPointsForCastDistance();
                }
                else
                {
                    _model3DResult.CalculateNewBeamPointsForDistanceAlongTheBeam();
                }

                _metalWebStructure.Origin = new Point3D(_model3DResult.xNew, _model3DResult.yNew, _model3DResult.zNew);
                member.Thickness = _metalWebStructure.Thickness;
                member.SetWidth(_uIData.BeamHeightValue);

                _metalWebStructure.AddMetalWeb(metalWeb);
                _model3DResult.SetBeamLocationWithExtensions(_metalWebStructure, member);
                PluginEngine?.PluginUpdate3D(true);
            }
        }


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
            PluginDataNode = null;
            //doDelegate = null;
            //undoDelegate = null;
            update3DNodes = new List<BaseDataNode>(0); // no need to update any model nodes

            List<BaseDataNode> addedNodes = new List<BaseDataNode>();
            if (_timberBeam != null)
            {
                addedNodes.Add(_timberBeam);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_timberBeam);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_timberBeam);
                };
            }
            else if (_support != null)
            {
                addedNodes.Add(_support);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_support);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_support);
                };
            }
            else if (_steelBeam != null)
            {
                addedNodes.Add(_steelBeam);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_steelBeam);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_steelBeam);
                };
            }
            else if (_bucklingSupport != null)
            {
                addedNodes.Add(_bucklingSupport);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_bucklingSupport);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_bucklingSupport);
                };
            }
            else if (_steelBeam3D != null)
            {
                addedNodes.Add(_steelBeam3D);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_steelBeam3D);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_steelBeam3D);
                };
            }
            else if (_timberMember != null)
            {
                addedNodes.Add(_timberMember);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_timberMember);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_timberMember);
                };
            }
            else if (_batten != null)
            {
                addedNodes.Add(_batten);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_batten);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_batten);
                };
            }
            else if (_purlin != null)
            {
                addedNodes.Add(_purlin);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_purlin);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_purlin);
                };
            }
            else if (_strongBack != null)
            {
                addedNodes.Add(_strongBack);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_strongBack);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_strongBack);
                };
            }
            else if (_metalWebStructure != null)
            {
                addedNodes.Add(_metalWebStructure);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_metalWebStructure);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_metalWebStructure);
                };
            }
            else
            {
                addedNodes.Add(_planarBeam);
                //addedNodes.Add(_supportFolder);
                if (_supportFolder != null)
                {
                    addedNodes.Add(_supportFolder);
                }

                ResetModelViewNodes();

                doDelegate = delegate
                {
                    this.Target.AddChild(_planarBeam);
                };
                undoDelegate = delegate
                {
                    this.Target.RemoveChild(_planarBeam);
                };
            }


            return addedNodes;
        }

        protected Member GetTrussMemberFromPluginInput_(PluginInput _pluginInput)
        {
            Member res = null;
            res = (_pluginInput as PluginObjectInput).Object as Member;
            return res;
        }

        public void DialogClosed(bool isCancel)
        {
            _dialog?.Close();
            _dialog = null;
        }

        public void CancelPlugin()
        {
            DialogResult = false;
            _dialog?.Close();
            ResetModelViewNodes();
        }

        private void ResetModelViewNodes()
        {
            _modelViewDimensions.Clear();
            _modelViewNodes.Clear();
        }

        public void MouseMoved(Point3D currentPoint)
        {
        }


        public void DimensionEdited(ModelDimensionLine dimensionLine, double newValue, DimensionEditType editType)
        {
            if (dimensionLine.Name == "DimensionAP")
            {
                //Edit corresponding data
            }
            PluginEngine?.PluginUpdate3D(true);
        }

        public void ContentElementEdited(ModelOverlayContentElement moce, object newValue)
        {
        }

        private Member.MemberAlignment ConvertToMemberAlignment(string alignmentOption)
        {
            string topEdgeTranslation = Strings.Strings.topEdge;
            string bottomEdgeTranslation = Strings.Strings.bottomEdge;
            string centerTranslation = Strings.Strings.center;

            if (_uIData.IsSelectedMemberLeftEdgeOnTop)
            {
                switch (alignmentOption)
                {
                    case var option when option == topEdgeTranslation:
                        return Member.MemberAlignment.LeftEdge;
                    case var option when option == bottomEdgeTranslation:
                        return Member.MemberAlignment.RightEdge;
                    case var option when option == centerTranslation:
                        return Member.MemberAlignment.Center;
                    default:
                        throw new ArgumentException(Strings.Strings.unknownAlignementOption);
                }
            }
            else
            {
                switch (alignmentOption)
                {
                    case var option when option == topEdgeTranslation:
                        return Member.MemberAlignment.RightEdge;
                    case var option when option == bottomEdgeTranslation:
                        return Member.MemberAlignment.LeftEdge;
                    case var option when option == centerTranslation:
                        return Member.MemberAlignment.Center;
                    default:
                        throw new ArgumentException(Strings.Strings.unknownAlignementOption);
                }
            }

       
        }

    }
}