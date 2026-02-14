using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Database;
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
        //context.AddRequestTransform(async transformContext =>
        //{
        //    Guid guid = Guid.NewGuid();
        //    transformContext.ProxyRequest.Headers.Add("testId", guid.ToString());
        //    await Task.CompletedTask;
        //});
        context.AddRequestTransform(async tContext =>
        {
            var uri = tContext.ProxyRequest.RequestUri;
            string requestClient = tContext.ProxyRequest.Headers.GetValues("Client_name").Single();

            using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
            {
                var yarpDbContext = serviceProvider.GetRequiredService<YarpDbContext>();

                var client = await yarpDbContext
                  .Clients
                  .AsNoTracking()
                  .SingleOrDefaultAsync(y => y.ClientName == requestClient);

                if (client is null)
                    return;


                var clientCredentialTable = await yarpDbContext.ClientCredentialTable
                  .SingleOrDefaultAsync(y => y.ClientId == client.ClientId);

                if (clientCredentialTable is null)
                    return;


                if ((DateTime.UtcNow - clientCredentialTable.AccessTime).TotalDays <= 1)
                {
                    //Bu token ile istek atılacak.
                    var token = clientCredentialTable.AccessToken;

                }
                else if ((DateTime.UtcNow - clientCredentialTable.RefreshTime).TotalDays <= 365)
                {
                    //Refresh token ile accessToken oluştur.
                }
                else
                {
                    //Artık bu kullanıcının servise erişim hakkı bitmiştir, yeni hak için clientCredentialtable'ye yeni kayıt ekle
                }



            }

            var path = tContext.Path;
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
