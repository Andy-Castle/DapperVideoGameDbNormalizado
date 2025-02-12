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

        [HttpPost]
        public async Task<ActionResult> CreateVideoGame(VideoGame videoGame)
        {
            if (videoGame == null)
            {

                return BadRequest();
            }

            var createdId = await _videoGameRepository.CreateVideoGameAsync(videoGame);

            return CreatedAtAction(nameof(GetOneVideoGame), new { id = createdId }, videoGame);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVideoGame(int id, VideoGame videoGame)
        {
            if (videoGame == null || videoGame.Id != id)
            {
                return BadRequest();
            }

            var existingVideoGame = await _videoGameRepository.GetVideoGameAsync(id);
            if (existingVideoGame == null)
            {
                return NotFound();
            }

            await _videoGameRepository.UpdateVideoGameAsyncs(videoGame);
            return NoContent();
        }
    }
}
