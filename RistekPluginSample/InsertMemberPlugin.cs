using Epx.BIM;
using Epx.BIM.Models;
using Epx.BIM.Models.Steel;
using Epx.BIM.Models.Timber;
using Epx.BIM.Plugins;
using Epx.Ristek.Data.Models;
using Epx.Ristek.Data.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

                isLeftEdgeOnTop = m0.StartPoint.X < m0.EndPoint.X;

                if (isLeftEdgeOnTop)
                {
                    theLowestStartPointForCombinedCuttedBeam = m0.MyMemberCuts
                                          .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                                          .OrderBy(point => point.Y)
                                          .ThenBy(point => point.X)
                                          .FirstOrDefault();

                    theHighestStartPointForCombinedCuttedBeam = m0.MyMemberCuts
                        .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                       .OrderByDescending(point => point.Y)
                       .ThenByDescending(point => point.X)
                       .FirstOrDefault();
                }
                else
                {
                    theLowestStartPointForCombinedCuttedBeam = m0.MyMemberCuts
                      .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                      .OrderBy(point => point.Y)
                      .OrderByDescending(point => point.X)
                      .FirstOrDefault();

                    theHighestStartPointForCombinedCuttedBeam = m0.MyMemberCuts
                        .SelectMany(x => new[] { x.CutLine.EndPoint, x.CutLine.StartPoint })
                       .OrderByDescending(point => point.Y)
                       .ThenBy(point => point.X)
                       .FirstOrDefault();
                }




                if (Math.Round(m0.LeftGeometryEdgeLine.StartPoint.X, 2) == Math.Round(m0.RightGeometryEdgeLine.StartPoint.X, 2))
                {
                    isVerticalEaves = true;
                }
                else if (m0.LeftGeometryEdgeLine.StartPoint.Y == m0.RightGeometryEdgeLine.StartPoint.Y)
                {
                    isHorizontalEaves = true;
                }
                else if (m0.Geometry.Count > 5)
                {
                    isCombinedEaves = true;
                }
                else
                {
                    isNormalEaves = true;
                }

                if (isCombinedEaves)
                {

                    if (isLeftEdgeOnTop)
                    {
                        horizontalMove1LowestPoint = Math.Abs(m0.RightGeometryEdgeLine.StartPoint.X - theLowestStartPointForCombinedCuttedBeam.X);
                        verticalMove1LowestPoint = Math.Abs(m0.RightGeometryEdgeLine.StartPoint.Y - theLowestStartPointForCombinedCuttedBeam.Y);
                        horizontalMove1HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.X - m0.LeftGeometryEdgeLine.EndPoint.X);
                        verticalMove1HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.Y - m0.LeftGeometryEdgeLine.EndPoint.Y);

                        horizontalMove2LowestPoint = Math.Abs(m0.LeftGeometryEdgeLine.StartPoint.X - theLowestStartPointForCombinedCuttedBeam.X);
                        verticalMove2LowestPoint = Math.Abs(m0.LeftGeometryEdgeLine.StartPoint.Y - theLowestStartPointForCombinedCuttedBeam.Y);
                        horizontalMove2HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.X - m0.RightGeometryEdgeLine.EndPoint.X);
                        verticalMove2HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.Y - m0.RightGeometryEdgeLine.EndPoint.Y);
                    }
                    else
                    {
                        horizontalMove1LowestPoint = Math.Abs(m0.LeftGeometryEdgeLine.StartPoint.X - theLowestStartPointForCombinedCuttedBeam.X);
                        verticalMove1LowestPoint = Math.Abs(m0.LeftGeometryEdgeLine.StartPoint.Y - theLowestStartPointForCombinedCuttedBeam.Y);
                        horizontalMove1HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.X - m0.RightGeometryEdgeLine.EndPoint.X);
                        verticalMove1HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.Y - m0.RightGeometryEdgeLine.EndPoint.Y);

                        horizontalMove2LowestPoint = Math.Abs(m0.RightGeometryEdgeLine.StartPoint.X - theLowestStartPointForCombinedCuttedBeam.X);
                        verticalMove2LowestPoint = Math.Abs(m0.RightGeometryEdgeLine.StartPoint.Y - theLowestStartPointForCombinedCuttedBeam.Y);
                        horizontalMove2HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.X - m0.LeftGeometryEdgeLine.EndPoint.X);
                        verticalMove2HighestPoint = Math.Abs(theHighestStartPointForCombinedCuttedBeam.Y - m0.LeftGeometryEdgeLine.EndPoint.Y);
                    }

                }


                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                {
                    m0XStart = Math.Round(m0.LeftGeometryEdgeLine.StartPoint.X, 0);
                    m0YStart = Math.Round(m0.LeftGeometryEdgeLine.StartPoint.Y, 0);
                    m0XEnd = Math.Round(m0.LeftGeometryEdgeLine.EndPoint.X, 0);
                    m0YEnd = Math.Round(m0.LeftGeometryEdgeLine.EndPoint.Y, 0);

                    m1XStart = Math.Round(m1.LeftGeometryEdgeLine.StartPoint.X, 0);
                    m1YStart = Math.Round(m1.LeftGeometryEdgeLine.StartPoint.Y, 0);
                    m1XEnd = Math.Round(m1.LeftGeometryEdgeLine.EndPoint.X, 0);
                    m1YEnd = Math.Round(m1.LeftGeometryEdgeLine.EndPoint.Y, 0);

                    horizontalCastLength = Math.Abs(m0.LeftGeometryEdgeLine.EndPoint.X - m0.LeftGeometryEdgeLine.StartPoint.X);
                }
                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                {
                    m0XStart = Math.Round(m0.RightGeometryEdgeLine.StartPoint.X, 0);
                    m0YStart = Math.Round(m0.RightGeometryEdgeLine.StartPoint.Y, 0);
                    m0XEnd = Math.Round(m0.RightGeometryEdgeLine.EndPoint.X, 0);
                    m0YEnd = Math.Round(m0.RightGeometryEdgeLine.EndPoint.Y, 0);

                    m1XStart = Math.Round(m1.RightGeometryEdgeLine.StartPoint.X, 0);
                    m1YStart = Math.Round(m1.RightGeometryEdgeLine.StartPoint.Y, 0);
                    m1XEnd = Math.Round(m1.RightGeometryEdgeLine.EndPoint.X, 0);
                    m1YEnd = Math.Round(m1.RightGeometryEdgeLine.EndPoint.Y, 0);

                    horizontalCastLength = Math.Abs(m0.RightGeometryEdgeLine.EndPoint.X - m0.RightGeometryEdgeLine.StartPoint.X);
                }
                else
                {
                    if (isCombinedEaves)
                    {
                        m0XStart = Math.Round(theLowestStartPointForCombinedCuttedBeam.X, 0);
                        m0YStart = Math.Round(theLowestStartPointForCombinedCuttedBeam.Y, 0);
                        m0XEnd = Math.Round(theHighestStartPointForCombinedCuttedBeam.X, 0);
                        m0YEnd = Math.Round(theHighestStartPointForCombinedCuttedBeam.Y, 0);

                        m1XStart = Math.Round(theLowestStartPointForCombinedCuttedBeam.X, 0);
                        m1YStart = Math.Round(theLowestStartPointForCombinedCuttedBeam.Y, 0);
                        m1XEnd = Math.Round(theHighestStartPointForCombinedCuttedBeam.X, 0);
                        m1YEnd = Math.Round(theHighestStartPointForCombinedCuttedBeam.Y, 0);
                    }
                    else
                    {
                        m0XStart = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.StartPoint.X, 0);
                        m0YStart = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.StartPoint.Y, 0);
                        m0XEnd = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.EndPoint.X, 0);
                        m0YEnd = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.EndPoint.Y, 0);

                        m1XStart = Math.Round(m1.MiddleCuttedLineWithBooleanCuts.StartPoint.X, 0);
                        m1YStart = Math.Round(m1.MiddleCuttedLineWithBooleanCuts.StartPoint.Y, 0);
                        m1XEnd = Math.Round(m1.MiddleCuttedLineWithBooleanCuts.EndPoint.X, 0);
                        m1YEnd = Math.Round(m1.MiddleCuttedLineWithBooleanCuts.EndPoint.Y, 0);
                    }

                    horizontalCastLength = Math.Abs(m0.MiddleCuttedLine.EndPoint.X - m0.MiddleCuttedLine.StartPoint.X);
                }

                if (isLeftEdgeOnTop)
                {
                    startPoint3Dm0 = new Point3D(m0.RightGeometryEdgeLine.StartPoint.X, distYm0Center, m0.RightGeometryEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.RightGeometryEdgeLine.EndPoint.X, distYm0Center, m0.RightGeometryEdgeLine.EndPoint.Y);
                }
                else
                {
                    startPoint3Dm0 = new Point3D(m0.LeftGeometryEdgeLine.StartPoint.X, distYm0Center, m0.LeftGeometryEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.LeftGeometryEdgeLine.EndPoint.X, distYm0Center, m0.LeftGeometryEdgeLine.EndPoint.Y);
                }

                double deltaX = endPoint3Dm0.X - startPoint3Dm0.X;
                double deltaY = endPoint3Dm0.Y - startPoint3Dm0.Y;
                double deltaZ = endPoint3Dm0.Z - startPoint3Dm0.Z;

                double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
                alongBeamLength = deltaTotal;

                existBeamLength = m0.Length;
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
        private TextBox _beamHeight;
        private double beamHeight;
        private TextBox _beamThickness;
        private double beamThickness;
        private double m0XStart;
        private double m0YStart;
        private double m1XStart;
        private double m1YStart;
        private double m0XEnd;
        private double m0YEnd;
        private double m1XEnd;
        private double m1YEnd;
        private TextBox _beamHorizontalInsertionDistance = new TextBox();
        private double beamHorizontalInsertionDistance;
        private TextBox _beamStartExtension;
        private double beamStartExtension;
        private TextBox _beamEndExtension;
        private double beamEndExtension;
        private TextBox _beamMultiplySpacing;
        private double beamMultiplySpacing;
        private double existBeamLength;
        private ComboBox _comboBoxNewBeamAlignement;
        private ComboBox _comboBoxExistBeamAlignement;
        private bool IsRotatedToTheMainTruss;
        private bool IsNewBeamMultiplyed;
        private Member m0;
        private Member m1;
        private double horizontalCastLength;
        private double alongBeamLength;
        private ComboBox _selectionOfBeamType;
        private ComboBox _selectionOfBeamDistanceType;
        private CheckBox beamRotationCheckBox;
        private bool castToTheHorizontalDistance;

        private bool isVerticalEaves;
        private bool isHorizontalEaves;
        private bool isNormalEaves;
        private bool isCombinedEaves;
        private double distYm0Center;
        private double distYm1Center;
        private bool isLeftEdgeOnTop;


        double verticalEavesZMove;
        double horizontalEavesXMove;

        double normalEavesXMove;
        double normalEavesZMove;

        double xMoveForNewBeam;
        double zMoveForNewBeam;
        double verticalMoveForNewBeam;

        Point theLowestStartPointForCombinedCuttedBeam;
        Point theHighestStartPointForCombinedCuttedBeam;

        double horizontalMove1LowestPoint;
        double verticalMove1LowestPoint;
        double horizontalMove2LowestPoint;
        double verticalMove2LowestPoint;

        double horizontalMove1HighestPoint;
        double verticalMove1HighestPoint;
        double horizontalMove2HighestPoint;
        double verticalMove2HighestPoint;

        double xNew;
        double yNew;
        double zNew;

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

            var mainStack = new StackPanel() { Name = Constants.MainStack, Margin = new Thickness(8) };
            mainStack.Orientation = Orientation.Vertical;

            CreateChapterTitleRow(mainStack, Constants.TitleChapter0SelectedBeamInformation);
            CreateSelectedBeamCoordinatesInfoRow(mainStack);
            CreateSelectedBeamLengthInfoRow(mainStack);
            CreateChapterTitleRow(mainStack, Constants.TitleChapter1BeamSettings);
            CreateSelectionOfBeamType(mainStack);
            CreateBeamDimensionsRow(mainStack);
            CreateNewBeamAlignementOptionRow(mainStack);
            CreateExistBeamAlignementOptionRow(mainStack);
            CreateBeamExtensionRow(mainStack);
            CreateBeamRotationRow(mainStack);
            CreateChapterTitleRow(mainStack, Constants.TitleChapter2BeamLocation);
            CreateBeamLocationRow(mainStack);
            CreateBeamMultiplicationRow(mainStack);


            CreateFinalButtonRow(mainStack);

            _dialog.Content = mainStack;

