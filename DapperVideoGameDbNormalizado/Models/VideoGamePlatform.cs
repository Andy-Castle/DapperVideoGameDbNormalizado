namespace DapperVideoGameDbNormalizado.Models
{
    public class VideoGamePlatform
    {

        public int VideoGameId { get; set; }

        public int PlatformId { get; set; }

        //Propiedades de navegación
        public VideoGame? VideoGame { get; set; }
        public Platform? Platform { get; set; }
    }
}
