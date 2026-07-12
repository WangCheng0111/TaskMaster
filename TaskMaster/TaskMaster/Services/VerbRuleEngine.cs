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
            _ => BuildUnavailableForms()
        };
    }

    private static List<VerbConjugationItem> BuildUnavailableForms()
    {
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = "无" },
            new() { FormName = "て形", FormValue = "无" },
            new() { FormName = "た形", FormValue = "无" },
            new() { FormName = "ない形", FormValue = "无" },
            new() { FormName = "仮定形", FormValue = "无" },
            new() { FormName = "禁止形", FormValue = "无" },
            new() { FormName = "使役形", FormValue = "无" },
            new() { FormName = "使役受身形", FormValue = "无" },
            new() { FormName = "受身形", FormValue = "无" },
            new() { FormName = "可能形", FormValue = "无" },
            new() { FormName = "意志形", FormValue = "无" }
        };
    }

    private static List<VerbConjugationItem> BuildIchidanForms(string word)
    {
        var stem = word.EndsWith("る") && word.Length > 1 ? word[..^1] : word;
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = stem + "ます" },
            new() { FormName = "て形", FormValue = stem + "て" },
            new() { FormName = "た形", FormValue = stem + "た" },
            new() { FormName = "ない形", FormValue = stem + "ない" },
            new() { FormName = "仮定形", FormValue = stem + "れば" },
            new() { FormName = "禁止形", FormValue = stem + "るな" },
            new() { FormName = "使役形", FormValue = stem + "させる" },
            new() { FormName = "使役受身形", FormValue = stem + "させられる" },
            new() { FormName = "受身形", FormValue = stem + "られる" },
            new() { FormName = "可能形", FormValue = stem + "られる" },
            new() { FormName = "意志形", FormValue = stem + "よう" }
        };
    }

    private static List<VerbConjugationItem> BuildSuruForms()
    {
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = "します" },
            new() { FormName = "て形", FormValue = "して" },
            new() { FormName = "た形", FormValue = "した" },
            new() { FormName = "ない形", FormValue = "しない" },
            new() { FormName = "仮定形", FormValue = "すれば" },
            new() { FormName = "禁止形", FormValue = "するな" },
            new() { FormName = "使役形", FormValue = "させる" },
            new() { FormName = "使役受身形", FormValue = "させられる" },
            new() { FormName = "受身形", FormValue = "される" },
            new() { FormName = "可能形", FormValue = "できる" },
            new() { FormName = "意志形", FormValue = "しよう" }
        };
    }

    private static List<VerbConjugationItem> BuildKuruForms()
    {
        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = "来ます" },
            new() { FormName = "て形", FormValue = "来て" },
            new() { FormName = "た形", FormValue = "来た" },
            new() { FormName = "ない形", FormValue = "来ない" },
            new() { FormName = "仮定形", FormValue = "来れば" },
            new() { FormName = "禁止形", FormValue = "来るな" },
            new() { FormName = "使役形", FormValue = "来させる" },
            new() { FormName = "使役受身形", FormValue = "来させられる" },
            new() { FormName = "受身形", FormValue = "来られる" },
            new() { FormName = "可能形", FormValue = "来られる" },
            new() { FormName = "意志形", FormValue = "来よう" }
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

        return new List<VerbConjugationItem>
        {
            new() { FormName = "ます形", FormValue = masuStem + "ます" },
            new() { FormName = "て形", FormValue = teForm },
            new() { FormName = "た形", FormValue = taForm },
            new() { FormName = "ない形", FormValue = aStem + "ない" },
            new() { FormName = "仮定形", FormValue = eStem + "ば" },
            new() { FormName = "禁止形", FormValue = word + "な" },
            new() { FormName = "使役形", FormValue = aStem + "せる" },
            new() { FormName = "使役受身形", FormValue = aStem + "せられる" },
            new() { FormName = "受身形", FormValue = aStem + "れる" },
            new() { FormName = "可能形", FormValue = eStem + "る" },
            new() { FormName = "意志形", FormValue = oStem + "う" }
        };
    }
}
