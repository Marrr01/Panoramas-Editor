using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor_DB.Models;

[Table("image_files")]
public class ImageFiles
{
    [Key, Column("images_info_id")]
    public int ImagesInfoId { get; set; } 
    public ImagesInfo ImagesInfo { get; set;}
    
    [Column("image")]
    public string path { get; set; } 
    //public BitmapImage imageFile { get; set; } 
    
}