#if DEBUG
            _beamHeight.Text = 100.ToString();
            _beamThickness.Text = 40.ToString();
            _beamStartExtension.Text = 42.ToString();
            _beamEndExtension.Text = 42.ToString();
            _comboBoxNewBeamAlignement.Text = "Upper plane";
            _comboBoxExistBeamAlignement.Text = "Upper plane";
            beamRotationCheckBox.IsChecked = true;
            _selectionOfBeamDistanceType.Text = "Along beam";
            _beamHorizontalInsertionDistance.Text = 0.ToString();

#endif

            this.DialogResult = _dialog.ShowDialog().Value;
            return this.DialogResult;
        }

        private void CreateSelectedBeamLengthInfoRow(StackPanel mainStack)
        {
            var selectedBeamLengthStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            var selectedBeamLengthStackText = new TextBlock() { Text = Strings.Strings.beamLength, Margin = new Thickness(0, 0, 8, 0) };
            var selectedBeamLengthStackValue = new TextBlock() { Text = Math.Round(existBeamLength, 0).ToString() + Constants.mm, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeamLengthStack.Children.Add(selectedBeamLengthStackText);
            selectedBeamLengthStack.Children.Add(selectedBeamLengthStackValue);

            mainStack.Children.Add(selectedBeamLengthStack);
        }

        private void CreateSelectionOfBeamType(StackPanel mainStack)
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = Strings.Strings.selectionOfBeamType, Margin = new Thickness(0, 0, 8, 0) };
            comboBoxStack.Children.Add(comboBoxText);
            _selectionOfBeamType = new ComboBox() { Width = Double.NaN, Margin = new Thickness(0, 0, 4, 0) };
            _selectionOfBeamType.Items.Add(Strings.Strings.timberBeam);
            _selectionOfBeamType.Items.Add(Strings.Strings.steelBeam);
            _selectionOfBeamType.SelectedIndex = 0;
            comboBoxStack.Children.Add(_selectionOfBeamType);

            mainStack.Children.Add(comboBoxStack);
        }

        private void CreateBeamRotationRow(StackPanel mainStack)
        {
            var beamStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamText = new TextBlock() { Text = Strings.Strings.setRotationRelativeToTheRoofSlope, Margin = new Thickness(0, 0, 10, 0) };
            beamStack.Children.Add(beamText);

            beamRotationCheckBox = new CheckBox()
            {
                Content = Strings.Strings.rotateBeam,
                Margin = new Thickness(0, 0, 4, 0)
            };
            beamRotationCheckBox.Checked += (s, e) => IsRotatedToTheMainTruss = true;
            beamRotationCheckBox.Unchecked += (s, e) => IsRotatedToTheMainTruss = false;

            beamStack.Children.Add(beamRotationCheckBox);

            mainStack.Children.Add(beamStack);
        }

        private void CreateChapterTitleRow(StackPanel mainStack, string chapterTitle)
        {
            var chapetrTitleStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var chapetrTitleStackText = new TextBlock()
            {
                Text = chapterTitle,
                Margin = new Thickness(0, 0, 4, 0),
                FontWeight = FontWeights.Bold
            };
            chapetrTitleStack.Children.Add(chapetrTitleStackText);
            mainStack.Children.Add(chapetrTitleStack);
        }

        private void CreateFinalButtonRow(StackPanel mainStack)
        {
            var buttonStack = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var generateButton = new Button() { Content = Strings.Strings.drawBeam, Width = 80, Margin = new Thickness(0, 0, 4, 0) };
            generateButton.Click += OnCreateButtonClicked;

            buttonStack.Children.Add(generateButton);
            mainStack.Children.Add(buttonStack);
        }

        private void CreateSelectedBeamCoordinatesInfoRow(StackPanel mainStack)
        {
            var selectedBeam0CoordinatesStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            var selectedBeamCoordinatesForFirstBeamStackText = new TextBlock() { Text = Strings.Strings.firstSelectedBeamCoordinates, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(selectedBeamCoordinatesForFirstBeamStackText);

            var startPoint0StackText = new TextBlock() { Text = Strings.Strings.startPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam0CoordinatesStack.Children.Add(startPoint0StackText);

            var beam0StartXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartXStackText);
            var beam0StartXValueStackText = new TextBlock() { Text = m0XStart.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartXValueStackText);
            var beam0StartYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartYStackText);
            var beam0StartYValueStackText = new TextBlock() { Text = m0YStart.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartYValueStackText);

            var endPoint0StackText = new TextBlock() { Text = Strings.Strings.endPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam0CoordinatesStack.Children.Add(endPoint0StackText);

            var beam0SEndXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0SEndXStackText);
            var beam0EndXValueStackText = new TextBlock() { Text = m0XEnd.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0EndXValueStackText);
            var beam0EndYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0EndYStackText);
            var beam0EndYValueStackText = new TextBlock() { Text = m0YEnd.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0EndYValueStackText);
            mainStack.Children.Add(selectedBeam0CoordinatesStack);

            var selectedBeam1CoordinatesStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            var selectedBeamCoordinatesForSecondBeamStackText = new TextBlock() { Text = Strings.Strings.secondSelectedBeamCoordinates, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(selectedBeamCoordinatesForSecondBeamStackText);

            var startPoint1StackText = new TextBlock() { Text = Strings.Strings.startPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam1CoordinatesStack.Children.Add(startPoint1StackText);

            var beam1StartXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartXStackText);
            var beam1StartXValueStackText = new TextBlock() { Text = m1XStart.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartXValueStackText);
            var beam1StartYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartYStackText);
            var beam1StartYValueStackText = new TextBlock() { Text = m1YStart.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartYValueStackText);

            var endPoint1StackText = new TextBlock() { Text = Strings.Strings.endPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam1CoordinatesStack.Children.Add(endPoint1StackText);

            var beam1SEndXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1SEndXStackText);
            var beam1EndXValueStackText = new TextBlock() { Text = m1XEnd.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1EndXValueStackText);
            var beam1EndYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1EndYStackText);
            var beam1EndYValueStackText = new TextBlock() { Text = m1YEnd.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1EndYValueStackText);

            mainStack.Children.Add(selectedBeam1CoordinatesStack);
        }

        private void CreateBeamDimensionsRow(StackPanel mainStack)
        {
            var thicknessHeightStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var thicknessHeightStackText = new TextBlock() { Text = Strings.Strings.setBeamDimensions, Margin = new Thickness(0, 0, 8, 0) };
            thicknessHeightStack.Children.Add(thicknessHeightStackText);
            string text = Strings.Strings.horizontalDimension;
            double textLength = text.Length * 6.5;

            _beamThickness = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            _beamThickness.BorderBrush = Brushes.Red;
            HandleNumericTextBox(_beamThickness);
            _beamThickness.TextChanged += BeamDimension_TextChanged;
            thicknessHeightStack.Children.Add(_beamThickness);

            text = Strings.Strings.verticalDimension;
            textLength = text.Length * 6.5;

            _beamHeight = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(_beamHeight);
            _beamHeight.BorderBrush = Brushes.Red;

            _beamHeight.TextChanged += BeamDimension_TextChanged;
            thicknessHeightStack.Children.Add(_beamHeight);



            mainStack.Children.Add(thicknessHeightStack);
        }

        private void BeamDimension_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text) ||
                    !double.TryParse(textBox.Text, out double value) || value <= 0)
                {
                    textBox.BorderBrush = Brushes.Red;
                }
                else
                {
                    textBox.BorderBrush = Brushes.Gray;
                }
            }
        }

        private void SomeMethodThatUsesDimensions()
        {
            if (!double.TryParse(_beamThickness.Text, out double thickness) || thickness <= 0)
            {
                MessageBox.Show(Strings.Strings.setBeamDimensions + " - " + Strings.Strings.horizontalDimension + Strings.Strings.haveToBeLargerThanZero, Strings.Strings.validationIssue, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(_beamHeight.Text, out double height) || height <= 0)
            {
                MessageBox.Show(Strings.Strings.setBeamDimensions + " - " + Strings.Strings.verticalDimension + Strings.Strings.haveToBeLargerThanZero, Strings.Strings.validationIssue, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void CreateBeamLocationRow(StackPanel mainStack)
        {
            var beamLocationStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamLocationStackStackText = new TextBlock()
            {
                Text = Strings.Strings.setHorizontalInsertionFromReferencePart1 + Constants.goToNewLine + Strings.Strings.setHorizontalInsertionFromReferencePart2,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            beamLocationStack.Children.Add(beamLocationStackStackText);
            _selectionOfBeamDistanceType = new ComboBox() { Margin = new Thickness(0, 0, 4, 0) };
            _selectionOfBeamDistanceType.Items.Add(Strings.Strings.alongBeam);
            _selectionOfBeamDistanceType.Items.Add(Strings.Strings.horizontalCast);
            _selectionOfBeamDistanceType.SelectedIndex = 0;
            _selectionOfBeamDistanceType.SelectionChanged += SelectionChangedBeamDistanceTypeHandler;
            int distanceTypeLength = 0;
            foreach (var item in _selectionOfBeamDistanceType.Items)
            {
                int currentItemLength = item.ToString().Length;
                if (distanceTypeLength < currentItemLength)
                {
                    distanceTypeLength = currentItemLength;
                }
            }
            _selectionOfBeamDistanceType.Width = distanceTypeLength * Constants.multiplyerForLettersWidth8;
            _selectionOfBeamDistanceType.VerticalAlignment = VerticalAlignment.Center;
            beamLocationStack.Children.Add(_selectionOfBeamDistanceType);
            castToTheHorizontalDistance = _selectionOfBeamDistanceType.Text == Constants.horizontalCast;

            string text = Strings.Strings.distance;
            double textLength = text.Length * Constants.multiplyerForLettersWidth8;
            _beamHorizontalInsertionDistance = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 0, 0), text);
            _beamHorizontalInsertionDistance.VerticalAlignment = VerticalAlignment.Center;

            _beamHorizontalInsertionDistance.PreviewTextInput += TextBox_PreviewTextInput;
            _beamHorizontalInsertionDistance.TextChanged += BeamHorizontalInsertionDistance_TextChanged;

            HandleNumericTextBox(_beamHorizontalInsertionDistance);
            beamLocationStack.Children.Add(_beamHorizontalInsertionDistance);

            mainStack.Children.Add(beamLocationStack);
        }

        private void SelectionChangedBeamDistanceTypeHandler(object sender, SelectionChangedEventArgs e)
        {
            string selectedText = (sender as ComboBox).SelectedItem.ToString();

            castToTheHorizontalDistance = selectedText == Constants.horizontalCast;

            BeamHorizontalInsertionDistance_TextChanged(_beamHorizontalInsertionDistance, null);
        }

        private void SelectionChangedExistBeamAlignementHandler(object sender, SelectionChangedEventArgs e)
        {
            string selectedText = (sender as ComboBox).SelectedItem.ToString();

            Member.MemberAlignment existBeamAlignment = GetBeamAlignment(selectedText, Strings.Strings.newBeamAlignement);

            SetMaxBeamLengthForExpectedAlignement(existBeamAlignment);
        }

        private void SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment existBeamAlignment)
        {
            double m0XStart;
            double m0YStart;
            double m0XEnd;
            double m0YEnd;

            if (existBeamAlignment.ToString() == Constants.RightEdge)
            {
                m0XStart = Math.Round(m0.RightGeometryEdgeLine.StartPoint.X, 0);
                m0YStart = Math.Round(m0.RightGeometryEdgeLine.StartPoint.Y, 0);
                m0XEnd = Math.Round(m0.RightGeometryEdgeLine.EndPoint.X, 0);
                m0YEnd = Math.Round(m0.RightGeometryEdgeLine.EndPoint.Y, 0);
            }
            else if (existBeamAlignment.ToString() == Constants.LeftEdge)
            {
                m0XStart = Math.Round(m0.LeftGeometryEdgeLine.StartPoint.X, 0);
                m0YStart = Math.Round(m0.LeftGeometryEdgeLine.StartPoint.Y, 0);
                m0XEnd = Math.Round(m0.LeftGeometryEdgeLine.EndPoint.X, 0);
                m0YEnd = Math.Round(m0.LeftGeometryEdgeLine.EndPoint.Y, 0);
            }
            else
            {
                m0XStart = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.StartPoint.X, 0);
                m0YStart = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.StartPoint.Y, 0);
                m0XEnd = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.EndPoint.X, 0);
                m0YEnd = Math.Round(m0.MiddleCuttedLineWithBooleanCuts.EndPoint.Y, 0);
            }


            startPoint3Dm0 = new Point3D(m0XStart, distYm0Center, m0YStart);
            endPoint3Dm0 = new Point3D(m0XEnd, distYm0Center, m0YEnd);
            double deltaX = endPoint3Dm0.X - startPoint3Dm0.X;
            double deltaY = endPoint3Dm0.Y - startPoint3Dm0.Y;
            double deltaZ = endPoint3Dm0.Z - startPoint3Dm0.Z;

            double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
            alongBeamLength = deltaTotal;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void BeamHorizontalInsertionDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            double beamMaxLength = castToTheHorizontalDistance ? horizontalCastLength : alongBeamLength;

            TextBox textBox = sender as TextBox;
            if (double.TryParse(textBox.Text, out double value))
            {
                if (value > beamMaxLength)
                {
                    textBox.Text = Math.Round(beamMaxLength, 0).ToString();
                }
                else if (value < 0)
                {
                    textBox.Text = Constants.zeroString;
                }

                textBox.CaretIndex = textBox.Text.Length;
            }
        }


        private void CreateBeamMultiplicationRow(StackPanel mainStack)
        {
            var beamStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamText = new TextBlock() { Text = Strings.Strings.setBeamMultiplication, Margin = new Thickness(0, 0, 10, 0) };
            beamStack.Children.Add(beamText);

            var beamMultiplyCheckBox = new CheckBox()
            {
                Content = Strings.Strings.multiplyBeamWithSpacing,
                Margin = new Thickness(0, 0, 4, 0)
            };
            beamMultiplyCheckBox.Checked += (s, e) => IsNewBeamMultiplyed = true;
            beamMultiplyCheckBox.Unchecked += (s, e) => IsNewBeamMultiplyed = false;
            beamStack.Children.Add(beamMultiplyCheckBox);

            _beamMultiplySpacing = new TextBox()
            {
                Width = 50,
                Margin = new Thickness(4, 0, 4, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var spacingLabel = new TextBlock()
            {
                Text = Constants.mm,
                Margin = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            beamStack.Children.Add(_beamMultiplySpacing);
            beamStack.Children.Add(spacingLabel);

            mainStack.Children.Add(beamStack);
        }

        private void CreateNewBeamAlignementOptionRow(StackPanel mainStack)
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = Strings.Strings.alignementForNewBeam, Margin = new Thickness(0, 0, 8, 0) };
            comboBoxStack.Children.Add(comboBoxText);
            _comboBoxNewBeamAlignement = new ComboBox() { Width = Double.NaN, Margin = new Thickness(0, 0, 4, 0) };
            _comboBoxNewBeamAlignement.Items.Add(Strings.Strings.topEdge);
            _comboBoxNewBeamAlignement.Items.Add(Strings.Strings.bottomEdge);
            _comboBoxNewBeamAlignement.Items.Add(Strings.Strings.center);
            _comboBoxNewBeamAlignement.SelectedIndex = 0;
            comboBoxStack.Children.Add(_comboBoxNewBeamAlignement);

            mainStack.Children.Add(comboBoxStack);
        }

        private void CreateExistBeamAlignementOptionRow(StackPanel mainStack)
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = Strings.Strings.alignementForExistBeam, Margin = new Thickness(0, 0, 8, 0) };
            comboBoxStack.Children.Add(comboBoxText);
            _comboBoxExistBeamAlignement = new ComboBox() { Width = Double.NaN, Margin = new Thickness(0, 0, 4, 0) };
            _comboBoxExistBeamAlignement.Items.Add(Strings.Strings.topEdge);
            _comboBoxExistBeamAlignement.Items.Add(Strings.Strings.bottomEdge);
            _comboBoxExistBeamAlignement.Items.Add(Strings.Strings.center);
            _comboBoxExistBeamAlignement.SelectedIndex = 0;
            _comboBoxExistBeamAlignement.SelectionChanged += SelectionChangedExistBeamAlignementHandler;
            _beamHorizontalInsertionDistance.TextChanged += BeamHorizontalInsertionDistance_TextChanged;//?????????????????????????????????????????????

            comboBoxStack.Children.Add(_comboBoxExistBeamAlignement);

            mainStack.Children.Add(comboBoxStack);
        }

        private void CreateBeamExtensionRow(StackPanel mainStack)
        {
            var beamExtensionStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamExtensionText = new TextBlock() { Text = Strings.Strings.setBeamExtension, Margin = new Thickness(0, 0, 8, 0) };
            beamExtensionStack.Children.Add(beamExtensionText);

            string text = Strings.Strings.beamStartExtension;
            double textLength = text.Length * 6.5;
            _beamStartExtension = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(_beamStartExtension);
            beamExtensionStack.Children.Add(_beamStartExtension);

            text = Strings.Strings.beamEndExtension;
            textLength = text.Length * 6.5;
            _beamEndExtension = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(_beamEndExtension);
            beamExtensionStack.Children.Add(_beamEndExtension);

            mainStack.Children.Add(beamExtensionStack);
        }

        private void HandleNumericTextBox(TextBox textBox)
        {
            textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(textBox, NumericTextBox_Pasting);
        }

        private TextBox CreateTextBox(string name, double width, Thickness margin, string textBoxText)
        {
            var textBox = new TextBox();
            textBox = new TextBox() { Name = name, Width = width, Margin = margin };
            textBox.Text = textBoxText;
            textBox.Loaded += (s, o) => { Keyboard.Focus(textBox); };

            return textBox;
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextNumeric(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsTextNumeric(string text)
        {
            Regex regex = new Regex(Constants.RegexForNumbersOnly);
            return !regex.IsMatch(text);
        }

        private TimberBeam _timberBeam;
        private PlanarBeam _planarBeam;
        private SteelBeam _steelBeam;
        private ModelFolderNode _supportFolder;
        Point3D startPoint3Dm0, endPoint3Dm0, startPoint3Dm1, endPoint3Dm1;
        Point3D newStartPoint3D, newEndPoint3D;
        bool hasError = true;
        bool changeExistBeamAlignement;
        private void OnCreateButtonClicked(object sender, RoutedEventArgs args)
        {
            if (!NecessaryDataValidated())
            {
                return;
            }

            beamHeight = ParseDoubleFromText(_beamHeight.Text, Strings.Strings.beamHeight); ;
            beamThickness = ParseDoubleFromText(_beamThickness.Text, Strings.Strings.beamThickness);
            beamHorizontalInsertionDistance = ParseDoubleFromText(_beamHorizontalInsertionDistance.Text, Strings.Strings.beamHorizontalInsertionDistance);
            beamStartExtension = ParseDoubleFromText(_beamStartExtension.Text, Strings.Strings.beamStartExtension);
            beamEndExtension = ParseDoubleFromText(_beamEndExtension.Text, Strings.Strings.beamEndExtension);

            if (!IsNewBeamMultiplyed)
            {
                CreateSingleBeam();
            }
            else
            {
                beamMultiplySpacing = ParseDoubleFromText(_beamMultiplySpacing.Text, Strings.Strings.beamMultiplySpacing);

                startPoint3Dm0 = new Point3D(m0.AlignedStartPoint.X, distYm0Center, m0.AlignedStartPoint.Y);
                endPoint3Dm0 = new Point3D(m0.AlignedEndPoint.X, distYm0Center, m0.AlignedEndPoint.Y);
                int multipleMemberCount = (int)Math.Ceiling((horizontalCastLength - beamHorizontalInsertionDistance) / beamMultiplySpacing);
                for (int i = 0; i < multipleMemberCount; i++)
                {
                    beamHorizontalInsertionDistance += beamMultiplySpacing;
                    CreateSingleBeam();
                }
            }
            _dialog.DialogResult = true;
        }

        private bool NecessaryDataValidated()
        {
            if (_beamThickness.BorderBrush == Brushes.Red || _beamHeight.BorderBrush == Brushes.Red)
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
            if (_selectionOfBeamType.Text == Constants.TimberBeam)
            {
                _timberBeam = new TimberBeam(Constants.TimberBeam);

                _timberBeam.AssemblyName = this.AssemblyName;
                _timberBeam.FullClassName = this.FullClassName;

                _timberBeam.Width = beamHeight;
                _timberBeam.Thickness = beamThickness;

                CalculateExistBeamPoints(out startPoint3Dm0, out endPoint3Dm0, out startPoint3Dm1, out endPoint3Dm1);
                double deltaX = Math.Abs(endPoint3Dm0.X - startPoint3Dm0.X);
                double deltaZ = Math.Abs(endPoint3Dm0.Z - startPoint3Dm0.Z);
                double slopeAngleInRadians = Math.Atan2(deltaZ, deltaX);
                double slopeAngleInDegrees = slopeAngleInRadians * 180 / Math.PI;

                double angle90MinusRoofSlopeDegrees = Constants.degrees90 - slopeAngleInDegrees;
                double angle90MinusRoofSlopeRadians = angle90MinusRoofSlopeDegrees * Math.PI / 180;

                double cosOfRoofSlope = CalculateCosAngle(slopeAngleInRadians);
                double sinOfRoofSlope = CalculateSinAngle(slopeAngleInRadians);
                double cosOfRoofSlopeAngleNotCasted = CalculateCosAngle(angle90MinusRoofSlopeRadians);
                double sinOfRoofSlopeAngleNotCasted = CalculateSinAngle(angle90MinusRoofSlopeRadians);

                if (_selectionOfBeamDistanceType.Text == Constants.horizontalCast)
                {
                    CalculateNewBeamPointsForCastDistance(startPoint3Dm0, endPoint3Dm0, beamHorizontalInsertionDistance, out newStartPoint3D, out newEndPoint3D);
                }
                else
                {
                    CalculateNewBeamPointsForDistanceAlongTheBeam(startPoint3Dm0, endPoint3Dm0, beamHorizontalInsertionDistance, out newStartPoint3D, out newEndPoint3D);
                }

                xNew = newStartPoint3D.X;
                yNew = newStartPoint3D.Y;
                zNew = newStartPoint3D.Z;

                verticalMoveForNewBeam = beamHeight / 2 / cosOfRoofSlope;
                verticalEavesZMove = m0.Width / cosOfRoofSlope;
                horizontalEavesXMove = m0.Width / 2 / sinOfRoofSlope;

                normalEavesXMove = m0.Width / 2 * cosOfRoofSlopeAngleNotCasted;
                normalEavesZMove = m0.Width / 2 * sinOfRoofSlopeAngleNotCasted;

                xMoveForNewBeam = beamHeight / 2 * cosOfRoofSlopeAngleNotCasted;
                zMoveForNewBeam = beamHeight / 2 * sinOfRoofSlopeAngleNotCasted;

                bool isNotSquareCrossSection = beamThickness != beamHeight;

                Member.MemberAlignment newBeamAlignment = GetBeamAlignment(_comboBoxNewBeamAlignement.SelectedItem.ToString(), Strings.Strings.newBeamAlignement);
                Member.MemberAlignment existBeamAlignment = GetBeamAlignment(_comboBoxExistBeamAlignement.SelectedItem.ToString(), Strings.Strings.existBeamAlignement);

                _timberBeam.Origin = DetermineBeamOrigin(newBeamAlignment, existBeamAlignment);

                SetBeamLocationWithExtensions(_timberBeam, beamStartExtension, beamEndExtension);
            }
            else if (_selectionOfBeamType.Text == Constants.SteelBeam)
            {
                _steelBeam = new SteelBeam(Constants.SteelBeam);

                _steelBeam.AssemblyName = this.AssemblyName;
                _steelBeam.FullClassName = this.FullClassName;

                _steelBeam.Width = beamThickness;
                _steelBeam.Height = beamHeight;

                CalculateExistBeamPoints(out startPoint3Dm0, out endPoint3Dm0, out startPoint3Dm1, out endPoint3Dm1);
                double deltaX = Math.Abs(endPoint3Dm0.X - startPoint3Dm0.X);
                double deltaZ = Math.Abs(endPoint3Dm0.Z - startPoint3Dm0.Z);
                double slopeAngleInRadians = Math.Atan2(deltaZ, deltaX);
                double slopeAngleInDegrees = slopeAngleInRadians * 180 / Math.PI;

                double angleInRadians = Math.Atan2(deltaZ, deltaX);
                double cosOfRoofSlopeAngle = CalculateCosAngle(angleInRadians);
                double tgOfRoofSlopeAngle = CalculateTgAngle(angleInRadians);

                double angle90MinusRoofSlopeDegrees = Constants.degrees90 - slopeAngleInDegrees;
                double angle90MinusRoofSlopeRadians = angle90MinusRoofSlopeDegrees * Math.PI / 180;

                double cosOfRoofSlope = CalculateCosAngle(slopeAngleInRadians);
                double sinOfRoofSlope = CalculateSinAngle(slopeAngleInRadians);
                double cosOfRoofSlopeAngleNotCasted = CalculateCosAngle(angle90MinusRoofSlopeRadians);
                double sinOfRoofSlopeAngleNotCasted = CalculateSinAngle(angle90MinusRoofSlopeRadians);

                if (_selectionOfBeamDistanceType.Text == Constants.horizontalCast)
                {
                    CalculateNewBeamPointsForCastDistance(startPoint3Dm0, endPoint3Dm0, beamHorizontalInsertionDistance, out newStartPoint3D, out newEndPoint3D);
                }
                else
                {
                    CalculateNewBeamPointsForDistanceAlongTheBeam(startPoint3Dm0, endPoint3Dm0, beamHorizontalInsertionDistance, out newStartPoint3D, out newEndPoint3D);
                }

                double verticalMoveForNewBeam = beamHeight / 2 / cosOfRoofSlope;
                double verticalEavesZMove = m0.Width / cosOfRoofSlope;
                double horizontalEavesXMove = m0.Width / 2 / sinOfRoofSlope;

                double normalEavesXMove = m0.Width / 2 * cosOfRoofSlopeAngleNotCasted;
                double normalEavesZMove = m0.Width / 2 * sinOfRoofSlopeAngleNotCasted;

                double xMoveForNewBeam = beamHeight / 2 * cosOfRoofSlopeAngleNotCasted;
                double zMoveForNewBeam = beamHeight / 2 * sinOfRoofSlopeAngleNotCasted;

                bool isNotSquareCrossSection = beamThickness != beamHeight;

                Member.MemberAlignment newBeamAlignment = GetBeamAlignment(_comboBoxNewBeamAlignement.SelectedItem.ToString(), Strings.Strings.newBeamAlignement);
                Member.MemberAlignment existBeamAlignment = GetBeamAlignment(_comboBoxExistBeamAlignement.SelectedItem.ToString(), Strings.Strings.existBeamAlignement);

                _steelBeam.Origin = DetermineBeamOrigin(newBeamAlignment, existBeamAlignment);
                SetBeamLocationWithExtensions(_steelBeam, beamStartExtension, beamEndExtension);
                _steelBeam.SectionGroupCode = "HEA";
                _steelBeam.SectionName = "HEA 400";
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

        private Member.MemberAlignment GetBeamAlignment(string alignmentOption, string alignmentName)
        {
            try
            {
                return ConvertToMemberAlignment(alignmentOption);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"{Strings.Strings.convertToMemberAlignementError} {alignmentName}: {ex.Message}");
                throw;
            }
        }

        private Point3D DetermineBeamOrigin(Member.MemberAlignment newBeamAlignment, Member.MemberAlignment existBeamAlignment)
        {
            if (IsRotatedToTheMainTruss)
            {
                return PrepareOriginForRotatedBeam(isLeftEdgeOnTop, newBeamAlignment, existBeamAlignment);
            }
            else
            {
                return PrepareOriginForNotRotatedBeam(isLeftEdgeOnTop, newBeamAlignment, existBeamAlignment);
            }
        }

        private Point3D PrepareOriginForNotRotatedBeam(bool isLeftEdgeOnTop, Member.MemberAlignment newBeamAlignment, Member.MemberAlignment existBeamAlignment)
        {
            double xNew = newStartPoint3D.X;
            double yNew = newStartPoint3D.Y;
            double zNew = newStartPoint3D.Z;

            if (isLeftEdgeOnTop)
            {
                switch (existBeamAlignment)
                {
                    case Member.MemberAlignment.RightEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew + verticalMove1LowestPoint + verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew + verticalMove1LowestPoint + verticalMove2LowestPoint + beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint + beamHeight / 2);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew + verticalMove1LowestPoint + verticalMove2LowestPoint);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint);
                                    }
                                }
                        }

                    case Member.MemberAlignment.LeftEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint - verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint - beamHeight / 2);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint - verticalMove2LowestPoint + beamHeight / 2);
                                    }

                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint + beamHeight / 2);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew + horizontalMove1LowestPoint - horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint - verticalMove2LowestPoint);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint);
                                    }
                                }
                        }

                    default:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint - beamHeight / 2);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - beamHeight / 2);

                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint - beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint + beamHeight / 2);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + beamHeight / 2);

                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + beamHeight / 2);

                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove + beamHeight / 2);

                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint + beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);

                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);

                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);

                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                        }
                }
            }
            else
            {
                switch (existBeamAlignment)
                {
                    case Member.MemberAlignment.RightEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - horizontalMove2LowestPoint, yNew, zNew + verticalMove1LowestPoint + verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2 + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - horizontalMove2LowestPoint, yNew, zNew + verticalMove1LowestPoint + verticalMove2LowestPoint + beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint + beamHeight / 2);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - horizontalMove2LowestPoint, yNew, zNew + verticalMove1LowestPoint + verticalMove2LowestPoint);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint);
                                    }
                                }
                        }

                    case Member.MemberAlignment.LeftEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint - verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint - beamHeight / 2);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2 + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint - verticalMove2LowestPoint + beamHeight / 2);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint + beamHeight / 2);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint - verticalMove2LowestPoint);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint);
                                    }
                                }
                        }

                    default:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 - beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint - beamHeight / 2);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew - beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 - beamHeight / 2);

                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove - beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint - beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2 + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove + beamHeight / 2);

                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint + beamHeight / 2);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew + beamHeight / 2);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2 + beamHeight / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove + beamHeight / 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint + beamHeight / 2);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                        }
                }
            }

        }

        private Point3D PrepareOriginForRotatedBeam(bool isLeftEdgeOnTop, Member.MemberAlignment newBeamAlignment, Member.MemberAlignment existBeamAlignment)
        {

            if (isLeftEdgeOnTop)
            {
                switch (existBeamAlignment)
                {
                    case Member.MemberAlignment.RightEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove1LowestPoint + verticalMove2LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew + verticalMove2LowestPoint - zMoveForNewBeam);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove1LowestPoint + verticalMove2LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove - xMoveForNewBeam, yNew, zNew + normalEavesZMove + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew + verticalMove2LowestPoint + zMoveForNewBeam);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + horizontalMove2LowestPoint, yNew, zNew - verticalMove1LowestPoint + verticalMove2LowestPoint);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint);
                                    }
                                }
                        }

                    case Member.MemberAlignment.LeftEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint + horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint + verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew + verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint + horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint + verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew + verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint + horizontalMove1LowestPoint, yNew, zNew - verticalMove2LowestPoint + verticalMove1LowestPoint);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint);
                                    }
                                }
                        }

                    default:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam - normalEavesXMove, yNew, zNew + zMoveForNewBeam + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                        }
                }
            }
            else
            {
                switch (existBeamAlignment)
                {
                    case Member.MemberAlignment.RightEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew + verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew + verticalMove2LowestPoint - zMoveForNewBeam);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + normalEavesZMove * 2 + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew + verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove + xMoveForNewBeam, yNew, zNew + normalEavesZMove + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew + verticalMove2LowestPoint + zMoveForNewBeam);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove * 2, yNew, zNew + normalEavesZMove * 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint);
                                    }
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove2LowestPoint, yNew, zNew + verticalMove2LowestPoint);
                                    }
                                }
                        }

                    case Member.MemberAlignment.LeftEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2 - xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint - horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint + verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2 + xMoveForNewBeam, yNew, zNew - normalEavesZMove * 2 + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint - horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint + verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove * 2, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove * 2, yNew, zNew - normalEavesZMove * 2);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint - horizontalMove1LowestPoint, yNew, zNew - verticalMove2LowestPoint + verticalMove1LowestPoint);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                                else
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew - horizontalMove1LowestPoint, yNew, zNew - verticalMove1LowestPoint);
                                    }
                                }
                        }

                    default:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove - xMoveForNewBeam, yNew, zNew - normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint - xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew - xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 - zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove - xMoveForNewBeam, yNew, zNew + normalEavesZMove - zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint - xMoveForNewBeam, yNew, zNew + verticalMove1LowestPoint - zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew - xMoveForNewBeam, yNew, zNew - zMoveForNewBeam);
                                }
                            case Member.MemberAlignment.LeftEdge:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew - verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove + xMoveForNewBeam, yNew, zNew - normalEavesZMove + zMoveForNewBeam);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint + xMoveForNewBeam, yNew, zNew - verticalMove2LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + verticalEavesZMove / 2 + zMoveForNewBeam);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + xMoveForNewBeam + normalEavesXMove, yNew, zNew + zMoveForNewBeam + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint + xMoveForNewBeam, yNew, zNew + verticalMove1LowestPoint + zMoveForNewBeam);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew + xMoveForNewBeam, yNew, zNew + zMoveForNewBeam);
                                }
                            default:
                                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew - horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew - verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew - normalEavesXMove, yNew, zNew - normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove2LowestPoint, yNew, zNew - verticalMove2LowestPoint);
                                    }
                                }
                                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                                {
                                    if (isHorizontalEaves)
                                    {
                                        return new Point3D(xNew + horizontalEavesXMove, yNew, zNew);
                                    }
                                    else if (isVerticalEaves)
                                    {
                                        return new Point3D(xNew, yNew, zNew + verticalEavesZMove / 2);
                                    }
                                    else if (isNormalEaves)
                                    {
                                        return new Point3D(xNew + normalEavesXMove, yNew, zNew + normalEavesZMove);
                                    }
                                    else
                                    {
                                        return new Point3D(xNew + horizontalMove1LowestPoint, yNew, zNew + verticalMove1LowestPoint);
                                    }
                                }
                                else
                                {
                                    return new Point3D(xNew, yNew, zNew);
                                }
                        }
                }
            }
        }

        private void SetBeamLocationWithExtensions(Member3D beam, double beamsStartExtension, double beamsEndExtension)
        {
            Point3D newStartPoint3DWithExtension;
            Point3D newEndPoint3DWithExtension;
            var beamOriginY = beam.Origin.Y;
            Vector3D planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(startPoint3Dm0, endPoint3Dm0, endPoint3Dm1);
            planeNormalToFutureBeamTruss.Normalize();

            double distanceBeetweenExistBeams = Math.Abs(m0.PartCSToGlobal.OffsetY - m1.PartCSToGlobal.OffsetY);

            bool isMinusDirection = startPoint3Dm0.Y > startPoint3Dm1.Y;

            if (isMinusDirection)
            {
                newStartPoint3DWithExtension = new Point3D(beam.Origin.X, beamOriginY - m0.Thickness / 2 + beamsStartExtension, beam.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(beam.Origin.X, beamOriginY + m0.Thickness / 2 - distanceBeetweenExistBeams - beamsEndExtension, beam.Origin.Z);
            }
            else
            {
                newStartPoint3DWithExtension = new Point3D(beam.Origin.X, beamOriginY + m0.Thickness / 2 - beamsStartExtension, beam.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(beam.Origin.X, beamOriginY - m0.Thickness / 2 + distanceBeetweenExistBeams + beamsEndExtension, beam.Origin.Z);
            }

            if (IsRotatedToTheMainTruss)
            {
                beam.SetAlignedStartPoint(newStartPoint3DWithExtension, planeNormalToFutureBeamTruss);
                beam.SetAlignedEndPoint(newEndPoint3DWithExtension, planeNormalToFutureBeamTruss);
            }
            else
            {
                beam.SetAlignedStartPoint(newStartPoint3DWithExtension, new Vector3D(0, 0, 1));
                beam.SetAlignedEndPoint(newEndPoint3DWithExtension, new Vector3D(0, 0, 1));
            }
        }

        private void CalculateExistBeamPoints(out Point3D startPoint3Dm0, out Point3D endPoint3Dm0, out Point3D startPoint3Dm1, out Point3D endPoint3Dm1)
        {
            startPoint3Dm0 = new Point3D(m0XStart, distYm0Center, m0YStart);
            endPoint3Dm0 = new Point3D(m0XEnd, distYm0Center, m0YEnd);
            startPoint3Dm1 = new Point3D(m1XStart, distYm1Center, m1YStart);
            endPoint3Dm1 = new Point3D(m1XEnd, distYm1Center, m1YEnd);
        }

        private void CalculateNewBeamPointsForCastDistance(Point3D startPoint3Dm0, Point3D endPoint3Dm0, double dH, out Point3D newStartPoint3D, out Point3D newEndPoint3D)
        {
            double deltaX = endPoint3Dm0.X - startPoint3Dm0.X;
            double deltaY = endPoint3Dm0.Y - startPoint3Dm0.Y;
            double deltaZ = endPoint3Dm0.Z - startPoint3Dm0.Z;

            double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

            double ux = deltaX / deltaTotal;
            double uy = deltaY / deltaTotal;

            double xNew = startPoint3Dm0.X + ux * dH;
            double yNew = startPoint3Dm0.Y + uy * dH;
            double zNew = startPoint3Dm0.Z + dH / deltaTotal * deltaZ;

            newStartPoint3D = new Point3D(xNew, yNew, zNew);
            newEndPoint3D = new Point3D(xNew, m1.PartCSToGlobal.OffsetY, zNew);
        }

        private void CalculateNewBeamPointsForDistanceAlongTheBeam(Point3D startPoint3Dm0, Point3D endPoint3Dm0, double dH, out Point3D newStartPoint3D, out Point3D newEndPoint3D)
        {
            double deltaX = endPoint3Dm0.X - startPoint3Dm0.X;
            double deltaY = endPoint3Dm0.Y - startPoint3Dm0.Y;
            double deltaZ = endPoint3Dm0.Z - startPoint3Dm0.Z;

            double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));

            double ux = deltaX / deltaTotal;
            double uy = deltaY / deltaTotal;
            double uz = deltaZ / deltaTotal;

            double xNew = startPoint3Dm0.X + ux * dH;
            double yNew = startPoint3Dm0.Y + uy * dH;
            double zNew = startPoint3Dm0.Z + uz * dH;

            newStartPoint3D = new Point3D(xNew, yNew, zNew);
            newEndPoint3D = new Point3D(xNew, m1.PartCSToGlobal.OffsetY, zNew);
        }

        private double ParseDoubleFromText(string text, string fieldName)
        {
            double result;
            if (double.TryParse(text, out result))
            {
                return result;
            }
            else
            {
                MessageBox.Show($"{Strings.Strings.wrong} {fieldName}. {Strings.Strings.willBeSetTo0}");
                return 0;
            }
        }

        private Vector3D? getReferenceEnforcementDirectionVector()
        {
            return new Vector3D(0, 0, 1);
        }

        private Member.MemberAlignment ConvertToMemberAlignment(string alignmentOption)
        {
            string topEdgeTranslation = Strings.Strings.topEdge;
            string bottomEdgeTranslation = Strings.Strings.bottomEdge;
            string centerTranslation = Strings.Strings.center;

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
            update3DNodes = new List<Epx.BIM.BaseDataNode>(0); // no need to update any model nodes

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