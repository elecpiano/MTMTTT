using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Shared.Control
{
    public class ContentButton : ContentControl
    {
        #region Property

        #endregion

        protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            VisualStateManager.GoToState(this, "Pressed", false);
        }

        protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnPointerReleased(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

    }
}

/*

public class ContentButton : ContentControl
{
    #region Property

    Storyboard sbNormal = null;
    Storyboard sbPressed = null;
    DoubleAnimation daPressedScaleX = null;
    DoubleAnimation daPressedScaleY = null;

    //PressedScaleX
    public double PressedScaleX
    {
        get { return (double)GetValue(PressedScaleXProperty); }
        set
        {
            SetValue(PressedScaleXProperty, value);
        }
    }

    public static readonly DependencyProperty PressedScaleXProperty =
        DependencyProperty.Register("PressedScaleX", typeof(double), typeof(ContentButton), new PropertyMetadata(0.95d));

    //PressedScaleY
    public double PressedScaleY
    {
        get { return (double)GetValue(PressedScaleYProperty); }
        set
        {
            SetValue(PressedScaleYProperty, value);
        }
    }

    public static readonly DependencyProperty PressedScaleYProperty =
        DependencyProperty.Register("PressedScaleY", typeof(double), typeof(ContentButton), new PropertyMetadata(0.95d));

    #endregion

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        sbNormal = GetTemplateChild("sbNormal") as Storyboard;
        sbPressed = GetTemplateChild("sbPressed") as Storyboard;
        daPressedScaleX = GetTemplateChild("daPressedScaleX") as DoubleAnimation;
        daPressedScaleX.To = PressedScaleX;
        daPressedScaleY = GetTemplateChild("daPressedScaleY") as DoubleAnimation;
        daPressedScaleY.To = PressedScaleY;
    }

    protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);
        //            VisualStateManager.GoToState(this, "Pressed", false);
        sbPressed.Begin();
    }

    protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        //VisualStateManager.GoToState(this, "Normal", true);
        sbNormal.Begin();
    }

    protected override void OnPointerReleased(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);
        //VisualStateManager.GoToState(this, "Normal", true);
        sbNormal.Begin();
    }

}
 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
  <Style TargetType="sharedControl:ContentButton">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="sharedControl:ContentButton">
                    <Grid Background="#00000000">
                        <Grid.Resources>
                            <Storyboard x:Name="sbNormal" Duration="0:0:0.2">
                                <DoubleAnimation Duration="0" To="1" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.ScaleX)" Storyboard.TargetName="content" />
                                <DoubleAnimation Duration="0" To="1" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.ScaleY)" Storyboard.TargetName="content" />
                                <DoubleAnimation Duration="0" To="1" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="content"/>
                            </Storyboard>
                            <Storyboard x:Name="sbPressed" Duration="0:0:0.2">
                                <DoubleAnimation x:Name="daPressedScaleX" Duration="0" To="0.95" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.ScaleX)" Storyboard.TargetName="content" />
                                <DoubleAnimation x:Name="daPressedScaleY" Duration="0" To="0.95" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.ScaleY)" Storyboard.TargetName="content" />
                                <DoubleAnimation Duration="0" To="0.8" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="content"/>
                            </Storyboard>
                        </Grid.Resources>
                        <VisualStateManager.VisualStateGroups>
                            <VisualStateGroup x:Name="VisualStates">
                                <VisualStateGroup.Transitions>
                                    <VisualTransition GeneratedDuration="0:0:0.1"/>
                                </VisualStateGroup.Transitions>
                                <VisualState x:Name="Normal"/>
                                <VisualState x:Name="Pressed">
                                    <Storyboard>
                                        <DoubleAnimation Duration="0" To="{TemplateBinding PressedScaleX}" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.ScaleX)" Storyboard.TargetName="content" />
                                        <DoubleAnimation Duration="0" To="{TemplateBinding PressedScaleY}" Storyboard.TargetProperty="(UIElement.RenderTransform).(CompositeTransform.ScaleY)" Storyboard.TargetName="content" />
                                        <DoubleAnimation Duration="0" To="0.8" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="content"/>
                                    </Storyboard>
                                </VisualState>
                            </VisualStateGroup>
                        </VisualStateManager.VisualStateGroups>

                        <ContentPresenter x:Name="content" Content="{TemplateBinding Content}" RenderTransformOrigin="0.5,0.5">
                            <ContentPresenter.RenderTransform>
                                <CompositeTransform/>
                            </ContentPresenter.RenderTransform>
                        </ContentPresenter>
                    </Grid>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

 */
