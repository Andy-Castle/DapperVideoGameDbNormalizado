using Dapper;
using DapperVideoGameDbNormalizado.Models;
using Microsoft.Data.SqlClient;

namespace DapperVideoGameDbNormalizado.Repositories
{
    public class VideoGameRepository : IVideoGameRepository
    {
        private readonly string _connectionString;

        public VideoGameRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IEnumerable<VideoGame>> GetAllVideoGamesAsync()
        {
            var sql = GetVideoGameSql(false);
            var videoGames = await QueryVideoGamesAsync(sql);
            return videoGames;
        }

        public async Task<VideoGame> GetVideoGameAsync(int id)
        {
            var sql = GetVideoGameSql(true);
            var videoGame = await QueryVideoGamesAsync(sql, new { Id = id});
            return videoGame.FirstOrDefault();

        }

        private string GetVideoGameSql(bool withWhereClause)
        {
            var sql = @"SELECT vg.*, p.*, d.*, gd.*, r.*, pf.* 
                        FROM VideoGames vg
                        LEFT JOIN Publishers p ON vg.PublisherId = p.Id 
                        LEFT JOIN Developers d ON vg.DeveloperId = d.Id
                        LEFT JOIN GameDetails gd ON vg.Id = gd.VideoGameId
                        LEFT JOIN Reviews r ON vg.Id = r.VideoGameId
                        LEFT JOIN VideoGamesPlatforms vgp ON vg.Id = vgp.VideoGameId
                        LEFT JOIN Platforms pf ON pf.Id = vgp.PlatformId";
            if (withWhereClause)
            {
                sql += " WHERE vg.Id = @Id";
            }
            return sql;

        }

        private async Task<IEnumerable<VideoGame>>QueryVideoGamesAsync(string sql, object? parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var videoGameDictionary = new Dictionary<int, VideoGame>();

                var games = await connection.QueryAsync<VideoGame, Publisher, Developer, GameDetail, Review, Platform, VideoGame>(
                    sql, (videoGame, publisher, developer, gameDetail, review, platform) =>
                    {
                        if (!videoGameDictionary.TryGetValue(videoGame.Id, out var currentGame))
                        {
                            currentGame = videoGame;
                            currentGame.Publisher = publisher;
                            currentGame.Developer = developer;
                            currentGame.GameDetail = gameDetail;
                            currentGame.Reviews = [];
                            currentGame.Platforms = [];
                            videoGameDictionary.Add(currentGame.Id, currentGame);
                        }

                        if (review is not null && !currentGame.Reviews.Any(r => r.Id == review.Id))
                        {
                            currentGame.Reviews.Add(review);
                        }

                        if (platform is not null && !currentGame.Platforms.Any(p => p.Id == platform.Id))
                        {
                            currentGame.Platforms.Add(platform);
                        }

                        return currentGame;


                    }, parameters, splitOn: "Id, Id, VideoGameId, Id, Id");

                return videoGameDictionary.Values;
            }
        }

        public  Task<int> CreateVideoGameAsync(VideoGame videoGame)
        {
            throw new NotImplementedException();

        }

        public Task UpdateVideoGameAsyncs(VideoGame videoGame)
        {
            throw new NotImplementedException();
        }

        public Task DeleteVideoGameAsync(int id)
        {
            throw new NotImplementedException();
        }
       
    }
}
