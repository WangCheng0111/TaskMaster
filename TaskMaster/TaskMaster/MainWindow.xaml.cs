using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

namespace TaskMaster
{
    public sealed partial class MainWindow : Window
    {
        private sealed class ColorAnimationState
        {
            public Storyboard Storyboard { get; init; } = null!;
            public Windows.UI.Color From { get; init; }
            public Windows.UI.Color To { get; init; }
            public DateTimeOffset StartedAt { get; init; }
            public TimeSpan Duration { get; init; }
        }

        private readonly Dictionary<Border, Windows.UI.Color> _defaultBorderColors = new();
        private readonly Dictionary<Border, Windows.UI.Color> _hoverBorderColors = new();
        private readonly Dictionary<Border, Windows.UI.Color> _pressedBorderColors = new();
        private readonly Dictionary<FontIcon, Windows.UI.Color> _defaultIconColors = new();
        private readonly Dictionary<FontIcon, Windows.UI.Color> _hoverIconColors = new();
        private readonly Dictionary<FontIcon, Windows.UI.Color> _pressedIconColors = new();
        private readonly Dictionary<Border, ColorAnimationState> _activeBorderAnimations = new();
        private readonly Dictionary<FontIcon, ColorAnimationState> _activeIconAnimations = new();
        private readonly Dictionary<Border, FontIcon> _buttonIcons = new();
        private readonly HashSet<Border> _pressedBorders = new();
        private readonly HashSet<Border> _pressedOutsideBorders = new();
        private readonly Windows.UI.Color _inactiveIconColor = Windows.UI.Color.FromArgb(255, 160, 160, 160);
        private static readonly Windows.UI.Color DefaultIconFocusColor = Windows.UI.Color.FromArgb(255, 45, 45, 45);
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromSeconds(2);
        private bool _isWindowFocused = true;
        private readonly string _maximizeGlyph = "\uE922";
        private readonly string _restoreGlyph = "\uE923";

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
            AppWindow.Resize(new SizeInt32(800, 600));
            AppWindow.Changed += AppWindow_Changed;
            Activated += MainWindow_Activated;

            UpdateMaximizeGlyph();

            RegisterButtonVisuals(
                MinimizeButtonBorder,
                MinimizeIcon,
                hoverBorderColor: Windows.UI.Color.FromArgb(255, 217, 221, 226),
                pressedBorderColor: Windows.UI.Color.FromArgb(255, 194, 197, 202),
                hoverIconColor: Windows.UI.Color.FromArgb(255, 45, 45, 45),
                pressedIconColor: Windows.UI.Color.FromArgb(255, 45, 45, 45));

            RegisterButtonVisuals(
                MaximizeButtonBorder,
                MaximizeIcon,
                hoverBorderColor: Windows.UI.Color.FromArgb(255, 217, 221, 226),
                pressedBorderColor: Windows.UI.Color.FromArgb(255, 194, 197, 202),
                hoverIconColor: Windows.UI.Color.FromArgb(255, 45, 45, 45),
                pressedIconColor: Windows.UI.Color.FromArgb(255, 45, 45, 45));

            RegisterButtonVisuals(
                CloseButtonBorder,
                CloseIcon,
                hoverBorderColor: Windows.UI.Color.FromArgb(255, 232, 17, 35),
                pressedBorderColor: Windows.UI.Color.FromArgb(255, 236, 109, 122),
                hoverIconColor: Windows.UI.Color.FromArgb(255, 255, 255, 255),
                pressedIconColor: Windows.UI.Color.FromArgb(255, 255, 255, 255));
        }
        private void RegisterButtonVisuals(
            Border border,
            FontIcon icon,
            Windows.UI.Color hoverBorderColor,
            Windows.UI.Color pressedBorderColor,
            Windows.UI.Color hoverIconColor,
            Windows.UI.Color pressedIconColor)
        {
            _buttonIcons[border] = icon;
            RegisterBorderColors(border, hoverBorderColor, pressedBorderColor);
            RegisterIconColors(icon, hoverIconColor, pressedIconColor);
        }

        private void RegisterBorderColors(Border border, Windows.UI.Color hoverColor, Windows.UI.Color pressedColor)
        {
            if (border.Background is not SolidColorBrush brush)
            {
                return;
            }

            _defaultBorderColors[border] = brush.Color;
            _hoverBorderColors[border] = hoverColor;
            _pressedBorderColors[border] = pressedColor;
        }

