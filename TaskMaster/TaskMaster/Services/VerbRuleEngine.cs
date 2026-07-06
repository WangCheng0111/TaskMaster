using System.Collections.Generic;
using TaskMaster.Models;

namespace TaskMaster.Services;

public sealed class VerbRuleEngine : IVerbRuleEngine
{
    public List<VerbConjugationItem> BuildForms(string word, VerbType verbType)
    {
        word = (word ?? string.Empty).Trim();

        return verbType switch
        {
            VerbType.Suru => BuildSuruForms(),
            VerbType.Kuru => BuildKuruForms(),
            VerbType.Ichidan => BuildIchidanForms(word),
            VerbType.Godan => BuildGodanForms(word),
            _ => BuildUnknownForms()
        };
    }

    private static List<VerbConjugationItem> BuildUnknownForms()
    {
        return BuildUnavailableForms();
    }

    private static List<VerbConjugationItem> BuildIchidanForms(string word)
    {
        var stem = word.EndsWith("る") && word.Length > 1 ? word[..^1] : word;
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = stem + "ます", IsAvailable = true },
            new() { FormName = "て形", FormValue = stem + "て", IsAvailable = true },
            new() { FormName = "た形", FormValue = stem + "た", IsAvailable = true },
            new() { FormName = "ない形", FormValue = stem + "ない", IsAvailable = true },
            new() { FormName = "仮定形", FormValue = stem + "れば", IsAvailable = true },
            new() { FormName = "禁止形", FormValue = stem + "るな", IsAvailable = true },
            new() { FormName = "使役形", FormValue = stem + "させる", IsAvailable = true },
            new() { FormName = "使役受身形", FormValue = stem + "させられる", IsAvailable = true },
            new() { FormName = "受身形", FormValue = stem + "られる", IsAvailable = true },
            new() { FormName = "可能形", FormValue = stem + "られる", IsAvailable = true },
            new() { FormName = "意志形", FormValue = stem + "よう", IsAvailable = true }
        };
    }

    private static List<VerbConjugationItem> BuildSuruForms()
    {
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = "します", IsAvailable = true },
            new() { FormName = "て形", FormValue = "して", IsAvailable = true },
            new() { FormName = "た形", FormValue = "した", IsAvailable = true },
            new() { FormName = "ない形", FormValue = "しない", IsAvailable = true },
            new() { FormName = "仮定形", FormValue = "すれば", IsAvailable = true },
            new() { FormName = "禁止形", FormValue = "するな", IsAvailable = true },
            new() { FormName = "使役形", FormValue = "させる", IsAvailable = true },
            new() { FormName = "使役受身形", FormValue = "させられる", IsAvailable = true },
            new() { FormName = "受身形", FormValue = "される", IsAvailable = true },
            new() { FormName = "可能形", FormValue = "できる", IsAvailable = true },
            new() { FormName = "意志形", FormValue = "しよう", IsAvailable = true }
        };
    }

    private static List<VerbConjugationItem> BuildKuruForms()
    {
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = "来ます", IsAvailable = true },
            new() { FormName = "て形", FormValue = "来て", IsAvailable = true },
            new() { FormName = "た形", FormValue = "来た", IsAvailable = true },
            new() { FormName = "ない形", FormValue = "来ない", IsAvailable = true },
            new() { FormName = "仮定形", FormValue = "来れば", IsAvailable = true },
            new() { FormName = "禁止形", FormValue = "来るな", IsAvailable = true },
            new() { FormName = "使役形", FormValue = "来させる", IsAvailable = true },
            new() { FormName = "使役受身形", FormValue = "来させられる", IsAvailable = true },
            new() { FormName = "受身形", FormValue = "来られる", IsAvailable = true },
            new() { FormName = "可能形", FormValue = "来られる", IsAvailable = true },
            new() { FormName = "意志形", FormValue = "来よう", IsAvailable = true }
        };
    }

    private static List<VerbConjugationItem> BuildGodanForms(string word)
    {
        if (string.IsNullOrWhiteSpace(word) || !word.EndsWith("う") && !word.EndsWith("く") && !word.EndsWith("ぐ") && !word.EndsWith("す") && !word.EndsWith("つ") && !word.EndsWith("ぬ") && !word.EndsWith("ぶ") && !word.EndsWith("む") && !word.EndsWith("る"))
        {
            return BuildUnavailableForms();
        }

        var stem = word[..^1];
        var last = word[^1];

        var masuStem = last switch
        {
            'う' => stem + "い",
            'く' => stem + "き",
            'ぐ' => stem + "ぎ",
            'す' => stem + "し",
            'つ' => stem + "ち",
            'ぬ' => stem + "に",
            'ぶ' => stem + "び",
            'む' => stem + "み",
            'る' => stem + "り",
            _ => stem
        };

        var aStem = last switch
        {
            'う' => stem + "わ",
            'く' => stem + "か",
            'ぐ' => stem + "が",
            'す' => stem + "さ",
            'つ' => stem + "た",
            'ぬ' => stem + "な",
            'ぶ' => stem + "ば",
            'む' => stem + "ま",
            'る' => stem + "ら",
            _ => stem
        };

        var eStem = last switch
        {
            'う' => stem + "え",
            'く' => stem + "け",
            'ぐ' => stem + "げ",
            'す' => stem + "せ",
            'つ' => stem + "て",
            'ぬ' => stem + "ね",
            'ぶ' => stem + "べ",
            'む' => stem + "め",
            'る' => stem + "れ",
            _ => stem
        };

        var oStem = last switch
        {
            'う' => stem + "お",
            'く' => stem + "こ",
            'ぐ' => stem + "ご",
            'す' => stem + "そ",
            'つ' => stem + "と",
            'ぬ' => stem + "の",
            'ぶ' => stem + "ぼ",
            'む' => stem + "も",
            'る' => stem + "ろ",
            _ => stem
        };

        var teForm = last switch
        {
            'う' or 'つ' or 'る' => stem + "って",
            'む' or 'ぶ' or 'ぬ' => stem + "んで",
            'く' => stem + "いて",
            'ぐ' => stem + "いで",
            'す' => stem + "して",
            _ => "无"
        };

        var taForm = last switch
        {
            'う' or 'つ' or 'る' => stem + "った",
            'む' or 'ぶ' or 'ぬ' => stem + "んだ",
            'く' => stem + "いた",
            'ぐ' => stem + "いだ",
            'す' => stem + "した",
            _ => "无"
        };

        var canTeOrTa = teForm != "无" && taForm != "无";

        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = masuStem + "ます", IsAvailable = true },
            new() { FormName = "て形", FormValue = canTeOrTa ? teForm : "无", IsAvailable = canTeOrTa },
            new() { FormName = "た形", FormValue = canTeOrTa ? taForm : "无", IsAvailable = canTeOrTa },
            new() { FormName = "ない形", FormValue = aStem + "ない", IsAvailable = true },
            new() { FormName = "仮定形", FormValue = eStem + "ば", IsAvailable = true },
            new() { FormName = "禁止形", FormValue = word + "な", IsAvailable = true },
            new() { FormName = "使役形", FormValue = aStem + "せる", IsAvailable = true },
            new() { FormName = "使役受身形", FormValue = aStem + "せられる", IsAvailable = true },
            new() { FormName = "受身形", FormValue = aStem + "れる", IsAvailable = true },
            new() { FormName = "可能形", FormValue = eStem + "る", IsAvailable = true },
            new() { FormName = "意志形", FormValue = oStem + "う", IsAvailable = true }
        };
    }

    private static List<VerbConjugationItem> BuildUnavailableForms()
    {
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = "无", IsAvailable = false },
            new() { FormName = "て形", FormValue = "无", IsAvailable = false },
            new() { FormName = "た形", FormValue = "无", IsAvailable = false },
            new() { FormName = "ない形", FormValue = "无", IsAvailable = false },
            new() { FormName = "仮定形", FormValue = "无", IsAvailable = false },
            new() { FormName = "禁止形", FormValue = "无", IsAvailable = false },
            new() { FormName = "使役形", FormValue = "无", IsAvailable = false },
            new() { FormName = "使役受身形", FormValue = "无", IsAvailable = false },
            new() { FormName = "受身形", FormValue = "无", IsAvailable = false },
            new() { FormName = "可能形", FormValue = "无", IsAvailable = false },
            new() { FormName = "意志形", FormValue = "无", IsAvailable = false }
        };
    }
}
