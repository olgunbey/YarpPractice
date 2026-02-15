using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Dtos;
using YarpExample.Gateway.Entity;
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
            HttpClient httpClient = new HttpClient();
            RefreshTokenResponseModel? refreshTokenResponseModel = new();
            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
            {
                using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
                {
                    var yarpDbContext = serviceProvider.GetRequiredService<YarpDbContext>();

                    var clientToken = await yarpDbContext.ClientTokenInfo
                    .Include(y => y.ClientCredentialTable)
                    .SingleOrDefaultAsync(y => y.ClientId == clientId && y.ClientSecret == clientSecret);

                    if (clientToken is null)
                        throw new Exception("Hata!!");


                    if (clientToken.ClientCredentialTable is not null && (DateTime.UtcNow - clientToken.ClientCredentialTable.AccessTime).TotalDays <= 1)
                    {
                        string accessToken = clientToken.ClientCredentialTable.AccessToken;
                        proxyRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        return;
                    }
                    else if (clientToken.ClientCredentialTable is not null && (DateTime.UtcNow - clientToken.ClientCredentialTable.RefreshTime).TotalDays <= 365)
                    {
                        AccessTokenRequestModel accessTokenRequestModel = new()
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                            GrantType = "refresh_token",
                            RefreshToken = clientToken.ClientCredentialTable.RefreshToken
                        };
                        var responseMessage = await httpClient.PostAsJsonAsync("http://localhost:5127/api/Connect/Token", accessTokenRequestModel);

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            refreshTokenResponseModel = await responseMessage.Content.ReadFromJsonAsync<RefreshTokenResponseModel>();

                            clientToken.ClientCredentialTable.AccessToken = refreshTokenResponseModel!.AccessToken;
                            clientToken.ClientCredentialTable.RefreshToken = refreshTokenResponseModel!.RefreshToken;
                            clientToken.ClientCredentialTable.AccessTime = refreshTokenResponseModel!.AccessTime;
                            clientToken.ClientCredentialTable.RefreshTime = refreshTokenResponseModel!.RefreshTime;
                            await yarpDbContext.SaveChangesAsync();

                            string accessToken = refreshTokenResponseModel.AccessToken;
                            proxyRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        }
                        else
                            return;

                    }
                    else
                    {
                        AccessTokenRequestModel accessTokenRequestModel = new()
                        {
                            ClientId =clientId,
                            ClientSecret = clientSecret,
                            GrantType = "client_credentials"
                        };
                        var responseMessage = await httpClient.PostAsJsonAsync("http://localhost:5127/api/Connect/Token", accessTokenRequestModel);

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            refreshTokenResponseModel = await responseMessage.Content.ReadFromJsonAsync<RefreshTokenResponseModel>();

                            clientToken.ClientCredentialTable = new ClientCredentialTable()
                            {
                                AccessToken = refreshTokenResponseModel!.AccessToken,
                                RefreshToken = refreshTokenResponseModel.RefreshToken,
                                AccessTime = refreshTokenResponseModel.AccessTime,
                                RefreshTime = refreshTokenResponseModel.RefreshTime
                            };

                            await yarpDbContext.SaveChangesAsync();

                            string accessToken = refreshTokenResponseModel.AccessToken;
                            proxyRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        }

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
