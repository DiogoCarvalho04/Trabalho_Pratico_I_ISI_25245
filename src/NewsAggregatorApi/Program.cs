using NewsAggregatorApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Serviços principais
builder.Services.AddHttpClient();
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<RedditService>();
builder.Services.AddScoped<AggregationService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocal");
app.UseAuthorization();
app.MapControllers();
app.Run();
