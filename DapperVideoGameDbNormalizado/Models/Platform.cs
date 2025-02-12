namespace DapperVideoGameDbNormalizado.Models
{
    public class Platform
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        //Propiedad de navegación
        public List<VideoGame> VideoGames { get; set; }

    }
}
