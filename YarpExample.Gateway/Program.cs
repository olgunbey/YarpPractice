using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Dtos;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<YarpDbContext>(y => y.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async tContext =>
        {
            var proxyRequest = tContext.ProxyRequest;
            string? clientId = proxyRequest.Headers.GetValues("client_id").FirstOrDefault();
            string? clientSecret = proxyRequest.Headers.GetValues("client_secret").FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
            {
                using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
                {
                    var yarpDbContext = serviceProvider.GetRequiredService<YarpDbContext>();

                    var clientCredentialTable = await yarpDbContext.ClientCredentialTable
                      .SingleOrDefaultAsync(y => y.ClientId == clientId && y.ClientSecret == clientSecret);

                    if (clientCredentialTable is null)
                        return;


                    if ((DateTime.UtcNow - clientCredentialTable.AccessTime).TotalDays <= 1)
                    {
                        //Bu token ile istek atılacak.
                        string accessToken = clientCredentialTable.AccessToken;
                        proxyRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        return;
                    }
                    else if ((DateTime.UtcNow - clientCredentialTable.RefreshTime).TotalDays <= 365)
                    {
                        //Refresh token ile accessToken oluştur.
                        HttpClient httpClient = new HttpClient();
                        RefreshTokenResponseModel? refreshTokenResponseModel = new();
                        RefreshTokenRequestModel refreshTokenRequestModel = new()
                        {
                            ClientId = "refresh_id",
                            ClientSecret = "refresh_secret",
                            RefreshToken = clientCredentialTable.RefreshToken
                        };
                        var responseMessage = await httpClient.PostAsJsonAsync("http://localhost:5127/api/Connect/TokenUsingRefreshToken", refreshTokenRequestModel);

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            refreshTokenResponseModel = await responseMessage.Content.ReadFromJsonAsync<RefreshTokenResponseModel>();

                            //Yarp'daki clientcredential tokenleri güncelle.
                            clientCredentialTable.AccessToken = refreshTokenResponseModel!.AccessToken;
                            clientCredentialTable.RefreshToken = refreshTokenResponseModel!.RefreshToken;
                            clientCredentialTable.AccessTime = refreshTokenResponseModel!.AccessTime;
                            clientCredentialTable.RefreshTime = refreshTokenResponseModel!.RefreshTime;
                            await yarpDbContext.SaveChangesAsync();

                            string accessToken = refreshTokenResponseModel.AccessToken;
                            proxyRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        }
                        else
                            return;

                    }
                    else
                    {
                        //Artık bu kullanıcının servise erişim hakkı bitmiştir, yeni hak için clientCredentialtable'ye yeni kayıt ekle
                    }
                }
            }
        });
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.MapControllers();
app.Run();
