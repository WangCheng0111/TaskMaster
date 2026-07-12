using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI;
using TaskMaster.ViewModels;

namespace TaskMaster.Views;

public sealed partial class SamplePage1 : Page
{
    public SamplePage1ViewModel ViewModel { get; } = new();

    private DispatcherTimer? _copyFeedbackTimer;

    public SamplePage1()
    {
        InitializeComponent();
        DataContext = ViewModel;

        if (Resources["FeedbackFadeOutStoryboard"] is Storyboard fadeOut)
        {
            fadeOut.Completed += (_, _) => CopyFeedbackBorder.Visibility = Visibility.Collapsed;
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
        SearchBorder.BorderBrush = Resources["AccentGradientBrush"] as Brush;
    }

    private void InputBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SearchBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240));
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
            // Clipboard may be unavailable in some environments; ignore failures silently.
            return;
        }

        CopyFeedbackText.Text = $"已复制：{formValue}";
        CopyFeedbackBorder.Visibility = Visibility.Visible;

        if (Resources["FeedbackFadeInStoryboard"] is Storyboard fadeIn)
        {
            fadeIn.Begin();
        }

        _copyFeedbackTimer ??= CreateFeedbackTimer();
        _copyFeedbackTimer.Stop();
        _copyFeedbackTimer.Start();
    }

    private DispatcherTimer CreateFeedbackTimer()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.6) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (Resources["FeedbackFadeOutStoryboard"] is Storyboard fadeOut)
            {
                fadeOut.Begin();
            }
        };
        return timer;
    }
}
