using System.Threading.Tasks;

namespace SDDev.Net.ContentRepository.Contracts
{
    public interface IContentRepository
    {
        Task<IStorageModel> Create(IContentItem item);

        Task Delete(IStorageModel item);

        Task<IContentItem> Get(IStorageModel model);
    }
}
