using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using WebMaterial.DAL.Data;
using WebMaterial.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Version = WebMaterial.DAL.Models.Version;
using System.Threading.Tasks;
using Hangfire;

namespace WebMaterial.BLL
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _config;
        private readonly IBackgroundJobClient _backgroundJob;
        public MaterialService(ApplicationContext context, IConfiguration config, IBackgroundJobClient backgroundJob)
        {
            _context = context;
            _config = config;
            _backgroundJob = backgroundJob;
        }
        public MaterialService(ApplicationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IList<Material> GetAllMaterials()
        {
            return _context.Materials.Include(p => p.Versions).ToList<Material>();
        }
        public Material GetMaterialByName(string name)
        {
            var material = _context.Materials.Include(p => p.Versions).Where(p => p.Name == name).FirstOrDefault();
            if (material == null)
                return null;
            return material;
        }

        public Material GetMaterialById(int id)
        {
            var material = _context.Materials.Include(p => p.Versions).FirstOrDefault(p => p.Id == id);
            if (material == null)
                return null;
            return (Material)material;
        }

        public Version AddMaterial(Material material, IFormFile file)
        {
            string ext = file.FileName.Split(".").Last();
            Version newVersion;
            if (_context.Materials.FirstOrDefault(p => p.Name == material.Name) == null)
            {
                newVersion = new Version
                {
                    Material = material,
                    Path = _config.GetValue<string>("PathFiles") + material.Name + "_1" + $".{ext}",
                    Release = 1,
                    Size = file.Length,
                    UploadDateTime = DateTime.Now
                };
                using (var filestream = new FileStream(newVersion.Path, FileMode.Create))
                {
                    file.CopyTo(filestream);
                }
                _context.Materials.Add(material);
                _context.Versions.Add(newVersion);
                _context.SaveChanges();
                _backgroundJob.Enqueue(() => MailService.SendAsync($"Material with name \"{material.Name}\" was created"));
                return newVersion;
            }
            return null;
        }

        public Version AddVersion(string name, IFormFile file)
        {
            string ext = file.FileName.Split(".").Last();
            Material material = _context.Materials.Include(p => p.Versions).FirstOrDefault(p => p.Name == name);
            Version newVersion;
            if (material != null)
            {
                newVersion = new Version
                {
                    Material = material,
                    Path = _config.GetValue<string>("PathFiles") + material.Name + "_" + (material.Versions.Count() + 1) + $".{ext}",
                    Release = material.Versions.Count() + 1,
                    Size = file.Length,
                    UploadDateTime = DateTime.Now
                };
                using (var filestream = new FileStream(newVersion.Path, FileMode.Create))
                {
                    file.CopyTo(filestream);
                }
                _context.Versions.Add(newVersion);
                _context.SaveChanges();
                _backgroundJob.Enqueue(() => MailService.SendAsync($"New version of \"{material.Name}\" was added"));
                return newVersion;
            }
            return null;
        }
        public IList<Material> GetFilteredMaterials(string category)
        {
            var materials = from m in _context.Materials.Include(p => p.Versions)
                           select m;
            var filteredMaterials = materials.Where(s => s.Category == category);
            if (filteredMaterials != null)
                return filteredMaterials.ToList();
            return null;
        }

        public Material ChangeMaterialCategory(string name, string category)
        {
            var material = _context.Materials.Include(p => p.Versions).Where(p => p.Name == name).FirstOrDefault();
            if (material != null)
            {
                material.Category = category;
                _context.SaveChanges();
                _backgroundJob.Enqueue(() => MailService.SendAsync($"The \"{material.Name}\"'s category was changed to {category}"));
                return material;
            }
            return null;
        }

        public byte[] DownloadFile(string name, int? version)
        {
            string Path;
            byte[] mas;
            var material = _context.Materials.Include(p => p.Versions).FirstOrDefault(p => p.Name == name);
            if (material != null)
            {
                if (version != null)
                    Path = _config.GetValue<string>("PathFiles") + material.Name + "_" + version;
                else
                    Path = _config.GetValue<string>("PathFiles") + material.Name + "_" + material.Versions.Count();
                mas = System.IO.File.ReadAllBytes(Path);
                return mas;
            }
            return null;
        }
    }
}
