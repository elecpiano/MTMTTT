using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Shared.Control
{
    public sealed partial class EnhancedTextBox : UserControl
    {
        #region Properties

        #region Text

        public string Text
        {
            get
            {
                return this.mainTextBox.Text;
            }
            set
            {
                if (value != null)
                {
                    this.mainTextBox.Text = value;
                    OnTextChanged();
                }                
            }
        }

        public int MaxLength
        {
            get
            {
                return this.mainTextBox.MaxLength;
            }
            set
            {
                this.mainTextBox.MaxLength = value;
            }
        }

        //public string Text
        //{
        //    get
        //    {
        //        return this.mainTextBox.Text;
        //    }
        //    set { SetValue(TextProperty, value); }
        //}

        //public static readonly DependencyProperty TextProperty =
        //    DependencyProperty.Register("Text", typeof(string), typeof(EnhancedTextBox), new PropertyMetadata(TextWrapping.NoWrap, TextPropertyChanged));

        //private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var etb = d as EnhancedTextBox;
        //    if (etb != null)
        //    {
        //        string value = (string)e.NewValue;
        //        etb.mainTextBox.Text = value;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            etb.watermarkTextBlock.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //}

        #endregion

        //#region TextWrapping

        //public TextWrapping TextWrapping
        //{
        //    get { return (TextWrapping)GetValue(TextWrappingProperty); }
        //    set
        //    {
        //        SetValue(TextWrappingProperty, value);
        //        this.mainTextBox.TextWrapping = value;
        //        this.watermarkTextBlock.TextWrapping = value;
        //        this.messageTextBlock.TextWrapping = value;
        //    }
        //}

        //public static readonly DependencyProperty TextWrappingProperty =
        //    DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(EnhancedTextBox), new PropertyMetadata(TextWrapping.NoWrap));

        //#endregion

        #region InputStyle

        public Style InputStyle
        {
            get { return (Style)GetValue(InputStyleProperty); }
            set { SetValue(InputStyleProperty, value); }

            //set
            //{
            //    SetValue(InputStyleProperty, value);
            //    this.mainTextBox.Style = value;
            //}
        }

        public static readonly DependencyProperty InputStyleProperty =
            DependencyProperty.Register("InputStyle", typeof(Style), typeof(EnhancedTextBox), new PropertyMetadata(null, InputStylePropertyChanged));

        private static void InputStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var etb = d as EnhancedTextBox;
            if (etb != null)
            {
                Style style = (Style)e.NewValue;
                etb.mainTextBox.Style = style;
            }
        }

        #endregion

        #region WatermarkStyle

        public Style WatermarkStyle
        {
            get { return (Style)GetValue(WatermarkStyleProperty); }
            set { SetValue(WatermarkStyleProperty, value); }
        }

        public static readonly DependencyProperty WatermarkStyleProperty =
            DependencyProperty.Register("WatermarkStyle", typeof(Style), typeof(EnhancedTextBox), new PropertyMetadata(null, WatermarkStylePropertyChanged));

        private static void WatermarkStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var etb = d as EnhancedTextBox;
            if (etb != null)
            {
                Style style = (Style)e.NewValue;
                etb.watermarkTextBlock.Style = style;
            }
        }

        #endregion

        #region MessageStyle

        public Style MessageStyle
        {
            get { return (Style)GetValue(MessageStyleProperty); }
            set { SetValue(MessageStyleProperty, value); }
        }

        public static readonly DependencyProperty MessageStyleProperty =
            DependencyProperty.Register("MessageStyle", typeof(Style), typeof(EnhancedTextBox), new PropertyMetadata(null, MessageStylePropertyChanged));

        private static void MessageStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var etb = d as EnhancedTextBox;
            if (etb != null)
            {
                Style style = (Style)e.NewValue;
                etb.messageTextBlock.Style = style;
            }
        }

        #endregion

        #region HideKeyboardOnEnterPressed

        public bool HideKeyboardOnEnterPressed
        {
            get { return (bool)GetValue(HideKeyboardOnEnterPressedProperty); }
            set { SetValue(HideKeyboardOnEnterPressedProperty, value); }
        }

        public static readonly DependencyProperty HideKeyboardOnEnterPressedProperty =
            DependencyProperty.Register("HideKeyboardOnEnterPressed", typeof(bool), typeof(EnhancedTextBox), new PropertyMetadata(false));

        #endregion

        public TextBlock WatermarkTextBlock
        {
            get
            {
                return this.watermarkTextBlock;
            }
        }

        #endregion

        #region Properties

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set
            {
                SetValue(WatermarkProperty, value);
                this.watermarkTextBlock.Text = value;
            }
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(EnhancedTextBox), new PropertyMetadata(null));

        private bool ErrorMessageIsShowing = false;
        private bool HintSelected = false;

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set
            {
                SetValue(ErrorMessageProperty, value);
                this.messageTextBlock.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    this.watermarkTextBlock.Visibility = Visibility.Visible;
                    this.messageTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.watermarkTextBlock.Visibility = Visibility.Collapsed;
                    this.messageTextBlock.Visibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(this.mainTextBox.Text))
                    {
                        ErrorMessageIsShowing = true;
                        this.mainTextBox.Text = string.Empty;
                    }
                }
            }
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(EnhancedTextBox), new PropertyMetadata(null));

        private List<string> _emailPostfixes = new List<string>();

        private List<string> _emailList = new List<string>();

        #endregion

        #region Constructor

        public EnhancedTextBox()
        {
            this.InitializeComponent();
            this.SetValue(Canvas.ZIndexProperty, 999);
        }

        #endregion

        #region Typing

        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnTextChanged();
            if (TextChanged != null)
            {
                TextChanged(this, e);
            }
        }

        private void OnTextChanged()
        {
            if (ErrorMessageIsShowing)
            {
                ErrorMessageIsShowing = false;
                return;
            }

            if (HintSelected)
            {
                HintSelected = false;
                return;
            }

            if (string.IsNullOrEmpty(this.Text))
            {
                this.watermarkTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                this.watermarkTextBlock.Visibility = Visibility.Collapsed;
            }

            this.messageTextBlock.Visibility = Visibility.Collapsed;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //this.normalBackground.Visibility = Visibility.Collapsed;
            //this.focusBackground.Visibility = Visibility.Visible;
            this.mainTextBox.SelectionStart = this.mainTextBox.Text.Length;
            this.mainTextBox.SelectionLength = 0;

            //this.watermarkTextBlock.Visibility = Visibility.Collapsed;
            this.messageTextBlock.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //this.normalBackground.Visibility = Visibility.Visible;
            //this.focusBackground.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(this.mainTextBox.Text) && this.messageTextBlock.Visibility == Visibility.Collapsed)
            {
                this.watermarkTextBlock.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Event

        public new event KeyEventHandler KeyDown;
        private void mainTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (KeyDown != null)
            {
                KeyDown(this, e);
            }
        }

        public event TextChangedEventHandler TextChanged;

        #endregion

        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mainTextBox.Focus(FocusState.Keyboard);
        }

    }
}
