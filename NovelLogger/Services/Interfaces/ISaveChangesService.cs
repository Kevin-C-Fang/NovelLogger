using NovelLogger.Services.Implementations;

namespace NovelLogger.Services.Interfaces
{
    public interface ISaveChangesService
    {
        ServiceResult TrySave();
    }
}
