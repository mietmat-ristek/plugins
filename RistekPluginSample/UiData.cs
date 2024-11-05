using Epx.Ristek.Data.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RistekPluginSample
{
    public class UiData
    {
        public StackPanel MainStack { get; set; }
        public ComboBox BeamType { get; set; }
        public ComboBox BeamDistanceType { get; set; }
        public ComboBox NewBeamAlignement { get; set; }
        public ComboBox ExistBeamAlignement { get; set; }
        public CheckBox BeamRotation { get; set; }
        public TextBox BeamHeightTextBox { get; set; }
        public TextBox BeamThicknessTextBox { get; set; }
        public TextBox BeamInsertionDistanceTextBox { get; set; } = new TextBox();
        public TextBox BeamStartExtensionTextBox { get; set; }
        public TextBox BeamEndExtensionTextBox { get; set; }
        public TextBox BeamMultiplySpacingTextBox { get; set; }

        public string BeamHeightText { get; set; }
        public string BeamThicknessText { get; set; }
        public string BeamInsertionDistanceText { get; set; }
        public string BeamStartExtensionText { get; set; }
        public string BeamEndExtensionText { get; set; }
        public string BeamMultiplySpacingText { get; set; }

        public double BeamHeightValue { get; set; }
        public double BeamThicknessValue { get; set; }
        public double BeamInsertionDistanceValue { get; set; }
        public double BeamStartExtensionValue { get; set; }
        public double BeamEndExtensionValue { get; set; }
        public double BeamMultiplySpacingValue { get; set; }

        public bool IsRotatedToTheMainTruss { get; set; }
        public bool IsNewBeamMultiplyed { get; set; }
        public bool CastToTheHorizontalDistance { get; set; }

        private Model3D _model3D { get; set; }

        public double horizontalCastLength;
        double alongBeamLength;

        public UiData(Model3D model3D)
        {
            _model3D = model3D;

            MainStack = new StackPanel() { Name = Constants.MainStack, Margin = new Thickness(8) };
            MainStack.Orientation = Orientation.Vertical;

            CreateChapterTitleRow(Constants.TitleChapter0SelectedBeamInformation);
            CreateSelectedBeamCoordinatesInfoRow();
            CreateSelectedBeamLengthInfoRow();
            CreateChapterTitleRow(Constants.TitleChapter1BeamSettings);
            CreateSelectionOfBeamType();
            CreateBeamDimensionsRow();
            CreateNewBeamAlignementOptionRow();
            CreateExistBeamAlignementOptionRow();
            CreateBeamExtensionRow();
            CreateBeamRotationRow();
            CreateChapterTitleRow(Constants.TitleChapter2BeamLocation);
            CreateBeamLocationRow();
            CreateBeamMultiplicationRow();

#if DEBUG
            BeamHeightTextBox.Text = 100.ToString();
            BeamThicknessTextBox.Text = 40.ToString();
            BeamStartExtensionTextBox.Text = 42.ToString();
            BeamEndExtensionTextBox.Text = 42.ToString();
            NewBeamAlignement.Text = "Upper plane";
            ExistBeamAlignement.Text = "Upper plane";
            BeamRotation.IsChecked = true;
            BeamDistanceType.Text = "Along beam";
            BeamInsertionDistanceTextBox.Text = 0.ToString();

#endif
        }

        private void CreateSelectedBeamLengthInfoRow()
        {
            var selectedBeamLengthStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            var selectedBeamLengthStackText = new TextBlock() { Text = Strings.Strings.beamLength, Margin = new Thickness(0, 0, 8, 0) };
            var selectedBeamLengthStackValue = new TextBlock() { Text = Math.Round(_model3D.SelectedBeamLength, 0).ToString() + Constants.mm, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeamLengthStack.Children.Add(selectedBeamLengthStackText);
            selectedBeamLengthStack.Children.Add(selectedBeamLengthStackValue);

            MainStack.Children.Add(selectedBeamLengthStack);
        }

        private void CreateSelectionOfBeamType()
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = Strings.Strings.selectionOfBeamType, Margin = new Thickness(0, 0, 8, 0) };
            comboBoxStack.Children.Add(comboBoxText);
            BeamType = new ComboBox() { Width = Double.NaN, Margin = new Thickness(0, 0, 4, 0) };
            BeamType.Items.Add(Strings.Strings.timberBeam);
            BeamType.Items.Add(Strings.Strings.steelBeam);
            BeamType.SelectedIndex = 0;
            comboBoxStack.Children.Add(BeamType);

            MainStack.Children.Add(comboBoxStack);
        }

        private void CreateBeamRotationRow()
        {
            var beamStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamText = new TextBlock() { Text = Strings.Strings.setRotationRelativeToTheRoofSlope, Margin = new Thickness(0, 0, 10, 0) };
            beamStack.Children.Add(beamText);

            BeamRotation = new CheckBox()
            {
                Content = Strings.Strings.rotateBeam,
                Margin = new Thickness(0, 0, 4, 0)
            };
            BeamRotation.Checked += (s, e) => IsRotatedToTheMainTruss = true;
            BeamRotation.Unchecked += (s, e) => IsRotatedToTheMainTruss = false;

            beamStack.Children.Add(BeamRotation);

            MainStack.Children.Add(beamStack);
        }

        private void CreateChapterTitleRow(string chapterTitle)
        {
            var chapetrTitleStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var chapetrTitleStackText = new TextBlock()
            {
                Text = chapterTitle,
                Margin = new Thickness(0, 0, 4, 0),
                FontWeight = FontWeights.Bold
            };
            chapetrTitleStack.Children.Add(chapetrTitleStackText);
            MainStack.Children.Add(chapetrTitleStack);
        }

        private void CreateSelectedBeamCoordinatesInfoRow()
        {
            var selectedBeam0CoordinatesStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            var selectedBeamCoordinatesForFirstBeamStackText = new TextBlock() { Text = Strings.Strings.firstSelectedBeamCoordinates, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(selectedBeamCoordinatesForFirstBeamStackText);

            var startPoint0StackText = new TextBlock() { Text = Strings.Strings.startPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam0CoordinatesStack.Children.Add(startPoint0StackText);

            var beam0StartXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartXStackText);
            var beam0StartXValueStackText = new TextBlock() { Text = _model3D.Beam3DNo1.MemberStartX.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartXValueStackText);
            var beam0StartYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartYStackText);
            var beam0StartYValueStackText = new TextBlock() { Text = _model3D.Beam3DNo1.MemberStartY.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0StartYValueStackText);

            var endPoint0StackText = new TextBlock() { Text = Strings.Strings.endPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam0CoordinatesStack.Children.Add(endPoint0StackText);

            var beam0SEndXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0SEndXStackText);
            var beam0EndXValueStackText = new TextBlock() { Text = _model3D.Beam3DNo1.MemberEndX.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0EndXValueStackText);
            var beam0EndYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0EndYStackText);
            var beam0EndYValueStackText = new TextBlock() { Text = _model3D.Beam3DNo1.MemberEndY.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam0CoordinatesStack.Children.Add(beam0EndYValueStackText);
            MainStack.Children.Add(selectedBeam0CoordinatesStack);

            var selectedBeam1CoordinatesStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            var selectedBeamCoordinatesForSecondBeamStackText = new TextBlock() { Text = Strings.Strings.secondSelectedBeamCoordinates, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(selectedBeamCoordinatesForSecondBeamStackText);

            var startPoint1StackText = new TextBlock() { Text = Strings.Strings.startPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam1CoordinatesStack.Children.Add(startPoint1StackText);

            var beam1StartXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartXStackText);
            var beam1StartXValueStackText = new TextBlock() { Text = _model3D.Beam3DNo2.MemberStartX.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartXValueStackText);
            var beam1StartYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartYStackText);
            var beam1StartYValueStackText = new TextBlock() { Text = _model3D.Beam3DNo2.MemberStartY.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1StartYValueStackText);

            var endPoint1StackText = new TextBlock() { Text = Strings.Strings.endPoint, Margin = new Thickness(0, 0, 8, 0), FontWeight = FontWeights.Bold };
            selectedBeam1CoordinatesStack.Children.Add(endPoint1StackText);

            var beam1SEndXStackText = new TextBlock() { Text = Constants.X, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1SEndXStackText);
            var beam1EndXValueStackText = new TextBlock() { Text = _model3D.Beam3DNo2.MemberEndX.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1EndXValueStackText);
            var beam1EndYStackText = new TextBlock() { Text = Constants.Y, Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1EndYStackText);
            var beam1EndYValueStackText = new TextBlock() { Text = _model3D.Beam3DNo2.MemberEndY.ToString(), Margin = new Thickness(0, 0, 8, 0) };
            selectedBeam1CoordinatesStack.Children.Add(beam1EndYValueStackText);

            MainStack.Children.Add(selectedBeam1CoordinatesStack);
        }

        private void CreateBeamDimensionsRow()
        {
            var thicknessHeightStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var thicknessHeightStackText = new TextBlock() { Text = Strings.Strings.setBeamDimensions, Margin = new Thickness(0, 0, 8, 0) };
            thicknessHeightStack.Children.Add(thicknessHeightStackText);
            string text = Strings.Strings.horizontalDimension;
            double textLength = text.Length * 6.5;

            BeamThicknessTextBox = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            BeamThicknessTextBox.BorderBrush = Brushes.Red;
            HandleNumericTextBox(BeamThicknessTextBox);
            BeamThicknessTextBox.TextChanged += BeamDimension_TextChanged;
            thicknessHeightStack.Children.Add(BeamThicknessTextBox);

            text = Strings.Strings.verticalDimension;
            textLength = text.Length * 6.5;

            BeamHeightTextBox = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(BeamHeightTextBox);
            BeamHeightTextBox.BorderBrush = Brushes.Red;

            BeamHeightTextBox.TextChanged += BeamDimension_TextChanged;
            thicknessHeightStack.Children.Add(BeamHeightTextBox);



            MainStack.Children.Add(thicknessHeightStack);
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
            if (!double.TryParse(BeamThicknessTextBox.Text, out double thickness) || thickness <= 0)
            {
                MessageBox.Show(Strings.Strings.setBeamDimensions + " - " + Strings.Strings.horizontalDimension + Strings.Strings.haveToBeLargerThanZero, Strings.Strings.validationIssue, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(BeamHeightTextBox.Text, out double height) || height <= 0)
            {
                MessageBox.Show(Strings.Strings.setBeamDimensions + " - " + Strings.Strings.verticalDimension + Strings.Strings.haveToBeLargerThanZero, Strings.Strings.validationIssue, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void CreateBeamLocationRow()
        {
            var beamLocationStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamLocationStackStackText = new TextBlock()
            {
                Text = Strings.Strings.setHorizontalInsertionFromReferencePart1 + Constants.goToNewLine + Strings.Strings.setHorizontalInsertionFromReferencePart2,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            beamLocationStack.Children.Add(beamLocationStackStackText);
            BeamDistanceType = new ComboBox() { Margin = new Thickness(0, 0, 4, 0) };
            BeamDistanceType.Items.Add(Strings.Strings.alongBeam);
            BeamDistanceType.Items.Add(Strings.Strings.horizontalCast);
            BeamDistanceType.SelectedIndex = 0;
            BeamDistanceType.SelectionChanged += SelectionChangedBeamDistanceTypeHandler;
            int distanceTypeLength = 0;
            foreach (var item in BeamDistanceType.Items)
            {
                int currentItemLength = item.ToString().Length;
                if (distanceTypeLength < currentItemLength)
                {
                    distanceTypeLength = currentItemLength;
                }
            }
            BeamDistanceType.Width = distanceTypeLength * Constants.multiplyerForLettersWidth8;
            BeamDistanceType.VerticalAlignment = VerticalAlignment.Center;
            beamLocationStack.Children.Add(BeamDistanceType);
            CastToTheHorizontalDistance = BeamDistanceType.Text == Constants.horizontalCast;

            string text = Strings.Strings.distance;
            double textLength = text.Length * Constants.multiplyerForLettersWidth8;
            BeamInsertionDistanceTextBox = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 0, 0), text);
            BeamInsertionDistanceTextBox.VerticalAlignment = VerticalAlignment.Center;

            BeamInsertionDistanceTextBox.PreviewTextInput += TextBox_PreviewTextInput;
            BeamInsertionDistanceTextBox.TextChanged += BeamHorizontalInsertionDistance_TextChanged;

            HandleNumericTextBox(BeamInsertionDistanceTextBox);
            beamLocationStack.Children.Add(BeamInsertionDistanceTextBox);

            MainStack.Children.Add(beamLocationStack);
        }

        private void SelectionChangedBeamDistanceTypeHandler(object sender, SelectionChangedEventArgs e)
        {
            string selectedText = (sender as ComboBox).SelectedItem.ToString();

            CastToTheHorizontalDistance = selectedText == Constants.horizontalCast;

            BeamHorizontalInsertionDistance_TextChanged(BeamInsertionDistanceTextBox, null);
        }

        private void SelectionChangedExistBeamAlignementHandler(object sender, SelectionChangedEventArgs e)
        {
            BeamHorizontalInsertionDistance_TextChanged(BeamInsertionDistanceTextBox, null);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void BeamHorizontalInsertionDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_model3D.Beam3DNo1.IsSelectedMemberLeftEdgeOnTop && ExistBeamAlignement.SelectedIndex == 0)
            {
                SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment.LeftEdge);
            }
            else if (_model3D.Beam3DNo1.IsSelectedMemberLeftEdgeOnTop && ExistBeamAlignement.SelectedIndex == 1)
            {
                SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment.RightEdge);
            }
            else if (!_model3D.Beam3DNo1.IsSelectedMemberLeftEdgeOnTop && ExistBeamAlignement.SelectedIndex == 1)
            {
                SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment.LeftEdge);
            }
            else if (!_model3D.Beam3DNo1.IsSelectedMemberLeftEdgeOnTop && ExistBeamAlignement.SelectedIndex == 0)
            {
                SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment.RightEdge);
            }
            else
            {
                SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment.Center);
            }


            double beamMaxLength = CastToTheHorizontalDistance ? horizontalCastLength : alongBeamLength;

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

        private void SetMaxBeamLengthForExpectedAlignement(Member.MemberAlignment existBeamAlignment)
        {
            double m0XStart;
            double m0YStart;
            double m0XEnd;
            double m0YEnd;

            if (existBeamAlignment.ToString() == Constants.RightEdge)
            {
                m0XStart = Math.Round(_model3D.Member1.RightGeometryEdgeLine.StartPoint.X, 0);
                m0YStart = Math.Round(_model3D.Member1.RightGeometryEdgeLine.StartPoint.Y, 0);
                m0XEnd = Math.Round(_model3D.Member1.RightGeometryEdgeLine.EndPoint.X, 0);
                m0YEnd = Math.Round(_model3D.Member1.RightGeometryEdgeLine.EndPoint.Y, 0);
            }
            else if (existBeamAlignment.ToString() == Constants.LeftEdge)
            {
                m0XStart = Math.Round(_model3D.Member1.LeftGeometryEdgeLine.StartPoint.X, 0);
                m0YStart = Math.Round(_model3D.Member1.LeftGeometryEdgeLine.StartPoint.Y, 0);
                m0XEnd = Math.Round(_model3D.Member1.LeftGeometryEdgeLine.EndPoint.X, 0);
                m0YEnd = Math.Round(_model3D.Member1.LeftGeometryEdgeLine.EndPoint.Y, 0);
            }
            else
            {
                if (!_model3D.Beam3DNo1.IsMemberEndCombinedCut)
                {
                    m0XStart = Math.Round(_model3D.Member1.MiddleCuttedLineWithBooleanCuts.StartPoint.X, 0);
                    m0YStart = Math.Round(_model3D.Member1.MiddleCuttedLineWithBooleanCuts.StartPoint.Y, 0);
                    m0XEnd = Math.Round(_model3D.Member1.MiddleCuttedLineWithBooleanCuts.EndPoint.X, 0);
                    m0YEnd = Math.Round(_model3D.Member1.MiddleCuttedLineWithBooleanCuts.EndPoint.Y, 0);
                }
                else
                {
                    m0XStart = Math.Round(_model3D.Beam3DNo1.TheLowestStartPointForCombinedCuttedBeam.X, 0);
                    m0YStart = Math.Round(_model3D.Beam3DNo1.TheLowestStartPointForCombinedCuttedBeam.Y, 0);
                    m0XEnd = Math.Round(_model3D.Beam3DNo1.TheHighestStartPointForCombinedCuttedBeam.X, 0);
                    m0YEnd = Math.Round(_model3D.Beam3DNo1.TheHighestStartPointForCombinedCuttedBeam.Y, 0);
                }

            }


            var startPoint3Dm0 = new Point3D(m0XStart, _model3D.Beam3DNo1.DistanceY, m0YStart);
            var endPoint3Dm0 = new Point3D(m0XEnd, _model3D.Beam3DNo1.DistanceY, m0YEnd);
            double deltaX = endPoint3Dm0.X - startPoint3Dm0.X;
            double deltaY = endPoint3Dm0.Y - startPoint3Dm0.Y;
            double deltaZ = endPoint3Dm0.Z - startPoint3Dm0.Z;

            double deltaTotal = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
            alongBeamLength = deltaTotal;
            horizontalCastLength = Math.Abs(deltaX);
        }


        private void CreateBeamMultiplicationRow()
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

            BeamMultiplySpacingTextBox = new TextBox()
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

            beamStack.Children.Add(BeamMultiplySpacingTextBox);
            beamStack.Children.Add(spacingLabel);

            MainStack.Children.Add(beamStack);
        }

        private void CreateNewBeamAlignementOptionRow()
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = Strings.Strings.alignementForNewBeam, Margin = new Thickness(0, 0, 8, 0) };
            comboBoxStack.Children.Add(comboBoxText);
            NewBeamAlignement = new ComboBox() { Width = Double.NaN, Margin = new Thickness(0, 0, 4, 0) };
            NewBeamAlignement.Items.Add(Strings.Strings.topEdge);
            NewBeamAlignement.Items.Add(Strings.Strings.bottomEdge);
            NewBeamAlignement.Items.Add(Strings.Strings.center);
            NewBeamAlignement.SelectedIndex = 0;
            comboBoxStack.Children.Add(NewBeamAlignement);

            MainStack.Children.Add(comboBoxStack);
        }

        private void CreateExistBeamAlignementOptionRow()
        {
            var comboBoxStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var comboBoxText = new TextBlock() { Text = Strings.Strings.alignementForExistBeam, Margin = new Thickness(0, 0, 8, 0) };
            comboBoxStack.Children.Add(comboBoxText);
            ExistBeamAlignement = new ComboBox() { Width = Double.NaN, Margin = new Thickness(0, 0, 4, 0) };
            ExistBeamAlignement.Items.Add(Strings.Strings.topEdge);
            ExistBeamAlignement.Items.Add(Strings.Strings.bottomEdge);
            ExistBeamAlignement.Items.Add(Strings.Strings.center);
            ExistBeamAlignement.SelectedIndex = 0;
            ExistBeamAlignement.SelectionChanged += SelectionChangedExistBeamAlignementHandler;
            BeamInsertionDistanceTextBox.TextChanged += BeamHorizontalInsertionDistance_TextChanged;

            comboBoxStack.Children.Add(ExistBeamAlignement);

            MainStack.Children.Add(comboBoxStack);
        }

        private void CreateBeamExtensionRow()
        {
            var beamExtensionStack = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var beamExtensionText = new TextBlock() { Text = Strings.Strings.setBeamExtension, Margin = new Thickness(0, 0, 8, 0) };
            beamExtensionStack.Children.Add(beamExtensionText);

            string text = Strings.Strings.beamStartExtension;
            double textLength = text.Length * 6.5;
            BeamStartExtensionTextBox = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(BeamStartExtensionTextBox);
            beamExtensionStack.Children.Add(BeamStartExtensionTextBox);

            text = Strings.Strings.beamEndExtension;
            textLength = text.Length * 6.5;
            BeamEndExtensionTextBox = CreateTextBox(Constants.InputBox, textLength, new Thickness(0, 0, 4, 0), text);
            HandleNumericTextBox(BeamEndExtensionTextBox);
            beamExtensionStack.Children.Add(BeamEndExtensionTextBox);

            MainStack.Children.Add(beamExtensionStack);
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

        public void CreateValuesFromTextBox()
        {
            BeamHeightValue = ParseDoubleFromText(BeamHeightTextBox.Text, Strings.Strings.beamHeight); ;
            BeamThicknessValue = ParseDoubleFromText(BeamThicknessTextBox.Text, Strings.Strings.beamThickness);
            BeamInsertionDistanceValue = ParseDoubleFromText(BeamInsertionDistanceTextBox.Text, Strings.Strings.beamHorizontalInsertionDistance);
            BeamStartExtensionValue = ParseDoubleFromText(BeamStartExtensionTextBox.Text, Strings.Strings.beamStartExtension);
            BeamEndExtensionValue = ParseDoubleFromText(BeamEndExtensionTextBox.Text, Strings.Strings.beamEndExtension);
            BeamMultiplySpacingValue = ParseDoubleFromText(BeamMultiplySpacingTextBox.Text, Strings.Strings.beamMultiplySpacing);
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


    }
}
