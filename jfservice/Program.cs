using jfservice.Interfaces;
using jfservice.Models;
using jfservice.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));
builder.Services.AddScoped<IDataLoaderService, DataLoaderService>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;//теперь € хот€ бы в фильтре могу сам проверить тип, но из описани€ модели сообщени€ об ошибках не берутс€
})
                .AddXmlSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//app.UseMiddleware<ValidationMiddleware>();

app.MapControllers();

app.Run();