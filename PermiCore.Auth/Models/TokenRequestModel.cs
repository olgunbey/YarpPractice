namespace PermiCore.Auth.Models
{
    public class TokenRequestModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
    }
}
