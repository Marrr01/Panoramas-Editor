using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Panoramas_Editor_DB.Models;

[Table("image_settings")]
public class ImagesSettings
{
   [Key]
   public int Id { get; set; }
   
   [ForeignKey("images_info_id")]  // сделать так, или как в ImageFiles
   public int ImagesInfoId { get; set; } 
   public ImagesInfo ImagesInfo { get; set;}
   
   [Column("horizontal_offset")] 
   public double horizontalOffset { get; set; }
   [Column("vertical_offset")] 
   public double verticalOffset { get; set; }
}

