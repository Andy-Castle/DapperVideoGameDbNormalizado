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

        public  async Task<int> CreateVideoGameAsync(VideoGame videoGame)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {

                        int publisherId = await GetOrCreatePublisherAsync(connection, transaction, videoGame.Publisher.Name);

                        int developerId = await GetOrCreateDeveloperAsync(connection, transaction, videoGame.Developer.Name);

                        string sql = @"INSERT INTO VideoGames (Title, PublisherId, DeveloperId, ReleaseDate) 
                                      VALUES (@Title, @PublisherId, @DeveloperId, @ReleaseDate); 
                                      SELECT CAST(SCOPE_IDENTITY() as int);";

                        var id = await connection.QuerySingleAsync<int>(sql, new
                        {
                            videoGame.Title,
                            PublisherId = publisherId,
                            DeveloperId = developerId,
                            videoGame.ReleaseDate
                        }, transaction);

                        videoGame.Id = id;

                        if (videoGame.GameDetail != null)
                        {
                            videoGame.GameDetail.VideoGameId = id;
                            await CreateGameDetailAsync(connection, videoGame.GameDetail, transaction);
                        }

                        if (videoGame.Reviews != null)
                        {
                            foreach (var review in videoGame.Reviews)
                            {
                                review.VideoGameId = id;
                                await CreateReviewAsync(connection, review, transaction);
                            }
                        }

                        if (videoGame.Platforms != null)
                        {
                            foreach (var platform in videoGame.Platforms)
                            {
    
                                await CreateVideoGamePlatformAsync(connection, new VideoGamePlatform
                                {
                                    VideoGameId = id,
                                    PlatformId = platform.Id

                                }, transaction);
                            }
                        }

                        transaction.Commit();

                        return id;


                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;

                    }
                }
            }

        }

        private async Task<int> GetOrCreatePublisherAsync(SqlConnection connection, SqlTransaction transaction, string publisherName)
        {
            string checkSql = "SELECT Id FROM Publishers WHERE Name = @Name";
            var existingPublisherId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { Name = publisherName }, transaction);

            if (existingPublisherId.HasValue)
            {
                return existingPublisherId.Value;
            }

            string insertSql = @"INSERT INTO Publishers (Name) VALUES (@Name); 
                                SELECT CAST(SCOPE_IDENTITY() as int) ";

            var newPublisherId = await connection.QuerySingleAsync<int>(insertSql, new { Name = publisherName }, transaction);

            return newPublisherId;
        }

        private async Task<int> GetOrCreateDeveloperAsync(SqlConnection connection, SqlTransaction transaction, string developerName)
        {
            string checkSql = "SELECT Id FROM Developers WHERE Name = @Name";
            var existingDeveloperId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { Name = developerName }, transaction);
            if (existingDeveloperId.HasValue)
            {
                return existingDeveloperId.Value;
            }
            string insertSql = @"INSERT INTO Developers (Name) VALUES (@Name); 
                                SELECT CAST(SCOPE_IDENTITY() as int) ";
            var newDeveloperId = await connection.QuerySingleAsync<int>(insertSql, new { Name = developerName }, transaction);
            return newDeveloperId;
        }

        private async Task CreateGameDetailAsync(SqlConnection connection, GameDetail gameDetail, SqlTransaction transaction)
        {
            string sql = @"INSERT INTO GameDetails (VideoGameId, Description, Rating) 
                           VALUES (@VideoGameId, @Description, @Rating);";

            await connection.ExecuteAsync(sql, gameDetail, transaction);

        }


        private async Task CreateReviewAsync(SqlConnection connection, Review review, SqlTransaction sqlTransaction)
        {
            string sql = @"INSERT INTO Reviews (VideoGameId, ReviewerName, Content, Rating) 
                          VALUES (@VideoGameId, @ReviewerName, @Content, @Rating);";

            await connection.ExecuteAsync(sql, review, sqlTransaction);

        }


        private async Task CreateVideoGamePlatformAsync(SqlConnection connection, VideoGamePlatform videoGamePlatform, SqlTransaction sqlTransaction)
        {

            string sql = @"INSERT INTO VideoGamesPlatforms (VideoGameId, PlatformId) 
                          VALUES (@VideoGameId, @PlatformId);";

            await connection.ExecuteAsync(sql, videoGamePlatform, sqlTransaction);

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
