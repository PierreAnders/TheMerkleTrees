using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Infrastructure.Configurations;
using TheMerkleTrees.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Charger les variables d'environnement depuis le fichier .env
DotEnv.Load();

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration MongoDB
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();
    return client.GetDatabase(settings.DatabaseName);
});

// Injection des dépendances pour les repositories
builder.Services.AddSingleton<IFileRepository, FileRepository>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IShortcutRepository, ShortcutRepository>();

builder.Services.AddHttpContextAccessor();

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Charger les paramètres JWT depuis la configuration
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddHttpClient();

var app = builder.Build();

// Configuration de l'application
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("SpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
