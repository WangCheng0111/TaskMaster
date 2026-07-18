using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using TaskMaster.ViewModels;

namespace TaskMaster.Views;

public sealed partial class SamplePage1 : Page
{
    public SamplePage1ViewModel ViewModel { get; } = new();

    private DispatcherTimer? _toastTimer;
    private readonly Dictionary<FrameworkElement, Storyboard> _cardStoryboards = new();

    public SamplePage1()
    {
        InitializeComponent();
        DataContext = ViewModel;

        if (Resources["ToastOutStoryboard"] is Storyboard fadeOut)
        {
            fadeOut.Completed += (_, _) => CopyFeedbackBorder.Visibility = Visibility.Collapsed;
        }
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Resources["PageEntranceStoryboard"] is Storyboard entrance)
        {
            entrance.Begin();
        }
    }

    private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ViewModel.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void InputBox_GotFocus(object sender, RoutedEventArgs e)
    {
        SearchBorder.BorderBrush = Resources["AccentBrush"] as Brush;
    }

    private void InputBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SearchBorder.BorderBrush = Resources["SeparatorBrush"] as Brush;
    }

    private void FormCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string formValue || string.IsNullOrWhiteSpace(formValue))
        {
            return;
        }

        try
        {
            var package = new DataPackage();
            package.SetText(formValue);
            Clipboard.SetContent(package);
        }
        catch
        {
            return;
        }

        CopyFeedbackText.Text = $"已复制：{formValue}";
        CopyFeedbackBorder.Visibility = Visibility.Visible;

        if (Resources["ToastInStoryboard"] is Storyboard toastIn)
        {
            toastIn.Begin();
        }

        _toastTimer ??= CreateToastTimer();
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void FormsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is Grid grid)
        {
            if (_cardStoryboards.TryGetValue(grid, out var prev) && prev is not null)
            {
                prev.Stop();
            }
            AnimateCardEntrance(grid, args.Index);
        }
    }

    private void FormsRepeater_ElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is Grid grid)
        {
            if (_cardStoryboards.TryGetValue(grid, out var sb) && sb is not null)
            {
                sb.Stop();
            }
            grid.Opacity = 0;
            if (grid.RenderTransform is TranslateTransform t)
            {
                t.Y = 14;
            }
        }
    }

    private void AnimateCardEntrance(FrameworkElement element, int index)
    {
        if (element.RenderTransform is not TranslateTransform translate)
        {
            return;
        }

        element.Opacity = 0;
        translate.Y = 14;

        var storyboard = new Storyboard();
        var easing = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.5 };
        var delay = TimeSpan.FromMilliseconds(Math.Min(index * 35, 420));

        var opacityAnim = new DoubleAnimation
        {
            To = 1,
            Duration = TimeSpan.FromMilliseconds(420),
            BeginTime = delay,
            EasingFunction = easing
        };
        Storyboard.SetTarget(opacityAnim, element);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");
        storyboard.Children.Add(opacityAnim);

        var yAnim = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(520),
            BeginTime = delay,
            EasingFunction = easing
        };
        Storyboard.SetTarget(yAnim, translate);
        Storyboard.SetTargetProperty(yAnim, "Y");
        storyboard.Children.Add(yAnim);

        _cardStoryboards[element] = storyboard;
        storyboard.Begin();
    }

    private DispatcherTimer CreateToastTimer()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.6) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (Resources["ToastOutStoryboard"] is Storyboard fadeOut)
            {
                fadeOut.Begin();
            }
        };
        return timer;
    }
}
