using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMaterialApp.Models
{
    public class Version
    {
        public int Id { get; set; }
        public long Size { get; set; }
        public DateTime UploadDate { get; set; }
        public int Release { get; set; }
        public string Path { get; set; }

        public int MaterialId { get; set; }
        public Material Material { get; set; }
    }
}
