using TaskMaster.Models;

namespace TaskMaster.Services;

public interface ISpecialVerbRepository
{
    bool TryGet(string word, out SpecialVerbDefinition definition);
}
