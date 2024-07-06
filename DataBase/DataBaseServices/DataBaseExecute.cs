using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Panoramas_Editor_DB.Models;

namespace Panoramas_Editor;

public class DataBaseExecute
{

    internal static void AddImagesList(List<ImageSettings> imageSettingsList)
    {
        var imagesList = ConvertImageSettingsToImageInfoList(imageSettingsList);
        using (var db = new ApplicationContextDB(ApplicationContextDB.GetOptions()))
        {
            db.AddRange(imagesList);
            db.SaveChanges();
        }
    }

    private static ImagesInfo ConvertImageSettingsToImagesInfo(ImageSettings imageToDB)
    {
        ImagesSettings imageSettings = new ImagesSettings
            { horizontalOffset = imageToDB.HorizontalOffset, verticalOffset = imageToDB.VerticalOffset };
        ImageFiles dataFile = new ImageFiles
            { path = imageToDB.FullPath }; // здесь нужно изменить на добавление изображения 
        ImagesInfo image = new ImagesInfo
            { name = imageToDB.FileName, directory = imageToDB.Directory, image = dataFile, settings = imageSettings };
        return image;
    }
    

    private static List<ImagesInfo> ConvertImageSettingsToImageInfoList(List<ImageSettings> imageSettingsList)
    {
        List<ImagesInfo> convertedImagesInfo = new List<ImagesInfo>();
        foreach (var imageSettings in imageSettingsList)
        {
            convertedImagesInfo.Add(ConvertImageSettingsToImagesInfo(imageSettings));
        }
        return convertedImagesInfo;
    }

    internal static void AddImage(ImageSettings imageToDB)
    {
        using (var db = new ApplicationContextDB(ApplicationContextDB.GetOptions()))
        {
            db.Add(ConvertImageSettingsToImagesInfo(imageToDB));
            db.SaveChanges();
        }
    }

    internal static List<ImageSettings> GetAllDataList()
    {
        List<ImageSettings> allDataFromDB = new List<ImageSettings>();
        using (var db = new ApplicationContextDB(ApplicationContextDB.GetOptions()))
        {
            foreach (var image in db.imagesInfo.Include(imagesInfo => imagesInfo.image)
                         .Include(imagesInfo => imagesInfo.settings))
            {
                ImageSettings itemImagesSettings = new ImageSettings(Path.Combine(image.directory, image.name));
                itemImagesSettings.HorizontalOffset = image.settings.horizontalOffset;
                itemImagesSettings.VerticalOffset = image.settings.verticalOffset;
                if (image.image != null && image.settings != null)
                {
                    Console.WriteLine(image.id + " : " + image.name + " : " + image.image.path + " : " +
                                      image.settings.verticalOffset);
                }

                allDataFromDB.Add(itemImagesSettings);
            }
        }

        return allDataFromDB;
    }

    internal static bool Update(ImagesInfo oldImagesInfo, ImagesInfo newImagesInfo)
    {
        bool isUpdated = false;
        var db = new ApplicationContextDB(ApplicationContextDB.GetOptions());
        ImagesInfo findImage = db.Find<ImagesInfo>(oldImagesInfo.id);
        if (findImage != null)
        {
            findImage.image = newImagesInfo.image; // не меняется 
            findImage.settings = newImagesInfo.settings; // не меняется 
            findImage.directory = newImagesInfo.directory;
            findImage.name = newImagesInfo.name;
            db.Update(findImage);
            db.SaveChanges();
            isUpdated = true;
        }

        return isUpdated;
    }

    internal static void DropDB()
    {
        using var db = new ApplicationContextDB(ApplicationContextDB.GetOptions());
        db.Database.EnsureDeleted();
    }
}