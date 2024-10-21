using Epx.BIM;
using Epx.BIM.Models;
using Epx.BIM.Models.Timber;
using Epx.BIM.Plugins;
using Epx.Ristek.Data.Models;
using Epx.Ristek.Data.Plugins;
using System;
using System.Collections.Generic;
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
                    if ((pluginObjectInput2.Point - (_preInputs[0] as PluginObjectInput).Point).Length < 1)
                    {
                        pluginInput.Valid = false;
                        pluginInput.ErrorMessage = Strings.Strings.pointsCannotBeTheSame;
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
        private TextBox _beamHorizontalInsertionDistance;
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

            CreateChapterTitleRow(mainStack, Constants.TitleChapter1BeamSettings);
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
            _beamHeight.Text = 60.ToString();
            _beamThickness.Text = 40.ToString();
            _beamHorizontalInsertionDistance.Text = 0.ToString();
            _beamStartExtension.Text = 0.ToString();
            _beamEndExtension.Text = 0.ToString();
#endif

            this.DialogResult = _dialog.ShowDialog().Value;
            return this.DialogResult;
        }

        private void CreateBeamRotationRow(StackPanel mainStack)
        {
            var beamStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamText = new TextBlock() { Text = Strings.Strings.setRotationRelativeToTheRoofSlope, Margin = new Thickness(0, 0, 10, 0) };
            beamStack.Children.Add(beamText);

            var beamRotationCheckBox = new CheckBox()
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

        private void CreateBeamDimensionsRow(StackPanel mainStack)
        {
            var thicknessHeightStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var thicknessHeightStackText = new TextBlock() { Text = Strings.Strings.setBeamDimensions, Margin = new Thickness(0, 0, 8, 0) };
            thicknessHeightStack.Children.Add(thicknessHeightStackText);
            string text = Strings.Strings.horizontalDimension;
            double textLength = text.Length * 6.5;

            _beamThickness = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(_beamThickness);
            _beamThickness.TextChanged += BeamDimension_TextChanged;
            thicknessHeightStack.Children.Add(_beamThickness);

            text = Strings.Strings.verticalDimension;
            textLength = text.Length * 6.5;

            _beamHeight = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(_beamHeight);
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
                MessageBox.Show(Strings.Strings.setBeamDimensions + " - " + Strings.Strings.horizontalDimension + " musi być większe od 0.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(_beamHeight.Text, out double height) || height <= 0)
            {
                MessageBox.Show(Strings.Strings.setBeamDimensions + " - " + Strings.Strings.verticalDimension + " musi być większe od 0.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            string text = Strings.Strings.distance;
            double textLength = text.Length * 7;
            _beamHorizontalInsertionDistance = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 0, 0), text);
            _beamHorizontalInsertionDistance.VerticalAlignment = VerticalAlignment.Center;
            HandleNumericTextBox(_beamHorizontalInsertionDistance);
            beamLocationStack.Children.Add(_beamHorizontalInsertionDistance);

            mainStack.Children.Add(beamLocationStack);
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

            m0 = GetTrussMemberFromPluginInput_(_preInputs[0]);
            m1 = GetTrussMemberFromPluginInput_(_preInputs[1]);
            existBeamLength = m0.Length;

            if (!IsNewBeamMultiplyed)
            {
                CreateSingleBeam();
            }
            else
            {
                beamMultiplySpacing = ParseDoubleFromText(_beamMultiplySpacing.Text, Strings.Strings.beamMultiplySpacing);

                double distYm0Center = m0.PartCSToGlobal.OffsetY;
                double distYm1Center = m1.PartCSToGlobal.OffsetY;

                startPoint3Dm0 = new Point3D(m0.AlignedStartPoint.X, distYm0Center, m0.AlignedStartPoint.Y);
                endPoint3Dm0 = new Point3D(m0.AlignedEndPoint.X, distYm0Center, m0.AlignedEndPoint.Y);
                horizontalCastLength = endPoint3Dm0.X - startPoint3Dm0.X;
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
            _timberBeam = new TimberBeam(Constants.TimberBeam);
            _timberBeam.AssemblyName = this.AssemblyName;
            _timberBeam.FullClassName = this.FullClassName;

            _timberBeam.Width = beamHeight;
            _timberBeam.Thickness = beamThickness;

            var distYm0Center = m0.PartCSToGlobal.OffsetY;
            var distYm1Center = m1.PartCSToGlobal.OffsetY;

            startPoint3Dm0 = new Point3D(m0.LeftEdgeLine.StartPoint.X, distYm0Center, m0.LeftEdgeLine.StartPoint.Y);
            endPoint3Dm0 = new Point3D(m0.LeftEdgeLine.EndPoint.X, distYm0Center, m0.LeftEdgeLine.EndPoint.Y);

            double cosOfRoofSlopeAngle = CalculateCosOfRoofSlopeAngle(startPoint3Dm0, endPoint3Dm0);

            CalculateTrussPoints(out startPoint3Dm0, out endPoint3Dm0, out startPoint3Dm1, out endPoint3Dm1);
            CalculateNewTrussPoints(startPoint3Dm0, endPoint3Dm0, beamHorizontalInsertionDistance, out newStartPoint3D, out newEndPoint3D);

            double verticalMoveForNewBeam = beamHeight / 2 / cosOfRoofSlopeAngle;
            double verticalMoveForExistBeam = m0.Width / cosOfRoofSlopeAngle;
            bool isNotSquareCrossSection = beamThickness != beamHeight;

            Member.MemberAlignment newBeamAlignment = GetBeamAlignment(_comboBoxNewBeamAlignement.SelectedItem.ToString(), Strings.Strings.newBeamAlignement);
            Member.MemberAlignment existBeamAlignment = GetBeamAlignment(_comboBoxExistBeamAlignement.SelectedItem.ToString(), Strings.Strings.existBeamAlignement);

            _timberBeam.Origin = DetermineTrussOrigin(verticalMoveForNewBeam, verticalMoveForExistBeam, newBeamAlignment, existBeamAlignment);

            SetBeamLocationWithExtensions(_timberBeam, beamStartExtension, beamEndExtension);
        }

        private double CalculateCosOfRoofSlopeAngle(Point3D startPoint3Dm0, Point3D endPoint3Dm0)
        {
            double deltaX = Math.Abs(endPoint3Dm0.X - startPoint3Dm0.X);
            double deltaZ = Math.Abs(endPoint3Dm0.Z - startPoint3Dm0.Z);
            double angleInRadians = Math.Atan2(deltaZ, deltaX);
            double angleInDegrees = angleInRadians * 180 / Math.PI;
            if (angleInDegrees > Constants.degrees45)
            {
                changeExistBeamAlignement = true;
            }
            return Math.Cos(angleInRadians);
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

        private Point3D DetermineTrussOrigin(double verticalMoveForNewBeam, double verticalMoveForExistBeam, Member.MemberAlignment newBeamAlignment, Member.MemberAlignment existBeamAlignment)
        {
            double xNew = newStartPoint3D.X;
            double yNew = newStartPoint3D.Y;
            double zNew = newStartPoint3D.Z;

            if (IsRotatedToTheMainTruss)
            {
                switch (existBeamAlignment)
                {
                    case Member.MemberAlignment.RightEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForNewBeam);
                            case Member.MemberAlignment.LeftEdge:
                                return new Point3D(xNew, yNew, zNew + verticalMoveForNewBeam);
                            default:
                                return new Point3D(xNew, yNew, zNew);
                        }

                    case Member.MemberAlignment.LeftEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam - verticalMoveForNewBeam);
                            case Member.MemberAlignment.LeftEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam + verticalMoveForNewBeam);
                            default:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam);
                        }

                    default:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam / 2 - verticalMoveForNewBeam);
                            case Member.MemberAlignment.LeftEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam / 2 + verticalMoveForNewBeam);
                            default:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam / 2);
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
                                return new Point3D(xNew, yNew, zNew - beamHeight / 2);
                            case Member.MemberAlignment.LeftEdge:
                                return new Point3D(xNew, yNew, zNew + beamHeight / 2);
                            default:
                                return new Point3D(xNew, yNew, zNew);
                        }

                    case Member.MemberAlignment.LeftEdge:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam - beamHeight / 2);
                            case Member.MemberAlignment.LeftEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam + beamHeight / 2);
                            default:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam);
                        }

                    default:
                        switch (newBeamAlignment)
                        {
                            case Member.MemberAlignment.RightEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam / 2 - beamHeight / 2);
                            case Member.MemberAlignment.LeftEdge:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam / 2 + beamHeight / 2);
                            default:
                                return new Point3D(xNew, yNew, zNew - verticalMoveForExistBeam / 2);
                        }
                }
            }
        }

        private void SetBeamLocationWithExtensions(TimberBeam beam, double beamsStartExtension, double beamsEndExtension)
        {
            Point3D newStartPoint3DWithExtension;
            Point3D newEndPoint3DWithExtension;
            Vector3D planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(startPoint3Dm0, endPoint3Dm0, endPoint3Dm1);
            planeNormalToFutureBeamTruss.Normalize();

            double distanceBeetweenExistBeams = Math.Abs(m0.PartCSToGlobal.OffsetY - m1.PartCSToGlobal.OffsetY);

            bool isMinusDirection = startPoint3Dm0.Y > startPoint3Dm1.Y;

            if (isMinusDirection)
            {
                newStartPoint3DWithExtension = new Point3D(beam.Origin.X, beam.Origin.Y - m0.Thickness / 2 + beamsStartExtension, beam.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(beam.Origin.X, beam.Origin.Y + m0.Thickness / 2 - distanceBeetweenExistBeams + beamsStartExtension, beam.Origin.Z);
            }
            else
            {
                newStartPoint3DWithExtension = new Point3D(beam.Origin.X, beam.Origin.Y + m0.Thickness / 2 - beamsStartExtension, beam.Origin.Z);
                newEndPoint3DWithExtension = new Point3D(beam.Origin.X, beam.Origin.Y - m0.Thickness / 2 + distanceBeetweenExistBeams + beamsStartExtension, beam.Origin.Z);
            }

            if (!IsRotatedToTheMainTruss)
            {
                beam.SetAlignedStartPoint(newStartPoint3DWithExtension, new Vector3D(0, 0, 1));
                beam.SetAlignedEndPoint(newEndPoint3DWithExtension, new Vector3D(0, 0, 1));
            }
            else
            {
                beam.SetAlignedStartPoint(newStartPoint3DWithExtension, planeNormalToFutureBeamTruss);
                beam.SetAlignedEndPoint(newEndPoint3DWithExtension, planeNormalToFutureBeamTruss);
            }
        }

        private void CalculateTrussPoints(out Point3D startPoint3Dm0, out Point3D endPoint3Dm0, out Point3D startPoint3Dm1, out Point3D endPoint3Dm1)
        {
            double distYm0Center = m0.PartCSToGlobal.OffsetY;
            double distYm1Center = m1.PartCSToGlobal.OffsetY;

            if (!changeExistBeamAlignement)
            {
                if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                {
                    startPoint3Dm0 = new Point3D(m0.LeftEdgeLine.StartPoint.X, distYm0Center, m0.LeftEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.LeftEdgeLine.EndPoint.X, distYm0Center, m0.LeftEdgeLine.EndPoint.Y);
                    startPoint3Dm1 = new Point3D(m1.LeftEdgeLine.StartPoint.X, distYm1Center, m1.LeftEdgeLine.StartPoint.Y);
                    endPoint3Dm1 = new Point3D(m1.LeftEdgeLine.EndPoint.X, distYm1Center, m1.LeftEdgeLine.EndPoint.Y);
                }
                else if (m0.Alignment == Member.MemberAlignment.RightEdge)
                {
                    startPoint3Dm0 = new Point3D(m0.RightEdgeLine.StartPoint.X, distYm0Center, m0.RightEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.RightEdgeLine.EndPoint.X, distYm0Center, m0.RightEdgeLine.EndPoint.Y);
                    startPoint3Dm1 = new Point3D(m1.RightEdgeLine.StartPoint.X, distYm1Center, m1.RightEdgeLine.StartPoint.Y);
                    endPoint3Dm1 = new Point3D(m1.RightEdgeLine.EndPoint.X, distYm1Center, m1.RightEdgeLine.EndPoint.Y);
                }
                else
                {
                    startPoint3Dm0 = new Point3D(m0.RightEdgeLine.StartPoint.X, distYm0Center, m0.RightEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.RightEdgeLine.EndPoint.X, distYm0Center, m0.RightEdgeLine.EndPoint.Y);
                    startPoint3Dm1 = new Point3D(m1.RightEdgeLine.StartPoint.X, distYm1Center, m1.RightEdgeLine.StartPoint.Y);
                    endPoint3Dm1 = new Point3D(m1.RightEdgeLine.EndPoint.X, distYm1Center, m1.RightEdgeLine.EndPoint.Y);
                }
            }
            else
            {
                if (m0.Alignment == Member.MemberAlignment.RightEdge)
                {
                    startPoint3Dm0 = new Point3D(m0.LeftEdgeLine.StartPoint.X, distYm0Center, m0.LeftEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.LeftEdgeLine.EndPoint.X, distYm0Center, m0.LeftEdgeLine.EndPoint.Y);
                    startPoint3Dm1 = new Point3D(m1.LeftEdgeLine.StartPoint.X, distYm1Center, m1.LeftEdgeLine.StartPoint.Y);
                    endPoint3Dm1 = new Point3D(m1.LeftEdgeLine.EndPoint.X, distYm1Center, m1.LeftEdgeLine.EndPoint.Y);
                }
                else if (m0.Alignment == Member.MemberAlignment.LeftEdge)
                {
                    startPoint3Dm0 = new Point3D(m0.RightEdgeLine.StartPoint.X, distYm0Center, m0.RightEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.RightEdgeLine.EndPoint.X, distYm0Center, m0.RightEdgeLine.EndPoint.Y);
                    startPoint3Dm1 = new Point3D(m1.RightEdgeLine.StartPoint.X, distYm1Center, m1.RightEdgeLine.StartPoint.Y);
                    endPoint3Dm1 = new Point3D(m1.RightEdgeLine.EndPoint.X, distYm1Center, m1.RightEdgeLine.EndPoint.Y);
                }
                else
                {
                    startPoint3Dm0 = new Point3D(m0.LeftEdgeLine.StartPoint.X, distYm0Center, m0.LeftEdgeLine.StartPoint.Y);
                    endPoint3Dm0 = new Point3D(m0.LeftEdgeLine.EndPoint.X, distYm0Center, m0.LeftEdgeLine.EndPoint.Y);
                    startPoint3Dm1 = new Point3D(m1.LeftEdgeLine.StartPoint.X, distYm1Center, m1.LeftEdgeLine.StartPoint.Y);
                    endPoint3Dm1 = new Point3D(m1.LeftEdgeLine.EndPoint.X, distYm1Center, m1.LeftEdgeLine.EndPoint.Y);
                }
            }



        }

        private void CalculateNewTrussPoints(Point3D startPoint3Dm0, Point3D endPoint3Dm0, double dH, out Point3D newStartPoint3D, out Point3D newEndPoint3D)
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