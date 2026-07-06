using TaskMaster.Models;

namespace TaskMaster.Services;

public interface IVerbClassifier
{
    VerbType Classify(string word);
}
