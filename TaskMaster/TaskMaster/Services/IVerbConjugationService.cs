using TaskMaster.Models;

namespace TaskMaster.Services;

public interface IVerbConjugationService
{
    VerbConjugationResult GetForms(string input);
}
