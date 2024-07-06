using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Panoramas_Editor_DB.Models;

[Table("images_info")]
public class ImagesInfo
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    [Column("name")]
    public string name { get; set; }
    [Column("path_to_folder")]
    public string directory { get; set; }

    public ImagesSettings settings { get; set; }
    public ImageFiles image { get; set; }
}