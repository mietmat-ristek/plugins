using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;

namespace RistekPluginSample.UserControls
{
    // see https://www.codeproject.com/Tips/1155905/WPF-Numerical-TextBox-with-Math-Operations
    /// <summary>
    /// A WPF custom <see cref="System.Windows.Controls.Ribbon.RibbonTextBox">RibbonTextBox</see> for editing numbers.
    /// </summary>
    public class NumberRibbonTextBox : RibbonTextBox
    {
        public NumberRibbonTextBox()
            : base()
        {
            PreviewTextInput += OnPreviewTextInput;
            //this.KeyDown += OnKeyDown;
            //this.TextChanged += TextChangedHandle;
            this.LostFocus += LostFocusHandle;
            this.EditFinished += NumberRibbonTextBox_EditFinished;
        }

        #region properties

        public bool AllowNegative { get; set; }

        #endregion

        private static Regex regexDisallowedInteger =
        new Regex(@"[^0-9-]+");  // matches disallowed text
        private static Regex regexDisallowedFloat =
        new Regex(@"[^0-9-+.,e]+");  // matches disallowed text

        private double getDoubleVal(out bool tryParse)
        {
            var fullText = this.Text;
            char decimalDeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            fullText = fullText.Replace('.', decimalDeparator);
            fullText = fullText.Replace(',', decimalDeparator);

            double val;
            tryParse = double.TryParse(fullText, out val);

            return val;
        }

        public double getDoubleVal()
        {
            bool tryParse;
            double res = getDoubleVal(out tryParse);

            if (!tryParse)
            { throw new NotImplementedException(); }

            return res;
        }

        public event EventHandler<NumberTextBoxEditFinishedEventArgs> EditFinished;

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Text.Length > 0 && OpenMathMenu(e.Text))
                e.Handled = true;
            else
                e.Handled = regexDisallowedFloat.IsMatch(e.Text);  // or regexDisallowedInteger

            // =========================

            var textBox = sender as System.Windows.Controls.TextBox;

            // Use SelectionStart property to find the caret position.
            // Insert the previewed text into the existing text in the textbox.
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            char decimalDeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            fullText = fullText.Replace('.', decimalDeparator);
            fullText = fullText.Replace(',', decimalDeparator);

