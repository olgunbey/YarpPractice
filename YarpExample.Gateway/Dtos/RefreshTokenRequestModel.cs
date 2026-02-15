namespace YarpExample.Gateway.Dtos
{
    public class RefreshTokenRequestModel
    {
        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

    }
}