        private void RegisterIconColors(FontIcon icon, Windows.UI.Color hoverColor, Windows.UI.Color pressedColor)
        {
            if (icon.Foreground is not SolidColorBrush brush)
            {
                return;
            }

            _defaultIconColors[icon] = brush.Color;
            _hoverIconColors[icon] = hoverColor;
            _pressedIconColors[icon] = pressedColor;
        }

        private void ButtonBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            if (_pressedBorders.Contains(border))
            {
                SetPressedInstant(border);
                return;
            }

            AnimateBorderToHover(border);
            SetButtonIconHoverState(border);
        }

        private void ButtonBorder_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border || !_pressedBorders.Contains(border))
            {
                return;
            }

            if (IsPointerInside(border, e))
            {
                if (_pressedOutsideBorders.Contains(border))
                {
                    _pressedOutsideBorders.Remove(border);
                    SetPressedInstant(border);
                }
                return;
            }

            _pressedOutsideBorders.Add(border);
            RestoreBorderImmediately(border);
            RestoreIconImmediatelyIfNeeded(border);
        }

        private void ButtonBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            if (_pressedBorders.Contains(border))
            {
                return;
            }

            AnimateBorderToDefault(border);
            SetButtonIconDefaultState(border);
        }

        private void ButtonBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            e.Handled = true;
            border.CapturePointer(e.Pointer);
            _pressedBorders.Add(border);
            _pressedOutsideBorders.Remove(border);
            SetPressedInstant(border);
        }

        private void ButtonBorder_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            e.Handled = true;
            var shouldInvoke = IsPointerInside(border, e);
            TryReleasePointer(border, e);
            ResetBorderInteractionState(border);

            if (shouldInvoke)
            {
                InvokeButtonAction(border);
            }
        }

        private void TryReleasePointer(Border border, PointerRoutedEventArgs e)
        {
            try
            {
                border.ReleasePointerCapture(e.Pointer);
            }
            catch
            {
                // Ignore capture release issues when the pointer is already gone.
            }
        }

        private void ResetBorderInteractionState(Border border)
        {
            _pressedBorders.Remove(border);
            _pressedOutsideBorders.Remove(border);
            RestoreBorderImmediately(border);
            RestoreIconImmediatelyIfNeeded(border);
            UpdateMaximizeGlyph();
        }

        private static bool IsPointerInside(Border border, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(border).Position;
            return point.X >= 0 && point.Y >= 0 && point.X <= border.ActualWidth && point.Y <= border.ActualHeight;
        }

        private void AnimateBorderToHover(Border border)
        {
            if (_hoverBorderColors.TryGetValue(border, out var hoverColor))
            {
                AnimateBorderBackground(border, hoverColor, animate: true);
            }

            if (TryGetButtonIcon(border, out var icon) && _hoverIconColors.TryGetValue(icon, out var hoverIconColor))
            {
                AnimateIconForeground(icon, hoverIconColor, animate: true);
            }
        }

        private void AnimateBorderToDefault(Border border)
        {
            if (_defaultBorderColors.TryGetValue(border, out var defaultColor))
            {
                AnimateBorderBackground(border, defaultColor, animate: true);
            }

            if (TryGetButtonIcon(border, out var icon) && _defaultIconColors.TryGetValue(icon, out var defaultIconColor))
            {
                AnimateIconForeground(icon, defaultIconColor, animate: true);
            }
        }

        private void SetPressedInstant(Border border)
        {
            StopBorderAnimation(border, freezeCurrent: true);

            if (_pressedBorderColors.TryGetValue(border, out var pressedColor))
            {
                border.Background = new SolidColorBrush(pressedColor);
            }

            if (TryGetButtonIcon(border, out var icon))
            {
                StopIconAnimation(icon, freezeCurrent: true);

                if (_pressedIconColors.TryGetValue(icon, out var pressedIconColor))
                {
                    icon.Foreground = new SolidColorBrush(pressedIconColor);
                }
            }
        }

        private void RestoreBorderImmediately(Border border)
        {
            StopBorderAnimation(border, freezeCurrent: true);

            if (_defaultBorderColors.TryGetValue(border, out var defaultColor))
            {
                border.Background = new SolidColorBrush(defaultColor);
            }
        }

        private void RestoreIconImmediatelyIfNeeded(Border border)
        {
            if (!TryGetButtonIcon(border, out var icon))
            {
                return;
            }

            StopIconAnimation(icon, freezeCurrent: true);

            if (_defaultIconColors.TryGetValue(icon, out var defaultIconColor))
            {
                icon.Foreground = new SolidColorBrush(defaultIconColor);
            }
        }

        private void InvokeButtonAction(Border border)
        {
            var presenter = GetPresenter();
            if (presenter is null)
            {
                return;
            }

            if (border == MinimizeButtonBorder)
            {
                presenter.Minimize();
                return;
            }

            if (border == MaximizeButtonBorder)
            {
                if (presenter.State == OverlappedPresenterState.Maximized)
                {
                    presenter.Restore();
                }
                else
                {
                    presenter.Maximize();
                }

                UpdateMaximizeGlyph();
                return;
            }

            if (border == CloseButtonBorder)
            {
                Close();
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            UpdateMaximizeGlyph();
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            _isWindowFocused = args.WindowActivationState != WindowActivationState.Deactivated;
            UpdateIconFocusState(_isWindowFocused);
        }

        private void UpdateMaximizeGlyph()
        {
            var presenter = GetPresenter();
            if (presenter is null || MaximizeIcon is null)
            {
                return;
            }

            var isMaximized = presenter.State == OverlappedPresenterState.Maximized;
            MaximizeIcon.Glyph = isMaximized ? _restoreGlyph : _maximizeGlyph;
            ToolTipService.SetToolTip(MaximizeButtonBorder, isMaximized ? "恢复" : "最大化");
        }

        private void UpdateIconFocusState(bool focused)
        {
            var targetColor = focused ? DefaultIconFocusColor : _inactiveIconColor;
            UpdateAllButtonIcons(targetColor);
        }

        private void SetButtonIconHoverState(Border border)
        {
            if (!TryGetButtonIcon(border, out var icon))
            {
                return;
            }

            if (_hoverIconColors.TryGetValue(icon, out var hoverIconColor))
            {
                AnimateIconForeground(icon, hoverIconColor, animate: true);
            }
        }

        private void SetButtonIconDefaultState(Border border)
        {
            if (!TryGetButtonIcon(border, out var icon))
            {
                return;
            }

            var targetColor = _isWindowFocused
                ? DefaultIconFocusColor
                : _inactiveIconColor;

            AnimateIconForeground(icon, targetColor, animate: true);
        }

        private void UpdateAllButtonIcons(Windows.UI.Color targetColor)
        {
            AnimateIconForeground(MinimizeIcon, targetColor, animate: true);
            AnimateIconForeground(MaximizeIcon, targetColor, animate: true);
            AnimateIconForeground(CloseIcon, targetColor, animate: true);
        }

        private OverlappedPresenter? GetPresenter()
            => AppWindow.Presenter as OverlappedPresenter;

        private bool TryGetButtonIcon(Border border, out FontIcon icon)
            => _buttonIcons.TryGetValue(border, out icon!);

        private void AnimateBorderBackground(Border border, Windows.UI.Color targetColor, bool animate)
        {
            if (border.Background is not SolidColorBrush brush)
            {
                brush = new SolidColorBrush(targetColor);
                border.Background = brush;
                return;
            }

            if (!animate)
            {
                StopBorderAnimation(border, freezeCurrent: false);
                brush.Color = targetColor;
                return;
            }

            var currentColor = GetCurrentBorderColor(border);
            if (ColorsEqual(currentColor, targetColor))
            {
                brush.Color = targetColor;
                StopBorderAnimation(border, freezeCurrent: false);
                return;
            }

            StartBorderAnimation(brush, border, currentColor, targetColor);
        }

        private void AnimateIconForeground(FontIcon icon, Windows.UI.Color targetColor, bool animate)
        {
            if (icon.Foreground is not SolidColorBrush brush)
            {
                brush = new SolidColorBrush(targetColor);
                icon.Foreground = brush;
                return;
            }

            if (!animate)
            {
                StopIconAnimation(icon, freezeCurrent: false);
                brush.Color = targetColor;
                return;
            }

            var currentColor = GetCurrentIconColor(icon);
            if (ColorsEqual(currentColor, targetColor))
            {
                brush.Color = targetColor;
                StopIconAnimation(icon, freezeCurrent: false);
                return;
            }

            StartIconAnimation(brush, icon, currentColor, targetColor);
        }

        private void StartBorderAnimation(SolidColorBrush brush, Border border, Windows.UI.Color fromColor, Windows.UI.Color toColor)
        {
            StopBorderAnimation(border, freezeCurrent: false);

            var storyboard = new Storyboard();
            var animation = new ColorAnimation
            {
                Duration = AnimationDuration,
                From = fromColor,
                To = toColor,
                EnableDependentAnimation = true,
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            Storyboard.SetTarget(animation, brush);
            Storyboard.SetTargetProperty(animation, "Color");
            storyboard.Children.Add(animation);

            _activeBorderAnimations[border] = new ColorAnimationState
            {
                Storyboard = storyboard,
                From = fromColor,
                To = toColor,
                StartedAt = DateTimeOffset.Now,
                Duration = AnimationDuration
            };

            storyboard.Completed += (_, _) =>
            {
                if (_activeBorderAnimations.TryGetValue(border, out var state) && ReferenceEquals(state.Storyboard, storyboard))
                {
                    _activeBorderAnimations.Remove(border);
                }
            };

            storyboard.Begin();
        }

        private void StartIconAnimation(SolidColorBrush brush, FontIcon icon, Windows.UI.Color fromColor, Windows.UI.Color toColor)
        {
            StopIconAnimation(icon, freezeCurrent: false);

            var storyboard = new Storyboard();
            var animation = new ColorAnimation
            {
                Duration = AnimationDuration,
                From = fromColor,
                To = toColor,
                EnableDependentAnimation = true,
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            Storyboard.SetTarget(animation, brush);
            Storyboard.SetTargetProperty(animation, "Color");
            storyboard.Children.Add(animation);

            _activeIconAnimations[icon] = new ColorAnimationState
            {
                Storyboard = storyboard,
                From = fromColor,
                To = toColor,
                StartedAt = DateTimeOffset.Now,
                Duration = AnimationDuration
            };

            storyboard.Completed += (_, _) =>
            {
                if (_activeIconAnimations.TryGetValue(icon, out var state) && ReferenceEquals(state.Storyboard, storyboard))
                {
                    _activeIconAnimations.Remove(icon);
                }
            };

            storyboard.Begin();
        }

        private void StopBorderAnimation(Border border, bool freezeCurrent)
        {
            if (!_activeBorderAnimations.TryGetValue(border, out var state))
            {
                return;
            }

            if (freezeCurrent && border.Background is SolidColorBrush brush)
            {
                brush.Color = InterpolateColor(state.From, state.To, state.StartedAt, state.Duration);
            }

            state.Storyboard.Stop();
            _activeBorderAnimations.Remove(border);
        }

        private void StopIconAnimation(FontIcon icon, bool freezeCurrent)
        {
            if (!_activeIconAnimations.TryGetValue(icon, out var state))
            {
                return;
            }

            if (freezeCurrent && icon.Foreground is SolidColorBrush brush)
            {
                brush.Color = InterpolateColor(state.From, state.To, state.StartedAt, state.Duration);
            }

            state.Storyboard.Stop();
            _activeIconAnimations.Remove(icon);
        }

        private Windows.UI.Color GetCurrentBorderColor(Border border)
        {
            if (_activeBorderAnimations.TryGetValue(border, out var state))
            {
                return InterpolateColor(state.From, state.To, state.StartedAt, state.Duration);
            }

            return border.Background is SolidColorBrush brush ? brush.Color : Windows.UI.Color.FromArgb(0, 0, 0, 0);
        }

        private Windows.UI.Color GetCurrentIconColor(FontIcon icon)
        {
            if (_activeIconAnimations.TryGetValue(icon, out var state))
            {
                return InterpolateColor(state.From, state.To, state.StartedAt, state.Duration);
            }

            return icon.Foreground is SolidColorBrush brush ? brush.Color : Windows.UI.Color.FromArgb(0, 0, 0, 0);
        }

        private static Windows.UI.Color InterpolateColor(Windows.UI.Color from, Windows.UI.Color to, DateTimeOffset startedAt, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                return to;
            }

            var progress = (DateTimeOffset.Now - startedAt).TotalMilliseconds / duration.TotalMilliseconds;
            progress = Math.Clamp(progress, 0.0, 1.0);
            var eased = 1 - Math.Pow(1 - progress, 3);

            byte Lerp(byte a, byte b) => (byte)Math.Round(a + (b - a) * eased);
            return Windows.UI.Color.FromArgb(
                Lerp(from.A, to.A),
                Lerp(from.R, to.R),
                Lerp(from.G, to.G),
                Lerp(from.B, to.B));
        }

        private static bool ColorsEqual(Windows.UI.Color a, Windows.UI.Color b)
            => a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;

    }
}
