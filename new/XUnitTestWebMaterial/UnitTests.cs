using System;
using Xunit;
using WebMaterial.DAL;
using WebMaterial.BLL;
using WebMaterial.DAL.Data;
using WebMaterial.DAL.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebMaterialTest
{
    public class UnitTests
    {
        private MaterialService _materialService = new MaterialService(
            new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NewMaterialDb;Trusted_Connection=True;")
                .Options), new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build());

        [Fact]
        public void GetMaterialById_GetMaterialWithId1_NotNull()
        {
            
            var result = _materialService.GetMaterialById(1);
            
            Assert.NotNull(result);
        }

        [Fact]
        public void GetMaterialByName_GetMaterialWithNameTestFile_NotNull()
        {
            var result = _materialService.GetMaterialByName("TestFile");
            
            Assert.NotNull(result);
        }

        [Fact]

        public void ChangeMaterialCategory_GetMaterialWithEditCategory_Other()
        {
            Material material = _materialService.GetMaterialById(1);

            var result = _materialService.ChangeMaterialCategory(material.Name, "Other");

            Assert.Equal("Other", result.Category);
        }

        [Fact]

        public void AddMaterial_AddEmptyFile_NotNull()
        {
            Material material = new Material { Name = "newmaterial", Category = "Other" };
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("new")), 0, 0, "Data", "text1.txt");

            var result = _materialService.AddMaterial(material, file);
            
            Assert.NotNull(result);
        }
    }
}