            double val;
            // If parsing is successful, set Handled to false
            e.Handled |= !(AllowNegative && fullText.Equals("-"))
                && !fullText.Equals("+")
                && !double.TryParse(fullText, out val)
                ;
        }

        /*
        // see https://stackoverflow.com/questions/9442097/how-to-replace-characters-in-a-text-box-as-a-user-is-typing-in-it-in-c
        private void OnKeyDown(Object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;


            char[] text = textBox.Text.ToCharArray();
            int pos = textBox.SelectionStart;

            switch (e.Key)
            {
                case Key.Back:
                    if (pos == 0)
                        return;
                    pos--;
                    break;
                case Key.Delete:
                    if (pos == text.Length)
                        return;
                    break;
                //default: return;
            }

            char charOnPosition = text[pos];
            switch (charOnPosition)
            {
                case ',':
                case '.':
                    text[pos] = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator.First();
                    break;
                default: return;
            }
            textBox.Text = new String(text);
            e.Handled = true;
        }
        */

        /*
        private void TextChangedHandle(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            string text = textBox.Text;
            //modify the text...
            string modifiedText = text;
            modifiedText = modifiedText.Replace(",", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            modifiedText = modifiedText.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            if (!String.Equals(text, modifiedText))
            {
                this.Text = modifiedText;
            }
        }
        */

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            //var tb = (TextBox)sender;
            var tb = this;
            using (tb.DeclareChangeBlock())
            {
                foreach (var c in e.Changes)
                {
                    if (c.AddedLength == 0) continue;
                    tb.Select(c.Offset, c.AddedLength);
                    if (tb.SelectedText.Contains(',')
                        && !String.Equals(",", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                        )
                    {
                        tb.SelectedText = tb.SelectedText.Replace(",", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    }
                    if (tb.SelectedText.Contains('.')
                        && !String.Equals(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                        )
                    {
                        tb.SelectedText = tb.SelectedText.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    }
                    tb.Select(c.Offset + c.AddedLength, 0);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                EditFinished?.Invoke(this, new NumberTextBoxEditFinishedEventArgs() { isEditOk = false });
            }
            else if (Keyboard.IsKeyDown(Key.Enter)
                && !String.IsNullOrEmpty(this.Text)
                )
            {
                bool tryParse;
                getDoubleVal(out tryParse);
                if (tryParse)
                {
                    EditFinished?.Invoke(this, new NumberTextBoxEditFinishedEventArgs() { isEditOk = true });

                    //forceSync();
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            LostFocusHandle(this, e);
        }

        // seems to be not working for RibbonTextBox; only when whole ribbon group is closed?
        private void LostFocusHandle(object sender, RoutedEventArgs e)
        {
            EditFinished?.Invoke(this, new NumberTextBoxEditFinishedEventArgs() { isEditOk = false, isLostFocus = true });
        }

        private void NumberRibbonTextBox_EditFinished(object sender, NumberTextBoxEditFinishedEventArgs e)
        {
            forceSync();
        }

        double? m_forceSyncLasrValSaved = null;
        public void forceSync()
        {
            bool _tryParse;
            double _valCurr = getDoubleVal(out _tryParse);
            if (_tryParse 
                && (!m_forceSyncLasrValSaved.HasValue
                    || m_forceSyncLasrValSaved.Value != _valCurr
                    )
                )
            {
                System.Windows.Data.BindingExpression be = this.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();

                m_forceSyncLasrValSaved = _valCurr;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (menuMath == null 
                || !menuMath.IsOpen
                )
            {
                forceSync();
            }
        }

        #region Math

        private enum EOperation { Add, Subtract, Multiply, Divide }
        private EOperation operation = EOperation.Add;
        private ContextMenu menuMath = null;
        private MenuItem miOperand = null;
        private MenuItem miResult = null;

        private bool OpenMathMenu(string text)
        {
            if (menuMath != null)
                return false;

            if (text == "+" || text == "-" ||
            text == "*" || text == "/")
            {
                if (text == "-" && CaretIndex == 0)  // negative sign in front of number
                    return false;

                miOperand = new MenuItem();
                miOperand.Header = "";
                miOperand.FontSize = 18;
                miOperand.FontWeight = FontWeights.Medium;
                miOperand.IsEnabled = false;

                if (text == "+")
                {
                    operation = EOperation.Add;
                    //miOperand.Icon = new Image { Source = new BitmapImage(new Uri("Images/operator_add.png", UriKind.Relative)) };
                    miOperand.Icon = new Image { Source = MyUtils.doGetImageSourceFromResource("Resources/operator_add.png") };
                    //miOperand.Icon = Properties.Resources.operator_add;
                }
                else if (text == "-")
                {
                    operation = EOperation.Subtract;
                    //miOperand.Icon = new Image { Source = new BitmapImage(new Uri("Images/operator_sub.png", UriKind.Relative)) };
                    miOperand.Icon = new Image { Source = MyUtils.doGetImageSourceFromResource("Resources/operator_sub.png") };
                    //miOperand.Icon = Properties.Resources.operator_sub;
                }
                else if (text == "*")
                {
                    operation = EOperation.Multiply;
                    //miOperand.Icon = new Image { Source = new BitmapImage(new Uri("Images/operator_mult.png", UriKind.Relative)) };
                    miOperand.Icon = new Image { Source = MyUtils.doGetImageSourceFromResource("Resources/operator_mult.png") };
                    //miOperand.Icon = Properties.Resources.operator_mult;
                }
                else if (text == "/")
                {
                    operation = EOperation.Divide;
                    //miOperand.Icon = new Image { Source = new BitmapImage(new Uri("Images/operator_div.png", UriKind.Relative)) };
                    miOperand.Icon = new Image { Source = MyUtils.doGetImageSourceFromResource("Resources/operator_div.png") };
                    //miOperand.Icon = Properties.Resources.operator_div;
                }

                miResult = new MenuItem();
                miResult.Header = "";
                //miResult.Icon = new Image { Source = new BitmapImage(new Uri("Images/operator_equals.png", UriKind.Relative)) };
                miResult.Icon = new Image { Source = MyUtils.doGetImageSourceFromResource("Resources/operator_equals.png") };
                //miResult.Icon = Properties.Resources.operator_equals;
                miResult.FontSize = 18;
                miResult.FontWeight = FontWeights.Medium;
                miResult.IsEnabled = false;
                miResult.Click += OnResultClick;

                menuMath = new ContextMenu();
                menuMath.Items.Add(miOperand);
                menuMath.Items.Add(new Separator());
                menuMath.Items.Add(miResult);
                menuMath.PlacementTarget = this;
                menuMath.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuMath.PreviewKeyDown += OnMathPreviewKeyDown;
                menuMath.Closed += OnMathClosed;
                menuMath.IsOpen = true;
                return true;
            }

            return false;
        }

        private void OnMathPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                return;

            string operand = miOperand.Header.ToString();
            //System.Diagnostics.Debug.WriteLine(string.Format("OnMathPreviewKeyDown {0}", e.Key.ToString()));
            switch (e.Key)
            {
                case Key.Cancel:
                case Key.Clear:
                case Key.Escape:
                case Key.OemClear:
                    miResult.Header = "";
                    OnResultClick(this, null);
                    break;

                case Key.Back:
                case Key.Delete:
                    if (operand.Length > 0)
                        UpdateResult(operand.Substring(0, operand.Length - 1));
                    else
                        OnResultClick(this, null);
                    break;

                case Key.LineFeed:
                case Key.Enter:
                    OnResultClick(this, null);
                    break;

                case Key.OemPlus:  // '='
                    OnResultClick(this, null);
                    break;

                case Key.Subtract:
                case Key.OemMinus:
                    if (operand.Length == 0)
                        miOperand.Header = "-";
                    break;

                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    char key1 = (char)((e.Key - Key.D0) + '0');
                    if (char.IsDigit(key1))
                        UpdateResult(operand + key1);
                    break;

                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    char key2 = (char)((e.Key - Key.NumPad0) + '0');
                    if (char.IsDigit(key2))
                        UpdateResult(operand + key2);
                    break;

                case Key.OemComma:
                    if (!operand.Contains(","))
                        //miOperand.Header = operand + ",";
                        miOperand.Header = operand + Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    break;

                case Key.Decimal:
                case Key.OemPeriod:
                    if (!operand.Contains("."))
                        //miOperand.Header = operand + ".";
                        miOperand.Header = operand + Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    break;

                    //case Key.Multiply:
                    //case Key.Add:
                    //case Key.Divide:
            }
        }

        private void UpdateResult(string operand)
        {
            miOperand.Header = operand;
            double dop1, dop2, result;
            if (double.TryParse(Text, out dop1) && double.TryParse(operand, out dop2))
            {
                switch (operation)
                {
                    case EOperation.Add:
                        result = dop1 + dop2;
                        miResult.Header = result.ToString();
                        miResult.IsEnabled = true;
                        break;

                    case EOperation.Subtract:
                        result = dop1 - dop2;
                        miResult.Header = result.ToString();
                        miResult.IsEnabled = true;
                        break;

                    case EOperation.Multiply:
                        result = dop1 * dop2;
                        miResult.Header = result.ToString();
                        miResult.IsEnabled = true;
                        break;

                    case EOperation.Divide:
                        if (dop2 == 0.0)
                        {
                            miResult.Header = "";
                            miResult.IsEnabled = false;
                        }
                        else
                        {
                            result = dop1 / dop2;
                            if (double.IsInfinity(result) || double.IsNaN(result))
                            {
                                miResult.Header = "";
                                miResult.IsEnabled = false;
                            }
                            else
                            {
                                miResult.Header = result.ToString();
                                miResult.IsEnabled = true;
                            }
                        }
                        break;
                }
            }
            else
            {
                miResult.Header = "";
                miResult.IsEnabled = false;
            }
        }

        private void OnResultClick(object sender, RoutedEventArgs e)
        {
            string result = miResult.Header.ToString();
            if (result.Length > 0)
            {
                Text = result;
                CaretIndex = Text.Length;
            }

            menuMath.IsOpen = false;
        }

        private void OnMathClosed(object sender, RoutedEventArgs e)
        {
            menuMath.PreviewKeyDown -= OnMathPreviewKeyDown;
            menuMath.Closed -= OnMathClosed;
            miResult.Click -= OnResultClick;

            menuMath = null;
            miOperand = null;
            miResult = null;
        }

        #endregion Math
    }

    public class NumberTextBoxEditFinishedEventArgs
    {
        public bool isEditOk { get; set; }
        public bool isLostFocus { get; set; }
    }

}
