using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Controls.Ribbon;

namespace RistekPluginSample.UserControls
{
    /// <summary>
    /// Interaction logic for InputPhaseRibbonControl.xaml
    /// </summary>
    public partial class InputPhaseRibbonControl : RibbonGroup
    {
        string m_PluginVersionStr;

        public InputPhaseRibbonControl(string _PluginVersionStr)
        {
            //m_PluginVersionStr == null used as marker for RibbonDialogTabGroups
            m_PluginVersionStr = _PluginVersionStr;
            InitializeComponent();

            //this.DataContextChanged += InputPhaseRibbonControl_DataContextChanged;
            this.Loaded += InputPhaseRibbonControl_Loaded;

            // todo check integrated - zrobic bardziej elegancko, someday
            // for RibbonDialogTabGroups
            if (m_PluginVersionStr == null)
            {
                // 20240328 Knaga
                this.Width = 275;

                //this.ribbonComboBox_PlaneAlignement.MaxWidth = 65;
                this.numberRibbonTextBox_OffsetPerpendicular.Width = 40;
                this.numberRibbonTextBox_OffsetLengwiseBottom.Width = 45;
                this.numberRibbonTextBox_OffsetLengwiseTop.Width = 45;
                this.numberRibbonTextBox_OutreachPerpPrimary.Width = 45;
                this.numberRibbonTextBox_OutreachPerpSeccondary.Width = 45;

                // 20240328 Knaga
                //Grid.SetColumn(numberRibbonTextBox_OffsetLengwiseTop_WrapPanel, 2);
                //Grid.SetRow(numberRibbonTextBox_OffsetLengwiseTop_WrapPanel, 0);

                /*
                checkBox_AutosetTrussParamsInEditMode.Visibility = Visibility.Visible;
                Grid.SetColumn(checkBox_AutosetTrussParamsInEditMode, 1);
                Grid.SetColumnSpan(checkBox_AutosetTrussParamsInEditMode, 3);
                checkBox_AutosetTrussParamsInEditMode.Width = this.Width - 
                    new double[] {
                    numberRibbonTextBox_OffsetPerpendicular_externalLabel.Width + numberRibbonTextBox_OffsetPerpendicular.Width,
                    ribbonComboBox_PlaneAlignement_externalLabel.Width + ribbonComboBox_PlaneAlignement.Width
                    }.Max()
                    - 15
                    ;
                */
            }
            // for PreDialogRibbonTabGroup
            else
            {
                // 20240328 Knaga
                //this.Width = 600;
                this.Width = 850;

                this.ribbonComboBox_PlaneAlignement.SelectionBoxWidth = 95;

                //checkBox_AutosetTrussParamsInEditMode.Visibility = Visibility.Collapsed;
            }

            initTranslation();

#if DEBUG
#else
            // todo tmp ? wywalic te kontrolki
            ribbonComboBoxWithEditor_PlaneAlignement.Visibility = Visibility.Collapsed;
            RibbonComboBox_GreenTmp.Visibility = Visibility.Collapsed;
            //ButtonTest.Visibility = Visibility.Collapsed;
#endif
        }

        private void InputPhaseRibbonControl_Loaded(object sender, RoutedEventArgs e)
        {
            // todo check integrated - zrobic bardziej elegancko, someday
            // for PreDialogRibbonTabGroup
            if (m_PluginVersionStr != null)
            {
                TextBlock HeaderModifiedSpacePrefix = MyUtils.FindVisualChildren<TextBlock>(this).Where(x => x.Name == "HeaderModifiedSpacePrefix").FirstOrDefault();
                if (HeaderModifiedSpacePrefix != null)
                {
                    HeaderModifiedSpacePrefix.Visibility = Visibility.Visible;
                    HeaderModifiedSpacePrefix.Text = " [";
                }
                TextBox _HeaderModifiedTextBoxAppendix = MyUtils.FindVisualChildren<TextBox>(this).Where(x => x.Name == "HeaderModifiedTextBoxAppendix").FirstOrDefault();
                if (_HeaderModifiedTextBoxAppendix != null)
                {
                    _HeaderModifiedTextBoxAppendix.Visibility = Visibility.Visible;
                    _HeaderModifiedTextBoxAppendix.Text = "v0.0.0.0 ()";
                }
                TextBlock HeaderModifiedSpaceSufffix = MyUtils.FindVisualChildren<TextBlock>(this).Where(x => x.Name == "HeaderModifiedSpaceSufffix").FirstOrDefault();
                if (HeaderModifiedSpaceSufffix != null)
                {
                    HeaderModifiedSpaceSufffix.Visibility = Visibility.Visible;
                    HeaderModifiedSpaceSufffix.Text = "]";
                }
            }

            if (m_PluginVersionStr != null)
            {
                TextBox _HeaderModifiedTextBoxAppendix = MyUtils.FindVisualChildren<TextBox>(this).Where(x => x.Name == "HeaderModifiedTextBoxAppendix").FirstOrDefault();
                if (_HeaderModifiedTextBoxAppendix != null)
                {
                    //string pluginVersionStr = m_AdmSamplePluginObj.RTSamPluginVersionStr;
                    string pluginVersionStr = this.m_PluginVersionStr;
                    _HeaderModifiedTextBoxAppendix.Text = String.Format("{0}", pluginVersionStr);

                    TextBlock _HeaderModifiedBase = MyUtils.FindVisualChildren<TextBlock>(this).Where(x => x.Name == "HeaderModifiedBase").FirstOrDefault();
                    _HeaderModifiedTextBoxAppendix.Foreground = _HeaderModifiedBase.Foreground;
                }
            }
        }

