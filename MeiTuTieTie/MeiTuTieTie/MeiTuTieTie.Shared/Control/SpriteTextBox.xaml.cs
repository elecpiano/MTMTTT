﻿using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Shared.Control
{
    public sealed partial class SpriteTextBox : UserControl
    {
        #region Property

        private bool _IsVirgin = true;
        private bool IsVirgin
        {
            get { return _IsVirgin; }
            set
            {
                _IsVirgin = value;
                virginTextBlock.Visibility = _IsVirgin ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static SolidColorBrush TransparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        private static SolidColorBrush EditBackgroundBrush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));

        public FontFamily Font
        {
            get { return textBox.FontFamily; }
            set
            {
                textBox.FontFamily = value;
                textBoxVisual.FontFamily = value;
            }
        }

        public Brush TextColor
        {
            get { return textBox.Foreground; }
            set
            {
                textBox.Foreground = value;
                textBoxVisual.Foreground = value;
            }
        }

        #endregion

        public SpriteTextBox()
        {
            this.InitializeComponent();
            this.textBox.LostFocus += textBox_LostFocus;
        }

        #region Mask

        private void mask_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!IsVirgin)
            {
                BeginEdit();
            }
        }

        private void mask_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsVirgin)
            {
                BeginEdit();
            }
        }

        #endregion

        #region Text Editing

        void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                IsVirgin = true;
            }
            else
            {
                IsVirgin = false;
            }
            rootGrid.Background = TransparentBrush;
            mask.IsHitTestVisible = true;
        }

        private void BeginEdit()
        {
            mask.IsHitTestVisible = false;
            virginTextBlock.Visibility = Visibility.Collapsed;
            this.textBox.Focus(FocusState.Programmatic);
            rootGrid.Background = EditBackgroundBrush;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxVisual.Text = textBox.Text;
            if (TextChanged != null)
            {
                TextChanged(this, e);
            }
        }

        #endregion

        #region Event

        public event TextChangedEventHandler TextChanged;

        #endregion
    }
}