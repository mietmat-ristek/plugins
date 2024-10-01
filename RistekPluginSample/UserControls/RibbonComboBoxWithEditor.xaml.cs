using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace RistekPluginSample.UserControls
{
    // made by AA
    // for short CombBox text / long ComboBoxItem text see https://stackoverflow.com/questions/3995853/how-to-display-a-different-value-for-dropdown-list-values-selected-item-in-a-wpf/69251915#69251915
    /// <summary>
    /// Interaction logic for RibbonComboBoxWithEditor.xaml
    /// </summary>
    public partial class RibbonComboBoxWithEditor : RibbonComboBox
    {
        #region init

        public RibbonComboBoxWithEditor()
        {
            InitializeComponent();

            this.LoadedCommand = new ActionCommand(OnLoaded); // ActionCommand: simple implementation of ICommand
            initNumberboxPopup();
        }

        private bool m_initStylesAdnTemplates_performed = false;
        private void initStylesAdnTemplates()
        {
            if (m_initStylesAdnTemplates_performed
                || !IsExternalsInitialized
                )
                return;

            // todo wywalic
            /*
            System.Windows.Data.Binding myBinding = new System.Windows.Data.Binding();
            myBinding.Source = this;
            myBinding.Path = new PropertyPath(SelectedValuePathSubstitution);
            System.Windows.Data.BindingOperations.SetBinding(this.ribbonGallery, RibbonGallery.SelectedValueProperty, myBinding);
            */
            //System.Windows.Data.BindingOperations.SetBinding(taskItemListView, ListView.DataContextProperty, new Binding("SelectedItem") { Source = itemListView });
            //System.Windows.Data.BindingOperations.SetBinding(this.ribbonGalleryCategory, RibbonGalleryCategory.ItemsSourceProperty, new System.Windows.Data.Binding(ItemsSourcePathSubstitution) { Source = this });
            initRibbonGallery();
            initRibbonGalleryCategory();

            // todo tmp odkomentowac
            initRibbonComboBoxSelectorDataTemplates();
            initRibbonGalleryCategorySelectorDataTemplates();
            initItemContainerStyle();

            m_initStylesAdnTemplates_performed = true;
        }

        RibbonGallery ribbonGallery;
        private void initRibbonGallery()
        {
            var xamlElemString =
                @"<RibbonGallery
                        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                        SelectedItem=""{Binding Path=" + SelectedValueBindingPathSubstitution + @", Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}""
                        DisplayMemberPath=""" + DataTemplateSelectedTemplatePropertyName + @"""
                        MaxColumnCount = ""1""
                    >
                    <RibbonGalleryCategory 
                            ItemsSource=""{Binding " + ItemsSourceBindingPathSubstitution + @"}""
                        >
                        <RibbonGalleryCategory.Resources>
                        </RibbonGalleryCategory.Resources>
                    </RibbonGalleryCategory>
                    <RibbonGallery.Resources>
                    </RibbonGallery.Resources>
                </RibbonGallery>";
            var stringReader = new StringReader(xamlElemString);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            RibbonGallery _ribbonGalleryNew = (RibbonGallery)XamlReader.Load(xmlReader);
            // todo tmp odkomentowac?
            //_ribbonGalleryNew.SelectionChanged += RibbonGallery_SelectionChanged;
            ribbonGallery = _ribbonGalleryNew;

            this.Items.Add(ribbonGallery);
        }

        RibbonGalleryCategory ribbonGalleryCategory;
        private void initRibbonGalleryCategory()
        {
            // todo tmp?
            /*
            var xamlElemString =
                @"<RibbonGalleryCategory 
                        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                        ItemsSource=""{Binding " + ItemsSourceBindingPathSubstitution + @"}""
                    >
                    <RibbonGalleryCategory.Resources>
                    </RibbonGalleryCategory.Resources>
                </RibbonGalleryCategory>";
            var stringReader = new StringReader(xamlElemString);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            RibbonGalleryCategory _ribbonGalleryCategoryNew = (RibbonGalleryCategory)XamlReader.Load(xmlReader);
            ribbonGalleryCategory = _ribbonGalleryCategoryNew;

            this.ribbonGallery.Items.Add(ribbonGalleryCategory);
            */
            ribbonGalleryCategory = this.ribbonGallery.Items[0] as RibbonGalleryCategory;
        }

        // see https://stackoverflow.com/questions/10370187/programmatically-add-a-textblock-to-a-datatemplate
        // see https://stackoverflow.com/questions/60499590/how-to-create-a-datatemplate-using-c-sharp-and-set-child-controls-resources
        // see https://stackoverflow.com/questions/8427972/how-do-i-create-a-datatemplate-with-content-programmatically
        // see https://stackoverflow.com/questions/5471405/create-datatemplate-in-codebehind
        private void initRibbonComboBoxSelectorDataTemplates()
        {
            foreach (KeyValuePair<string, string> _keyValuePariCurr in new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("selectedTemplate", DataTemplateSelectedTemplatePropertyName),
                //new KeyValuePair<string, string>("dropDownTemplate", DataTemplateDropDownTemplatePropertyName),
                })
            {
                //DataTemplate _selectedTemplate = this.Resources[_keyValuePariCurr.Key] as DataTemplate;

                // not working - cant override _selectedTemplate.VisualTree
                /*
                FrameworkElementFactory newTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                //newTextBlock.Name = "txt";
                newTextBlock.SetBinding(TextBlock.TextProperty, new Binding(_keyValuePariCurr.Value));
                _selectedTemplate.VisualTree = newTextBlock;
                */

                // alternative version, works ok
                /*
                DataTemplate _templateNew =
                  TemplateGenerator.CreateDataTemplate
                  (
                    () =>
                    {
                        var _newTextBlock = new TextBlock();
                        _newTextBlock.SetBinding(TextBlock.TextProperty, _keyValuePariCurr.Value);
                        return _newTextBlock;
                    }
                  );
                */

                var dataTemplateString =
                    @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                                    x:Key=""" + _keyValuePariCurr.Key + @"""
                                    >
                        <TextBlock Text=""{Binding " + _keyValuePariCurr.Value + @"}""/>
                    </DataTemplate>"
                    ;
                var stringReader = new StringReader(dataTemplateString);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                DataTemplate _templateNew = (DataTemplate)XamlReader.Load(xmlReader);

                //this.Resources[_keyValuePariCurr.Key] = _templateNew;
                if (this.Resources.Contains(_keyValuePariCurr.Key))
                {
                    this.Resources.Remove(_keyValuePariCurr.Key);
                }
                // todo tmp? odkomentowac?
                //this.Resources.Add(_keyValuePariCurr.Key, _templateNew);
            }

            // todo tmp? odkomentowac?
            //this.ItemTemplateSelector = new RibbonComboBoxTemplateSelector();
        }

        private void initRibbonGalleryCategorySelectorDataTemplates()
        {
            foreach (KeyValuePair<string, string> _keyValuePariCurr in new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("selectedTemplate", DataTemplateSelectedTemplatePropertyName),
                new KeyValuePair<string, string>("dropDownTemplate", DataTemplateDropDownTemplatePropertyName),
                })
            {
                //DataTemplate _selectedTemplate = this.Resources[_keyValuePariCurr.Key] as DataTemplate;

                // not working - cant override _selectedTemplate.VisualTree
                /*
                FrameworkElementFactory newTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                //newTextBlock.Name = "txt";
                newTextBlock.SetBinding(TextBlock.TextProperty, new Binding(_keyValuePariCurr.Value));
                _selectedTemplate.VisualTree = newTextBlock;
                */

                // alternative version, works ok
                /*
                DataTemplate _templateNew =
                  TemplateGenerator.CreateDataTemplate
                  (
                    () =>
                    {
                        var _newTextBlock = new TextBlock();
                        _newTextBlock.SetBinding(TextBlock.TextProperty, _keyValuePariCurr.Value);
                        return _newTextBlock;
                    }
                  );
                */

                var dataTemplateString =
                    @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                                    x:Key=""" + _keyValuePariCurr.Key + @"""
                                    >
                        <TextBlock Text=""{Binding " + _keyValuePariCurr.Value + @"}""/>
                    </DataTemplate>"
                    ;
                var stringReader = new StringReader(dataTemplateString);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                DataTemplate _templateNew = (DataTemplate)XamlReader.Load(xmlReader);

                //this.Resources[_keyValuePariCurr.Key] = _templateNew;
                if (this.ribbonGalleryCategory.Resources.Contains(_keyValuePariCurr.Key))
                {
                    this.ribbonGalleryCategory.Resources.Remove(_keyValuePariCurr.Key);
                }
                this.ribbonGalleryCategory.Resources.Add(_keyValuePariCurr.Key, _templateNew);
            }

            this.ribbonGalleryCategory.ItemTemplateSelector = new RibbonGalleryItemTemplateSelector();
        }

        private void initItemContainerStyle()
        {
            string _keyName = "myItemToolTipStyle";
            string _toolTipPropertyName = this.ItemToolTipPropertyName;

            var _styleString =
                @"<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                                TargetType=""RibbonGalleryItem""
                                x:Key=""" + _keyName + @"""
                                >
                    <Setter Property=""ToolTip"">
                        <Setter.Value>
                            <TextBlock Text=""{Binding " + _toolTipPropertyName + @"}""/>
                        </Setter.Value>
                    </Setter>
                 </Style>"
                ;
            var stringReader = new StringReader(_styleString);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Style _styleNew = (Style)XamlReader.Load(xmlReader);

            this.ribbonGalleryCategory.ItemContainerStyle = _styleNew;
        }

        private void initNumberboxPopup()
        {
            popup = new Popup();
            popup.IsOpen = false;
            popup.PlacementTarget = this.ribbonGallery;
            popup.Placement = PlacementMode.Center;

            numberTextBox = new NumberRibbonTextBox();
            numberTextBox.EditFinished += numberTextBox_EditFinished;

            DockPanel dockPanel = new DockPanel();
            dockPanel.Children.Add(numberTextBox);

            popup.Child = dockPanel;
        }

        #endregion

        #region objects

        private Popup popup;
        private NumberRibbonTextBox numberTextBox;

        public static readonly DependencyProperty IsExternalsInitializedProperty =
            DependencyProperty.Register("IsExternalsInitialized", typeof(bool), typeof(RibbonComboBoxWithEditor), new PropertyMetadata(null));

        #endregion

        #region properties

        //public object ItemWithExternalEdit { get; set; }
        public Object[] ItemWithExternalEditArr { get; set; }

        public bool IsExternalsInitialized
        {
            get { return (bool)this.GetValue(IsExternalsInitializedProperty); }
            set
            {
                this.SetValue(IsExternalsInitializedProperty, value);
                initStylesAdnTemplates();
            }
        }

        public string SelectedValueBindingPathSubstitution { get; set; }
        public string ItemsSourceBindingPathSubstitution { get; set; }

        public string ExternalItemEditableFieldName { get; set; }

        public string DataTemplateSelectedTemplatePropertyName { get; set; }
        public string DataTemplateDropDownTemplatePropertyName { get; set; }
        public string ItemToolTipPropertyName { get; set; }

        #endregion

        #region event handlers

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            initStylesAdnTemplates();

            // todo tmp wywalic ?
            //System.Windows.Data.BindingExpression be = ribbonGallery.GetBindingExpression(RibbonGallery.SelectedItemProperty);
            //be?.UpdateSource();
        }

        // see: https://stackoverflow.com/questions/15555449/how-to-databind-selecteditem-of-ribboncombobox
        #region inducted onLoad

        public ICommand LoadedCommand { get; private set; }

        private void OnLoaded()
        {
            //...
            initStylesAdnTemplates();
        }

        #endregion

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            // todo odkomentowac
            //showNumberboxPopup(sender);
        }

        //private void ComboBox_SelectionChangedXXX(object sender, SelectionChangedEventArgs e)
        private void RibbonGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // handled by ComboBox_DropDownClosed anyway
            //showNumberboxPopup(sender);
        }

        private void ComboBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // todo odkomentowac
            /*
            // try-catch beacause design time error, see https://stackoverflow.com/questions/34264934/wpf-converter-throwing-object-reference-not-set-at-design-time
            try
            {
                popup.Width = this.ribbonGallery.Width;
                popup.Height = this.ribbonGallery.Height;
            }
            catch
            {

            }
            */
        }

        private void numberTextBox_EditFinished(object sender, NumberTextBoxEditFinishedEventArgs e)
        {
            if (e.isEditOk)
            {
                //ItemWithExternalEdit.GetType().GetProperty(ExternalItemEditableFieldName).SetValue(ItemWithExternalEdit, MyUtils.ToDouble(numberTextBox.Text));
                Object _externalEditItemCurr = this.ribbonGallery.SelectedItem;
                _externalEditItemCurr.GetType().GetProperty(ExternalItemEditableFieldName).SetValue(_externalEditItemCurr, MyUtils.ToDouble(numberTextBox.Text));
            }
            this.popup.IsOpen = false;
        }

        #endregion

        #region methods helpers

        private void showNumberboxPopup(object sender)
        {
            // try-catch beacause design time error, see https://stackoverflow.com/questions/34264934/wpf-converter-throwing-object-reference-not-set-at-design-time
            try
            {
                if (//senderCasted.SelectedItem == ItemWithExternalEdit
                    new List<Object>(ItemWithExternalEditArr).Contains(this.ribbonGallery.SelectedItem)
                    && IsExternalsInitialized
                    )
                {
                    //numberTextBox.Text = MyUtils.pv((double)ItemWithExternalEdit.GetType().GetProperty(ExternalItemEditableFieldName).GetValue(ItemWithExternalEdit));
                    Object _externalEditItemCurr = this.ribbonGallery.SelectedItem;
                    numberTextBox.Text = MyUtils.pv((double)_externalEditItemCurr.GetType().GetProperty(ExternalItemEditableFieldName).GetValue(_externalEditItemCurr));
                    this.popup.IsOpen = true;
                    numberTextBox.Focus();
                }
                else
                {
                    this.popup.IsOpen = false;
                }
            }
            catch
            {

            }
        }

        #endregion



    }

    // see https://stackoverflow.com/questions/3995853/how-to-display-a-different-value-for-dropdown-list-values-selected-item-in-a-wpf/69251915#69251915
    public class RibbonComboBoxTemplateSelector : DataTemplateSelector
    {
        // version with static properities names hardcoded in xaml
        /*
        public DataTemplate DropDownTemplate
        {
            get;
            set;
        }
        public DataTemplate SelectedTemplate
        {
            get;
            set;
        }
        */

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            RibbonComboBoxWithEditor comboBoxWithEditor = MyUtils.GetVisualParent<RibbonComboBoxWithEditor>(container);
            if (comboBoxWithEditor == null)
            {
                return null;
            }
            return comboBoxWithEditor.Resources["selectedTemplate"] as DataTemplate;
        }

    }

    public class RibbonGalleryItemTemplateSelector : DataTemplateSelector
    {
        // version with static properities names hardcoded in xaml
        /*
        public DataTemplate DropDownTemplate
        {
            get;
            set;
        }
        public DataTemplate SelectedTemplate
        {
            get;
            set;
        }
        */

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // version with static properities names hardcoded in xaml
            /*
            ComboBoxItem comboBoxItem = ComboBoxItemTemplateSelector.GetVisualParent<ComboBoxItem>(container);
            if (comboBoxItem != null)
            {
                return DropDownTemplate;
            }
            return SelectedTemplate;
            */

            RibbonGalleryItem comboBoxItem = MyUtils.GetVisualParent<RibbonGalleryItem>(container);
            if (comboBoxItem != null)
            {
                RibbonGalleryCategory comboBox = ItemsControl.ItemsControlFromItemContainer(comboBoxItem) as RibbonGalleryCategory;
                return comboBox.Resources["dropDownTemplate"] as DataTemplate;
            }

            RibbonGalleryCategory comboBoxWithEditor = MyUtils.GetVisualParent<RibbonGalleryCategory>(container);
            if (comboBoxWithEditor == null)
            {
                return null;
            }
            return comboBoxWithEditor.Resources["selectedTemplate"] as DataTemplate;
        }

    }

}
