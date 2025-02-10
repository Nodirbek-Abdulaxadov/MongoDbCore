using System.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration.GetSection("MongoDB").Get<MongoDbCoreOptions>();

builder.Services.AddMongoDbContext<AppDbContext>(config);

// its current config
//builder.Services.AddMongoDbIdentity<UsersDbContext, User>(config);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

GC.TryStartNoGCRegion(100_000_000);
GC.EndNoGCRegion();
GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;


app.Run();
