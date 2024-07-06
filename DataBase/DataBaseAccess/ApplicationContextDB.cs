using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panoramas_Editor_DB.Models;

namespace Panoramas_Editor;

public class ApplicationContextDB : DbContext
{
    private const string PATH_TO_APPSETTING = @"D:\Development\ProjectsFromVCS\С#\Panoramas-Editor\Panoramas-Editor\appsettings.json";

    public DbSet<ImagesInfo> imagesInfo { get; set; } = null!;
    private DbSet<ImagesSettings> imagesSettings { get; set; } = null!;
    private DbSet<ImageFiles> imageFiles { get; set; } = null!;
    
    
    public ApplicationContextDB(DbContextOptions<ApplicationContextDB> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    public static DbContextOptions<ApplicationContextDB> GetOptions(String pathToAppSettings = PATH_TO_APPSETTING)
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile(pathToAppSettings);
        var config = builder.Build();
        var connectionString = config.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContextDB>();
        var options = optionsBuilder.UseSqlite(connectionString).Options;
        return options;
    }

    
}