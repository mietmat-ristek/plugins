using Epx.BIM;
using Epx.BIM.Models;
using Epx.BIM.Plugins;
using Epx.Ristek.Data.Models;
using Epx.Ristek.Data.Plugins;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                return "Sample plugin - insert Member";
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
                if (CurrentUICulture.ThreeLetterISOLanguageName == "fin")
                    return "Lisää sauva ristikolle.";
                else
                    return "Insert a member to the truss.";
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
                objectInput1.Prompt = "Select the first beam";
                if (CurrentUICulture.ThreeLetterISOLanguageName == "fin") objectInput1.Prompt = "Valitse ensimmäinen piste"; // alternative to Resx dictionary
                _preInputs.Add(objectInput1);

                PluginObjectInput objectInput2 = new PluginObjectInput();
                objectInput2.Index = 1;
                objectInput2.Prompt = "Select the second beam";
                if (CurrentUICulture.ThreeLetterISOLanguageName == "fin") objectInput2.Prompt = "Valitse toinen piste";
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
            if (pluginInput.Index == 0 && pluginInput is PluginObjectInput)
            {
                pluginInput.Valid = true;
                return true;
            }
            else if (pluginInput.Index == 1)
            {
                PluginObjectInput poi = pluginInput as PluginObjectInput;
                if (poi == null)
                {
                    pluginInput.Valid = false;
                    pluginInput.ErrorMessage = "Wrong input type";
                    if (CurrentUICulture.ThreeLetterISOLanguageName == "fin") pluginInput.ErrorMessage = "Syötteen tyyppi on virheellinen";
                    return false;
                }
                else
                {
                    if ((poi.Point - (_preInputs[0] as PluginObjectInput).Point).Length < 1)
                    {
                        pluginInput.Valid = false;
                        pluginInput.ErrorMessage = "First and second point cannot be the same";
                        if (CurrentUICulture.ThreeLetterISOLanguageName == "fin") pluginInput.ErrorMessage = "Ensimmäinen ja toinen piste eivät saa olla samoja";
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
                pluginInput.ErrorMessage = "Wrong input index";
                if (CurrentUICulture.ThreeLetterISOLanguageName == "fin") pluginInput.ErrorMessage = "Syötteen indeksi on virheellinen";
                return false;
            }
        }

        private System.Windows.Window _dialog;
        private TextBox _beamHeight;
        private TextBox _beamThickness;
        private TextBox _beamHorizontalInsertionDistance;
        private TextBox _beamStartExtension;
        private TextBox _beamEndExtension;
        private ComboBox _comboBox;
        private bool IsRotatedToTheMainTruss;

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

            var mainStack = new StackPanel() { Name = "MainStack", Margin = new Thickness(8) };
            mainStack.Orientation = Orientation.Vertical;

            CreateChapterTitleRow(mainStack, Constants.TitleChapter1BeamSettings);
            CreateBeamDimensionsRow(mainStack);
            CreateAlignementOptionRow(mainStack);
            CreateBeamExtensionRow(mainStack);
            CreateBeamRotationRow(mainStack);
            CreateChapterTitleRow(mainStack, Constants.TitleChapter2BeamLocation);
            CreateBeamLocationRow(mainStack);


            CreateFinalButtonRow(mainStack);

            _dialog.Content = mainStack;
            _dialog.Closed += (s, a) => { _dialog = null; PluginEngine?.PluginDialogClosed(DialogResult); };
            _dialog.Show();

            return false;
        }

        private void CreateBeamRotationRow(StackPanel mainStack)
        {
            var beamStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamText = new TextBlock() { Text = "Set the rotation relative to the roof slope:", Margin = new Thickness(0, 0, 15, 0) };
            beamStack.Children.Add(beamText);

            var beamRotationCheckBox = new CheckBox()
            {
                Content = "Rotate beam",
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
            var generateButton = new Button() { Content = "Draw Beam", Width = 80, Margin = new Thickness(0, 0, 4, 0) };
            generateButton.Click += OnCreateButtonClicked;
            generateButton.Click += (s, a) => { this.DialogResult = true; _dialog.Close(); };
            buttonStack.Children.Add(generateButton);
            mainStack.Children.Add(buttonStack);
        }

        private void CreateBeamDimensionsRow(StackPanel mainStack)
        {
            var thicknessHeightStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var thicknessHeightStackText = new TextBlock() { Text = "Set beam dimensions: ", Margin = new Thickness(0, 0, 4, 0) };
            thicknessHeightStack.Children.Add(thicknessHeightStackText);

            _beamThickness = CreateTextBox("InputBox", 120, new Thickness(0, 0, 4, 0), "Beam Thickness [mm]");
            HandleNumericTextBox(_beamThickness);
            thicknessHeightStack.Children.Add(_beamThickness);

            _beamHeight = CreateTextBox("InputBox", 120, new Thickness(0, 0, 4, 0), "Beam Height [mm]");
            HandleNumericTextBox(_beamHeight);
            thicknessHeightStack.Children.Add(_beamHeight);

            mainStack.Children.Add(thicknessHeightStack);
        }

        private void CreateBeamLocationRow(StackPanel mainStack)
        {
            var beamLocationStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamLocationStackStackText = new TextBlock() { Text = "Set the beam horizontal insertion distance (from reference beam): ", Margin = new Thickness(0, 0, 4, 0) };
            beamLocationStack.Children.Add(beamLocationStackStackText);

            _beamHorizontalInsertionDistance = CreateTextBox("InputBox", 120, new Thickness(0, 0, 4, 0), "Beam horizontal distance [mm]");
            HandleNumericTextBox(_beamHorizontalInsertionDistance);
            beamLocationStack.Children.Add(_beamHorizontalInsertionDistance);

            mainStack.Children.Add(beamLocationStack);
        }

        private void CreateAlignementOptionRow(StackPanel mainStack)
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = "Select Vertical Beam Alignement Option:", Margin = new Thickness(0, 0, 4, 0) };
            comboBoxStack.Children.Add(comboBoxText);

            _comboBox = new ComboBox() { Width = 120, Margin = new Thickness(0, 0, 4, 0) };
            _comboBox.Items.Add("Top edge");
            _comboBox.Items.Add("Bottom edge");
            _comboBox.Items.Add("Center");
            _comboBox.SelectedIndex = 0;
            comboBoxStack.Children.Add(_comboBox);

            mainStack.Children.Add(comboBoxStack);
        }

        private void CreateBeamExtensionRow(StackPanel mainStack)
        {
            var beamExtensionStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamExtensionText = new TextBlock() { Text = "Set beam extension:", Margin = new Thickness(0, 0, 4, 0) };
            beamExtensionStack.Children.Add(beamExtensionText);

            _beamStartExtension = CreateTextBox("InputBox", 140, new Thickness(0, 0, 4, 0), "Beam start extension [mm]");
            HandleNumericTextBox(_beamStartExtension);
            beamExtensionStack.Children.Add(_beamStartExtension);

            _beamEndExtension = CreateTextBox("InputBox", 140, new Thickness(0, 0, 4, 0), "Beam end extension [mm]");
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
            Regex regex = new Regex("[^0-9.-]+"); // Regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private TrussFrame _myTruss;
        private ModelFolderNode _supportFolder;

        private void OnCreateButtonClicked(object sender, RoutedEventArgs args)
        {
            double beamHeight;
            double thickness;
            double beamHorizontalInsertionDistance;
            double beamsStartExtension;
            double beamsEndExtension;


            _myTruss = new TrussFrame("Truss");
            Member member = new Member("AP");

            Member m0 = GetTrussMemberFromPluginInput_(_preInputs[0]);
            Member m1 = GetTrussMemberFromPluginInput_(_preInputs[1]);

            if (double.TryParse(_beamHorizontalInsertionDistance.Text, out beamHorizontalInsertionDistance))
            {
                beamHorizontalInsertionDistance = double.Parse(_beamHorizontalInsertionDistance.Text);
            }
            else
            {
                MessageBox.Show("Wrong beam thickness value.");
            }

            var distXm0Center = m0.PartCSToGlobal.OffsetX;
            var distYm0Center = m0.PartCSToGlobal.OffsetY;
            var distZm0Center = m0.PartCSToGlobal.OffsetZ;
            var distXm1Center = m1.PartCSToGlobal.OffsetX;
            var distYm1Center = m1.PartCSToGlobal.OffsetY;
            var distZm1Center = m1.PartCSToGlobal.OffsetZ;

            double distanceBeetweenTrusses = Math.Abs(distYm1Center - distYm0Center) - m1.Thickness / 2;

            double startPointXm0 = m0.AlignedStartPoint.X;
            double endPointXm0 = m0.AlignedEndPoint.X;
            double startPointZm0 = m0.AlignedStartPoint.Y;
            double endPointZm0 = m0.AlignedEndPoint.Y;
            double startPointXm1 = m1.AlignedStartPoint.X;
            double endPointXm1 = m1.AlignedEndPoint.X;
            double startPointZm1 = m1.AlignedStartPoint.Y;
            double endPointZm1 = m1.AlignedEndPoint.Y;


            Point3D startPoint3Dm0 = new Point3D(startPointXm0, distYm0Center, startPointZm0);
            Point3D endPoint3Dm0 = new Point3D(endPointXm0, distYm0Center, endPointZm0);
            Point3D startPoint3Dm1 = new Point3D(startPointXm1, distYm1Center, startPointZm1);
            Point3D endPoint3Dm1 = new Point3D(endPointXm1, distYm1Center, endPointZm1);

            var deltaX = endPoint3Dm0.X - startPoint3Dm0.X;
            var deltaY = endPoint3Dm0.Y - startPoint3Dm0.Y;
            var deltaZ = endPoint3Dm0.Z - startPoint3Dm0.Z;

            var deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            var ux = deltaX / deltaTotal;
            var uy = deltaY / deltaTotal;

            var dH = beamHorizontalInsertionDistance;
            var xNew = startPointXm0 + ux * dH;
            var yNew = distYm0Center + uy * dH;
            var zNew = startPointZm0 + dH / deltaTotal * deltaZ;

            var newStartPoint3D = new Point3D(xNew, yNew, zNew);
            var newEndPoint3D = new Point3D(xNew, distYm1Center, zNew);

            _myTruss.Origin = newStartPoint3D;
            var vectorMember = newStartPoint3D - newEndPoint3D;
            _myTruss.XAxis = vectorMember;

            if (IsRotatedToTheMainTruss)
            {
                Vector3D planeNormalToFutureBeamTruss = MyUtils.CalculateNormal(startPoint3Dm0, endPoint3Dm0, endPoint3Dm1);
                planeNormalToFutureBeamTruss.Normalize();

                Point3D directionPoint = newEndPoint3D;

                _myTruss.SetXAxis(newStartPoint3D - directionPoint, planeNormalToFutureBeamTruss);
            }
            _modelViewNodes.Add(_myTruss); // show in 3D view while plugin is running

            if (double.TryParse(_beamHeight.Text, out beamHeight))
            {
                member.SetWidth(double.Parse(_beamHeight.Text));
            }
            else
            {
                MessageBox.Show("Wrong beam height value.");
            }


            if (double.TryParse(_beamThickness.Text, out thickness))
            {
                _myTruss.Thickness = double.Parse(_beamThickness.Text);
            }
            else
            {
                MessageBox.Show("Wrong beam thickness value.");
            }

            string selectedAlignementOption = _comboBox.SelectedItem.ToString();

            try
            {
                Member.MemberAlignment alignment = ConvertToMemberAlignment(selectedAlignementOption);
                member.Alignment = alignment;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Convert To Member Alignment error: {ex.Message}");
            }

            member.AlignedStartPoint = new Point(-m0.Thickness / 2, 0);

            if (double.TryParse(_beamStartExtension.Text, out beamsStartExtension))
            {
                member.AlignedStartPoint = new Point(-m0.Thickness / 2 + beamsStartExtension, 0);
            }
            else
            {
                MessageBox.Show("Wrong beam start extension value.");
            }

            var memberLengthX = Math.Abs((_preInputs[1] as PluginObjectInput).Point.X - (_preInputs[0] as PluginObjectInput).Point.X);
            var memberLengthY = Math.Abs((_preInputs[1] as PluginObjectInput).Point.Y - (_preInputs[0] as PluginObjectInput).Point.Y);
            var memberLengthZ = Math.Abs((_preInputs[1] as PluginObjectInput).Point.Z - (_preInputs[0] as PluginObjectInput).Point.Z);


            if (memberLengthX != 0)
            {
                member.AlignedEndPoint = new Point(memberLengthX, 0);

                if (double.TryParse(_beamEndExtension.Text, out beamsEndExtension))
                {
                    member.AlignedEndPoint = new Point(memberLengthX + beamsEndExtension, 0);
                }
                else
                {
                    MessageBox.Show("Wrong beam end extension value.");
                }
            }
            else if (memberLengthY != 0)
            {
                member.AlignedEndPoint = new Point(memberLengthY, 0);

                if (double.TryParse(_beamEndExtension.Text, out beamsEndExtension))
                {
                    member.AlignedEndPoint = new Point(memberLengthY + beamsEndExtension, 0);
                }
                else
                {
                    MessageBox.Show("Wrong beam end extension value.");
                }
            }
            else
            {
                member.AlignedEndPoint = new Point(memberLengthZ, 0);

                if (double.TryParse(_beamEndExtension.Text, out beamsEndExtension))
                {
                    member.AlignedEndPoint = new Point(memberLengthZ + beamsEndExtension, 0);
                }
                else
                {
                    MessageBox.Show("Wrong beam end extension value.");
                }
            }

            member.AlignedEndPoint = new Point(-distanceBeetweenTrusses - beamsEndExtension, 0);


            _myTruss.AddMember(member, true);
            _myTruss.UpdateMemberCuts(member, true);

            PluginEngine?.PluginUpdate3D(true);
        }

        private Vector3D? getReferenceEnforcementDirectionVector()
        {
            return new Vector3D(0, 0, 1);
        }

        private Member.MemberAlignment ConvertToMemberAlignment(string alignmentOption)
        {
            switch (alignmentOption)
            {
                case "Top edge":
                    return Member.MemberAlignment.LeftEdge;
                case "Bottom edge":
                    return Member.MemberAlignment.RightEdge;
                case "Center":
                    return Member.MemberAlignment.Center;
                default:
                    throw new ArgumentException("Unknown alignment option");
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
            doDelegate = null;
            undoDelegate = null;
            update3DNodes = new List<Epx.BIM.BaseDataNode>(0); // no need to update any model nodes

            List<BaseDataNode> addedNodes = new List<BaseDataNode>();
            addedNodes.Add(_myTruss);
            addedNodes.Add(_supportFolder);

            ResetModelViewNodes();

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