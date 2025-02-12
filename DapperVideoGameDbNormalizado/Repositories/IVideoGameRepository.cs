using DapperVideoGameDbNormalizado.Models;

namespace DapperVideoGameDbNormalizado.Repositories
{
    public interface IVideoGameRepository
    {
        Task<int> CreateVideoGameAsync(VideoGame videoGame);

        Task<VideoGame> GetVideoGameAsync(int id);

        Task<IEnumerable<VideoGame>> GetAllVideoGamesAsync();

        Task UpdateVideoGameAsyncs(VideoGame videoGame);

        Task DeleteVideoGameAsync(int id);
    }
}
