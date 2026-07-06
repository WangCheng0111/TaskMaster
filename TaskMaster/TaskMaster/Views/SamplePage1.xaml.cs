using Microsoft.UI.Xaml.Controls;
using TaskMaster.ViewModels;

namespace TaskMaster.Views;

public sealed partial class SamplePage1 : Page
{
    public SamplePage1ViewModel ViewModel { get; } = new();

    public SamplePage1()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
