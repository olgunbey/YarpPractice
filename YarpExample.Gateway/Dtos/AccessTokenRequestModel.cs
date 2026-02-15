namespace YarpExample.Gateway.Dtos
{
    public class AccessTokenRequestModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public string RefreshToken { get; set; }
    }
}
