namespace DapperVideoGameDbNormalizado.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int VideoGameId { get; set; }

        public required string ReviewerName { get; set; }

        public required string Content { get; set; }

        public int Rating { get; set; }

        //Propiedad de navegación
        public VideoGame? VideoGame { get; set; }
    }
}
