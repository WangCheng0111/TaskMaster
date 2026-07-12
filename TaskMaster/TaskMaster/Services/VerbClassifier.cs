using TaskMaster.Models;

namespace TaskMaster.Services;

public sealed class VerbClassifier : IVerbClassifier
{
    private readonly ISpecialVerbRepository specialVerbRepository = new SpecialVerbRepository();

    public VerbType Classify(string word)
    {
        word = (word ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(word))
        {
            return VerbType.Unknown;
        }

        // 日語动词辞書形至少需要2个字符（詞幹 + 語尾）
        if (word.Length < 2)
        {
            return VerbType.Unknown;
        }

        if (specialVerbRepository.TryGet(word, out var definition))
        {
            return definition.VerbType;
        }

        if (!IsLikelyVerb(word))
        {
            return VerbType.Unknown;
        }

        if (word.EndsWith("する"))
        {
            return VerbType.Suru;
        }

        if (word.EndsWith("来る"))
        {
            return VerbType.Kuru;
        }

        if (!word.EndsWith("る"))
        {
            return VerbType.Godan;
        }

        return LooksLikeIchidan(word) ? VerbType.Ichidan : VerbType.Godan;
    }

    private static bool IsLikelyVerb(string word)
    {
        var last = word[^1];
        return last is 'う' or 'く' or 'ぐ' or 'す' or 'つ' or 'ぬ' or 'ぶ' or 'む' or 'る';
    }

    private static bool LooksLikeIchidan(string word)
    {
        if (word.Length < 2)
        {
            return false;
        }

        var beforeRu = word[^2];
        return beforeRu is 'い' or 'き' or 'ぎ' or 'し' or 'じ' or 'ち' or 'に' or 'ひ' or 'び' or 'み' or 'り'
            or 'え' or 'け' or 'げ' or 'せ' or 'ぜ' or 'て' or 'で' or 'ね' or 'へ' or 'べ' or 'め' or 'れ';
    }
}
