using System.Collections.Generic;
using TaskMaster.Models;

namespace TaskMaster.Services;

public interface IVerbRuleEngine
{
    List<VerbConjugationItem> BuildForms(string word, VerbType verbType);
}
