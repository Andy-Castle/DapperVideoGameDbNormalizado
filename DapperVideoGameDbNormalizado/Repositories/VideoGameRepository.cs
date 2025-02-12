using DapperVideoGameDbNormalizado.Models;

namespace DapperVideoGameDbNormalizado.Repositories
{
    public class VideoGameRepository : IVideoGameRepository
    {
        private readonly string _connectionString;

        public VideoGameRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("");
        }
        public Task<int> CreateVideoGameAsync(VideoGame videoGame)
        {
            throw new NotImplementedException();
        }

        public Task DeleteVideoGameAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VideoGame>> GetAllVideoGamesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<VideoGame> GetVideoGameAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateVideoGameAsyncs(VideoGame videoGame)
        {
            throw new NotImplementedException();
        }
    }
}
