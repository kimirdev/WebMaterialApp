using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMaterialApp.Models
{
    public class NewVersionMaterialDTO
    {
        public string Name { get; set; }
        public byte[] File { get; set; }
    }
}
