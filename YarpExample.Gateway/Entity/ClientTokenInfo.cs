namespace YarpExample.Gateway.Entity
{
    public class ClientTokenInfo
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public ClientCredentialTable ClientCredentialTable { get; set; }
    }
}
