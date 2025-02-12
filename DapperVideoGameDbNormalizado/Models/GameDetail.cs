namespace DapperVideoGameDbNormalizado.Models
{
    public class GameDetail
    {
        public int VideoGameId { get; set; }

        public required string Description { get; set; }

        public required string Rating { get; set; }

        //Propiedad de navegación
        public VideoGame? VideoGame { get; set; }
    }
}
