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
                m0 = GetTrussMemberFromPluginInput_(_preInputs[0]);
                m1 = GetTrussMemberFromPluginInput_(_preInputs[1]);

                _model3D = new Model3D(m0, m1);
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

                    distYm0Center = m0.PartCSToGlobal.OffsetY;
                    distYm1Center = m1.PartCSToGlobal.OffsetY;
                    bool areCollinear = distYm0Center - distYm1Center == 0;
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

        private System.Windows.Window _dialog;
        private Member m0;
        private Member m1;

        private Model3D _model3D;
        private Model3DResult _model3DResult;
        private UiData _uIData;

        private double distYm0Center;
        private double distYm1Center;


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

            _uIData = new UiData(_model3D);

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
        private PlanarBeam _planarBeam;
        private SteelBeam _steelBeam;
        private ModelFolderNode _supportFolder;
     
        bool hasError = true;
        public void OnCreateButtonClicked(object sender, RoutedEventArgs args)
        {
            if (!NecessaryDataValidated())
            {
                return;
            }
            _uIData.CreateValuesFromTextBox();
            _model3DResult = new Model3DResult(_model3D, _uIData);

            if (!_uIData.IsNewBeamMultiplyed)
            {
                CreateSingleBeam();
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
            if (_uIData.BeamType.Text == Constants.TimberBeam)
            {
                _timberBeam = new TimberBeam(Constants.TimberBeam);

                _timberBeam.AssemblyName = this.AssemblyName;
                _timberBeam.FullClassName = this.FullClassName;

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

                _timberBeam.Origin = _model3DResult.DetermineBeamOrigin();

                _model3DResult.SetBeamLocationWithExtensions(_timberBeam);
            }

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

        private bool ChangeExistBeamAlignement(double angleInDegrees, Point3D startPoint3Dm0, Point3D endPoint3Dm0)
        {
            return angleInDegrees > Constants.degrees45 ? true : false;
        }

        private Vector3D? getReferenceEnforcementDirectionVector()
        {
            return new Vector3D(0, 0, 1);
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
            else
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

    }
}