namespace YarpExample.Gateway.Entity
{
    public class ClientCredentialTable
    {
        public int Id { get; set; } 
        public int ClientTokenId { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public DateTime RefreshTime { get; set; }
        public DateTime AccessTime { get; set; }
        public ClientTokenInfo ClientTokenInfo { get; set; }

    }
}
