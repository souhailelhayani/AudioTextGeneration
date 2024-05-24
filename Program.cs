using AudioTextGeneration.src.main.Services;
using Microsoft.Extensions.Azure;
using Azure.Identity;

public class EntryPoint {
    private static readonly string _storageAccountName = "testaccountalexsys1";

    public static void Main(String[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //Register the services
        builder.Services.AddScoped<TranscriptionService>();
        builder.Services.AddScoped<AudioService>();
        builder.Services.AddScoped<StorageService>();

        //add azure service clients
        builder.Services.AddAzureClients(clientBuilder =>
        {
            // Register clients for each service
            clientBuilder.AddBlobServiceClient(new Uri($"https://{_storageAccountName}.blob.core.windows.net"));
            clientBuilder.UseCredential(new DefaultAzureCredential());
        });

        //needs to be added to discover the controllers
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        //need to add this to be able to call the controller methods
        app.MapControllers();
        
        app.Run();
    }
}