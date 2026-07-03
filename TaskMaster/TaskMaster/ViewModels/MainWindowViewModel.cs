using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TaskMaster.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public event Action<string>? NavigateRequested;

    private string selectedPageTag = "SamplePage1";
    public string SelectedPageTag
    {
        get => selectedPageTag;
        set
        {
            if (SetProperty(ref selectedPageTag, value))
            {
                NavigateRequested?.Invoke(value);
            }
        }
    }

    private bool isMaximized;
    public bool IsMaximized
    {
        get => isMaximized;
        set
        {
            if (SetProperty(ref isMaximized, value))
            {
                OnPropertyChanged(nameof(MaximizeGlyph));
                OnPropertyChanged(nameof(MaximizeToolTip));
            }
        }
    }

    public string MaximizeGlyph => IsMaximized ? "\uE923" : "\uE922";

    public string MaximizeToolTip => IsMaximized ? "恢复" : "最大化";
}
