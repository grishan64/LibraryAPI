using LibraryAPI.Data.Contexts;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

namespace LibraryAPI;

public static class Startup
{
    public static Task<WebApplicationBuilder> ConfigureServices(this WebApplicationBuilder builder)
    {

        #region Alias

        var services = builder.Services;
        var configuration = builder.Configuration;
        var enviroment = builder.Environment;
        var host = builder.Host;

        #endregion

        builder
            .AddLogging()
            .AddDatabase()
            .AddControllers()
            .AddMapper()
            .AddSwagger();

        return Task.FromResult(builder);
    }


    public static async Task<WebApplication> Configure(this WebApplication app)
    {

#if !NO_AUTO_MIGRATE
        var migrationTask = app.MigrateDatabase();
#endif

        app.UseDevelopmentConfiguration();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

#if !NO_AUTO_MIGRATE
        await migrationTask;
#endif

        return app;
    }

    #region WebApplicationBuilder | Add.*

    private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {

        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(builder.Configuration);
        });

        return builder;
    }

    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddControllers();

        return builder;
    }


    private static WebApplicationBuilder AddMapper(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton((serviceProvider) =>
        {
            var config = new TypeAdapterConfig();
            config.Default.PreserveReference(true);

            config.Scan(typeof(Startup).Assembly);
            return config;
        });

        services.AddScoped<IMapper, ServiceMapper>();

        return builder;
    }

    private static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            var isDev = builder.Environment.IsDevelopment();

            options
                .UseNpgsql(builder.Configuration.GetConnectionString(nameof(ApplicationDbContext)))
                .EnableDetailedErrors(isDev)
                .EnableSensitiveDataLogging(isDev);
        });

        return builder;
    }

    private static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            var name = nameof(LibraryAPI);
            options.SwaggerDoc("v1", new OpenApiInfo { Title = name, Version = "v1" });

            options.CustomSchemaIds(type => type.ToString());

            options.EnableAnnotations();

            var filePath = Path.Combine(AppContext.BaseDirectory, $"{name}.xml");
            if (File.Exists(filePath))
                options.IncludeXmlComments(filePath);
        });

        return builder;
    }

    #endregion


    #region WebApplication | Use.*

    private static async Task MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider;

        using var dbContext = provider.GetRequiredService<ApplicationDbContext>();

        if (!dbContext.Database.IsRelational()) return;

        await dbContext.Database.MigrateAsync();
    }

    private static WebApplication UseDevelopmentConfiguration(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.UseWhen(x => !x.Request.Path.StartsWithSegments("/api"), (app) =>
        {
            app.UseDeveloperExceptionPage();

        });

        var swaggerPrefix = "api";

        app.UseSwagger(options =>
        {
            options.RouteTemplate = $"{swaggerPrefix}/swagger/{{documentName}}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = $"{swaggerPrefix}/swagger";
            c.SwaggerEndpoint($"/{swaggerPrefix}/swagger/v1/swagger.json", $"{nameof(LibraryAPI)} v1");
        });

        return app;
    }

    #endregion


    #region WebApplicationBuilder | public deps

    public static WebApplicationBuilder ConfigureStaticLogger(this WebApplicationBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console()
            .ReadFrom
            .Configuration(builder.Configuration);

        Log.Logger = loggerConfiguration.CreateBootstrapLogger();

        return builder;
    }

    #endregion
}
