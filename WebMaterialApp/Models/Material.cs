using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebMaterialApp.Models
{
    public enum Category
    {
        NONE,
        PRESENTATION,
        APPLICATION,
        OTHER
    }
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public ICollection<Version> Versions { get; set; }
    }
}
