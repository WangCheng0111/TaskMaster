using System.Collections.Generic;
using TaskMaster.Models;

namespace TaskMaster.Services;

public sealed class VerbConjugationService : IVerbConjugationService
{
    private readonly ISpecialVerbRepository specialVerbRepository = new SpecialVerbRepository();
    private readonly IVerbClassifier verbClassifier = new VerbClassifier();
    private readonly IVerbRuleEngine verbRuleEngine = new VerbRuleEngine();

    public VerbConjugationResult GetForms(string input)
    {
        input = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return new VerbConjugationResult
            {
                InputWord = string.Empty,
                VerbType = "Unknown",
                Forms = new List<VerbConjugationItem>()
            };
        }

        if (specialVerbRepository.TryGet(input, out var specialDefinition))
        {
            return new VerbConjugationResult
            {
                InputWord = input,
                VerbType = specialDefinition.VerbType.ToString(),
                Forms = verbRuleEngine.BuildForms(input, specialDefinition.VerbType)
            };
        }

        var verbType = verbClassifier.Classify(input);
        return new VerbConjugationResult
        {
            InputWord = input,
            VerbType = verbType.ToString(),
            Forms = verbRuleEngine.BuildForms(input, verbType)
        };
    }
}
