using RSSFeedReader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISubscriptionStore, InMemorySubscriptionStore>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5213", "https://localhost:7213")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
