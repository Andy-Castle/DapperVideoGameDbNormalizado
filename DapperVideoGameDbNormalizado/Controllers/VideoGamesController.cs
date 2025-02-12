using DapperVideoGameDbNormalizado.Models;
using DapperVideoGameDbNormalizado.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperVideoGameDbNormalizado.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoGamesController : ControllerBase
    {
        private readonly IVideoGameRepository _videoGameRepository;

        public VideoGamesController(IVideoGameRepository videoGameRepository)
        {
            _videoGameRepository = videoGameRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<VideoGame>>> GetAllVideoGames()
        {
            var videoGames = await _videoGameRepository.GetAllVideoGamesAsync();

            return Ok(videoGames);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VideoGame>> GetOneVideoGame(int id)
        {
            var videoGame = await _videoGameRepository.GetVideoGameAsync(id);

            if (videoGame == null)
            {
                return NotFound();
            }

            return Ok(videoGame);
        }
    }
}
