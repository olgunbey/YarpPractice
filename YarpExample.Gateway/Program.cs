using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder =>
    {
        transformBuilder.AddRequestTransform(async transformRequest =>
        {
            string path = transformRequest.Path.Value!;


            if(path.StartsWith("/order",StringComparison.OrdinalIgnoreCase))
            {
                //Auth Server'a istek atýp token al, header'a ekle

            }
            if (path.StartsWith("/basket",StringComparison.OrdinalIgnoreCase))
            {
                var newPath = "/api/Basket" + path.Substring("/basket".Length);
                transformRequest.Path = newPath;
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

app.MapReverseProxy();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
