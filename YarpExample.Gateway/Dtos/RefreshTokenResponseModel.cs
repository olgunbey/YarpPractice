namespace YarpExample.Gateway.Dtos
{
    public class RefreshTokenResponseModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTime { get; set; }
        public DateTime RefreshTime { get; set; }
    }
}
