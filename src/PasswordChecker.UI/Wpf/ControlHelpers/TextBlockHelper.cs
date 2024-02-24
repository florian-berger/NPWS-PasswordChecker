using System.Windows.Controls;
using System.Windows;

namespace PasswordChecker.UI.Wpf.ControlHelpers
{
    /// <summary>
    ///     Class that provides utilities for <see cref="TextBlock" /> elements.
    ///     Source: https://blog.scottlogic.com/2011/01/31/automatically-showing-tooltips-on-a-trimmed-textblock-silverlight-wpf.html
    /// </summary>
    internal class TextBlockUtils
    {
        #region Attached properties

        /// <summary>
        ///     Identified the attached AutoTooltip property. When true, this will set the TextBlock TextTrimming
        ///     property to WordEllipsis, and display a tooltip with the full text whenever the text is trimmed.
        /// </summary>
        public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached("AutoTooltip",
            typeof(bool), typeof(TextBlockUtils), new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        #endregion Attached properties

        #region Public methods

        /// <summary>
        ///     Gets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static bool GetAutoTooltip(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoTooltipProperty);
        }

        /// <summary>
        ///     Sets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static void SetAutoTooltip(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoTooltipProperty, value);
        }

        #endregion Public methods

        #region Private methods

        private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock textBlock))
            {
                return;
            }

            if (e.NewValue.Equals(true))
            {
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                ComputeAutoTooltip(textBlock);
                textBlock.SizeChanged += TextBlock_SizeChanged;
            }
            else
            {
                textBlock.SizeChanged -= TextBlock_SizeChanged;
            }
        }

        private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                ComputeAutoTooltip(textBlock);
            }
        }

        /// <summary>
        /// Assigns the ToolTip for the given TextBlock based on whether the text is trimmed
        /// </summary>
        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var width = textBlock.DesiredSize.Width;

            ToolTipService.SetToolTip(textBlock, textBlock.ActualWidth < width ? textBlock.Text : null);
        }

        #endregion Private methods
    }
}