        /*
        private void InputPhaseRibbonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is AdmSamplePlugin)
            {
                //m_AdmSamplePluginObj = e.NewValue as AdmSamplePlugin;
            }

            if (m_AdmSamplePluginObj != null
                && e.NewValue != m_AdmSamplePluginObj)
            {
                //this.DataContext = m_AdmSamplePluginObj;
            }
        }
        */

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            //this.numberRibbonTextBox_test.forceSync();
        }

        private void CheckBox_isKnagaMode_Checked(object sender, RoutedEventArgs e)
        {
            initTranslation();
        }

        internal void initTranslation()
        {
            this.ribbonComboBox_PlaneAlignement_WrapPanel.ToolTip = Strings.Strings._PlaneAlignementDesc;
            this.numberRibbonTextBox_OffsetPerpendicular_WrapPanel.ToolTip = Strings.Strings._OffsetPerpendicularDesc;
            this.numberRibbonTextBox_OffsetLengwiseBottom_WrapPanel.ToolTip = Strings.Strings._OffsetLengwiseBottomDesc;
            this.numberRibbonTextBox_OffsetLengwiseTop_WrapPanel.ToolTip = Strings.Strings._OffsetLengwiseTopDesc;
            this.numberRibbonTextBox_OutreachPerpPrimary_WrapPanel.ToolTip = Strings.Strings._OutreachPerpPrimaryDesc;
            this.numberRibbonTextBox_OutreachPerpSeccondary_WrapPanel.ToolTip = Strings.Strings._OutreachPerpSeccondaryDesc;
            this.checkBox_AutosetTrussParamsInEditMode_AccessText.Text = Strings.Strings._AutosetTrussParamsInEditModeDesc;
            this.checkBox_isKnagaMode_AccessText.Text = Strings.Strings._KnagaModeDesc;
            this.checkBox_isAutoaddSupports_AccessText.Text = Strings.Strings._AutoaddSupportsDesc;

            this.ribbonComboBox_PlaneAlignement.Label = "";
            this.numberRibbonTextBox_OffsetPerpendicular.Label = "";
            this.numberRibbonTextBox_OffsetLengwiseBottom.Label = "";
            this.numberRibbonTextBox_OffsetLengwiseTop.Label = "";

            // todo check integrated - zrobic bardziej elegancko, someday
            // for RibbonDialogTabGroups
            if (m_PluginVersionStr == null)
            {
                this.ribbonComboBox_PlaneAlignement_externalLabel.Content = "\u21C5";
                this.numberRibbonTextBox_OffsetPerpendicular_externalLabel.Content = "\u21A7";
                if (this.checkBox_isKnagaMode.IsChecked.Value)
                {
                    this.ribbonComboBox_PlaneAlignement_externalLabel.Content = "\u21C4";
                    this.numberRibbonTextBox_OffsetPerpendicular_externalLabel.Content = "\u21A6";
                }
                this.numberRibbonTextBox_OffsetLengwiseBottom_externalLabel.Content = "\u2197";
                this.numberRibbonTextBox_OffsetLengwiseTop_externalLabel.Content = "\u2199";
                this.numberRibbonTextBox_OutreachPerpPrimary.Label = "\u21A9";
                this.numberRibbonTextBox_OutreachPerpSeccondary.Label = "\u21AA";
            }
            // for PreDialogRibbonTabGroup
            else
            {
                this.ribbonComboBox_PlaneAlignement.Label = Strings.Strings._PlaneAlignementDesc + " ";
                this.numberRibbonTextBox_OffsetPerpendicular.Label = Strings.Strings._OffsetPerpendicularDesc + " ";
                this.numberRibbonTextBox_OffsetLengwiseBottom.Label = Strings.Strings._OffsetLengwiseBottomDesc + " ";
                this.numberRibbonTextBox_OffsetLengwiseTop.Label = Strings.Strings._OffsetLengwiseTopDesc + " ";
                this.numberRibbonTextBox_OutreachPerpPrimary.Label = Strings.Strings._OutreachPerpPrimaryDesc + " ";
                this.numberRibbonTextBox_OutreachPerpSeccondary.Label = Strings.Strings._OutreachPerpSeccondaryDesc + " ";

                this.ribbonComboBox_PlaneAlignement.Label += "\u21C5";
                this.numberRibbonTextBox_OffsetPerpendicular.Label += "\u21A7";
                if (this.checkBox_isKnagaMode.IsChecked.Value)
                {
                    this.ribbonComboBox_PlaneAlignement.Label = this.ribbonComboBox_PlaneAlignement.Label.Replace("\u21C5", "\u21C4");
                    this.numberRibbonTextBox_OffsetPerpendicular.Label = this.numberRibbonTextBox_OffsetPerpendicular.Label.Replace("\u21A7", "\u21A6");
                }
                this.numberRibbonTextBox_OffsetLengwiseBottom.Label += " \u2197";
                this.numberRibbonTextBox_OffsetLengwiseTop.Label += " \u2199";
                // 20240328 Knaga
                this.numberRibbonTextBox_OutreachPerpPrimary.Label += " \u21A9";
                this.numberRibbonTextBox_OutreachPerpSeccondary.Label += " \u21AA";
            }
        }
    }
}
