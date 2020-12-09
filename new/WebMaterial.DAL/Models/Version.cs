using System;
using System.ComponentModel.DataAnnotations;

namespace WebMaterial.DAL.Models
{
    public class Version
    {
        [Key]
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public DateTime UploadDateTime { get; set; }
        public long Size { get; set; }
        public int Release { get; set; }
        public string Path { get; set; }

    }
}
