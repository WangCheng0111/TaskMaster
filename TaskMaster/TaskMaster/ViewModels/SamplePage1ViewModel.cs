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
        set => SetProperty(ref inputWord, value);
    }

    private string centerMessage = "请输入动词";
    public string CenterMessage
    {
        get => centerMessage;
        set => SetProperty(ref centerMessage, value);
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

    [RelayCommand]
    private void Search()
    {
        var input = InputWord.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            Forms.Clear();
            HasResults = false;
            CenterMessage = "请输入动词";
            CenterMessageVisibility = Visibility.Visible;
            return;
        }

        var verbType = verbClassifier.Classify(input);
        if (verbType == VerbType.Unknown)
        {
            Forms.Clear();
            HasResults = false;
            CenterMessage = "你输入的不是日语动词";
            CenterMessageVisibility = Visibility.Visible;
            return;
        }

        var result = verbConjugationService.GetForms(input);
        Forms = new ObservableCollection<VerbConjugationItem>(result.Forms);
        HasResults = Forms.Count > 0;
        CenterMessage = string.Empty;
        CenterMessageVisibility = Visibility.Collapsed;
    }
}
