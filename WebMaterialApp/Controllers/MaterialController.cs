using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMaterialApp.Models;

namespace WebMaterialApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _filesPath;

        public MaterialController(ApplicationContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _filesPath = _webHostEnvironment.ContentRootPath + "\\Files\\";
        }

        // GET: api/Material
        [HttpGet]
        public IActionResult GetMaterials(Category? category)
        {
            if (category != null && ((int)category >= 1 && (int)category <= 3))
                return Ok(_context.Materials.Include(p => p.Versions).Where(p => p.Category == category).ToList());
            return Ok(_context.Materials.Include(p => p.Versions).ToList());
        }

        // GET: api/Material/name
        [HttpGet("{name}")]
        public IActionResult GetMaterial(string name)
        {
            var material = _context.Materials.Include(p => p.Versions).FirstOrDefault(p => p.Name == name);
            if (material == null)
            {
                return NotFound();
            }

            return Ok(material);
        }

        [HttpPatch]
        public IActionResult ChangeMaterialCategory(string name, Category category)
        {
            var material = _context.Materials.FirstOrDefault(p => p.Name == name);

            if (material == null)
                return NotFound();
            if ((int)category < 1 || (int)category > 3)
                return BadRequest("Invalid category");
            material.Category = category;
            _context.SaveChanges();
            return Ok(material);
        }

        // POST: api/Material
        [HttpPost]
        public async Task<ActionResult<Material>> PostMaterial(CreateMaterialDTO model)
        {
            var materialPath = $"{_filesPath}{model.Name}\\";
            var unique = _context.Materials.FirstOrDefault(m => m.Name == model.Name) == null;

            if (ModelState.IsValid)
            {
                if (!unique)
                    return (BadRequest($"Material with name {model.Name} already exists"));
                if ((int)model.Category > 3 || (int)model.Category < 1)
                    return BadRequest("Invalid category");
                Directory.CreateDirectory(materialPath);
                System.IO.File.WriteAllBytes($"{materialPath}{model.Name}_1", model.File);

                var material = new Material() { Name = model.Name, Category = model.Category };
                _context.Materials.Add(material);
                var version = new Models.Version()
                {
                    Material = material,
                    UploadDate = DateTime.Now,
                    Release = 1,
                    Size = model.File.Length,
                    Path = $"{materialPath}{material.Name}_1"
                };
                _context.Versions.Add(version);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetMaterial", new { id = material.Id }, material);
            }
            return BadRequest("Invalid model");
        }


        [Route("update")]
        [HttpPost]
        public async Task<ActionResult<Material>> UpdateMaterial(NewVersionMaterialDTO model)
        {
            if (ModelState.IsValid)
            {
                Material material = _context.Materials.Include(p => p.Versions).FirstOrDefault(p => p.Name == model.Name);
                if (material == null)
                    return NoContent();
                var materialPath = $"{_filesPath}{model.Name}\\";
                var release = material.Versions.Count + 1;
                var version = new Models.Version()
                {
                    MaterialId = material.Id,
                    Material = material,
                    Release = release,
                    UploadDate = DateTime.Now,
                    Size = model.File.Length,
                    Path = $"{materialPath}{model.Name}_{release}"
                };
                _context.Versions.Add(version);
                Directory.CreateDirectory(materialPath);
                System.IO.File.WriteAllBytes($"{materialPath}{model.Name}_{release}", model.File);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetMaterial", new { id = material.Id }, material);
            }
            return BadRequest("Invalid model");
        }

        // DELETE: api/Material/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Material>> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return material;
        }

        [HttpGet("download/{name}")]
        public IActionResult DownloadMaterial(string name, int? version)
        {
            var material = _context.Materials.Include(v => v.Versions).FirstOrDefault(p => p.Name == name);
            if (material == null)
                return NoContent();
            Models.Version vers;
            if (version != null)
                vers = material.Versions.FirstOrDefault(v => v.Release == version);
            else
                vers = material.Versions.Last();
            string fileName = $"{material.Name}_{vers.Release}";
            string path = Path.Combine(_filesPath, material.Name, fileName);
            byte[] file = System.IO.File.ReadAllBytes(path);
            return File(file, "application/octet-stream", fileName);
        }
        
    }
}
