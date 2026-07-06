using System.Collections.Generic;
using TaskMaster.Models;

namespace TaskMaster.Services;

public sealed class SpecialVerbRepository : ISpecialVerbRepository
{
    private static readonly Dictionary<string, SpecialVerbDefinition> specialVerbs = new()
    {
        ["する"] = new SpecialVerbDefinition
        {
            Word = "する",
            VerbType = VerbType.Suru
        },
        ["来る"] = new SpecialVerbDefinition
        {
            Word = "来る",
            VerbType = VerbType.Kuru
        },
        ["ある"] = new SpecialVerbDefinition
        {
            Word = "ある",
            VerbType = VerbType.Godan
        },
        ["いらっしゃる"] = new SpecialVerbDefinition
        {
            Word = "いらっしゃる",
            VerbType = VerbType.Godan
        },
        ["おっしゃる"] = new SpecialVerbDefinition
        {
            Word = "おっしゃる",
            VerbType = VerbType.Godan
        },
        ["くださる"] = new SpecialVerbDefinition
        {
            Word = "くださる",
            VerbType = VerbType.Godan
        },
        ["帰る"] = new SpecialVerbDefinition
        {
            Word = "帰る",
            VerbType = VerbType.Godan
        },
        ["入る"] = new SpecialVerbDefinition
        {
            Word = "入る",
            VerbType = VerbType.Godan
        },
        ["走る"] = new SpecialVerbDefinition
        {
            Word = "走る",
            VerbType = VerbType.Godan
        },
        ["知る"] = new SpecialVerbDefinition
        {
            Word = "知る",
            VerbType = VerbType.Godan
        },
        ["要る"] = new SpecialVerbDefinition
        {
            Word = "要る",
            VerbType = VerbType.Godan
        },
        ["切る"] = new SpecialVerbDefinition
        {
            Word = "切る",
            VerbType = VerbType.Godan
        }
    };

    public bool TryGet(string word, out SpecialVerbDefinition definition)
    {
        return specialVerbs.TryGetValue(word, out definition!);
    }
}
