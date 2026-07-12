using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using TaskMaster.Models;
using TaskMaster.Services;

namespace TaskMaster.ViewModels;

public partial class SamplePage1ViewModel : ObservableObject
{
    private readonly IVerbConjugationService verbConjugationService = new VerbConjugationService();
    private readonly IVerbClassifier verbClassifier = new VerbClassifier();

    private string inputWord = string.Empty;
    public string InputWord
    {
        get => inputWord;
        set
        {
            if (SetProperty(ref inputWord, value))
            {
                OnPropertyChanged(nameof(HasInput));
            }
        }
    }

    public bool HasInput => !string.IsNullOrWhiteSpace(InputWord);

    private string centerTitle = "开始查询活用形式";
    public string CenterTitle
    {
        get => centerTitle;
        set => SetProperty(ref centerTitle, value);
    }

    private string centerSubtitle = "输入日语动词的辞書形，例如 書く、食べる、する、来る";
    public string CenterSubtitle
    {
        get => centerSubtitle;
        set => SetProperty(ref centerSubtitle, value);
    }

    private bool isError;
    public bool IsError
    {
        get => isError;
        set => SetProperty(ref isError, value);
    }

    private Visibility centerMessageVisibility = Visibility.Visible;
    public Visibility CenterMessageVisibility
    {
        get => centerMessageVisibility;
        set => SetProperty(ref centerMessageVisibility, value);
    }

    private ObservableCollection<VerbConjugationItem> forms = new();
    public ObservableCollection<VerbConjugationItem> Forms
    {
        get => forms;
        set => SetProperty(ref forms, value);
    }

    private bool hasResults;
    public bool HasResults
    {
        get => hasResults;
        set => SetProperty(ref hasResults, value);
    }

    private string verbTypeText = string.Empty;
    public string VerbTypeText
    {
        get => verbTypeText;
        set => SetProperty(ref verbTypeText, value);
    }

    private Visibility verbTypeBadgeVisibility = Visibility.Collapsed;
    public Visibility VerbTypeBadgeVisibility
    {
        get => verbTypeBadgeVisibility;
        set => SetProperty(ref verbTypeBadgeVisibility, value);
    }

    private string resultsCountText = string.Empty;
    public string ResultsCountText
    {
        get => resultsCountText;
        set => SetProperty(ref resultsCountText, value);
    }

    [RelayCommand]
    private void Search()
    {
        var input = InputWord.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            ResetToEmpty("开始查询活用形式", "输入日语动词的辞書形，例如 書く、食べる、する、来る");
            return;
        }

        var verbType = verbClassifier.Classify(input);
        if (verbType == VerbType.Unknown)
        {
            ResetToEmpty("无法识别为日语动词", "请检查输入是否为日语动词的辞書形");
            IsError = true;
            return;
        }

        var result = verbConjugationService.GetForms(input);
        Forms = new ObservableCollection<VerbConjugationItem>(result.Forms);
        HasResults = Forms.Count > 0;
        CenterTitle = string.Empty;
        CenterSubtitle = string.Empty;
        CenterMessageVisibility = Visibility.Collapsed;
        IsError = false;
        VerbTypeText = GetVerbTypeDisplay(verbType);
        VerbTypeBadgeVisibility = Visibility.Visible;
        ResultsCountText = $"· {Forms.Count} 项";
    }

    [RelayCommand]
    private void ClearInput()
    {
        InputWord = string.Empty;
    }

    private void ResetToEmpty(string title, string subtitle)
    {
        Forms.Clear();
        HasResults = false;
        CenterTitle = title;
        CenterSubtitle = subtitle;
        CenterMessageVisibility = Visibility.Visible;
        IsError = false;
        VerbTypeText = string.Empty;
        VerbTypeBadgeVisibility = Visibility.Collapsed;
        ResultsCountText = string.Empty;
    }

    private static string GetVerbTypeDisplay(VerbType verbType) => verbType switch
    {
        VerbType.Ichidan => "一段动词",
        VerbType.Godan => "五段动词",
        VerbType.Suru => "サ行变格",
        VerbType.Kuru => "カ行变格",
        _ => string.Empty
    };
}
