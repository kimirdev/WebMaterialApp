using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebMaterial.BLL;
using WebMaterial.DAL.Models;
using WebMaterial.DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebMaterial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private List<string> Categories = new List<string> { "Presentation", "Application", "Other" };

        public MaterialController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        [HttpGet]
        public ActionResult<IList<Material>> GetAllMaterials(string category)
        {
            if (category == null)
                return Ok(_materialService.GetAllMaterials());
            if (Categories.Contains(category))
            {
                var materials = _materialService.GetFilteredMaterials(category);
                if (materials != null)
                    return materials.ToList();
            }
            return BadRequest();
        }

        // GET: api/Material/{id}
        [HttpGet("{id}")]
        public ActionResult<Material> GetMaterialsById(int id)
        {
            var material = _materialService.GetMaterialById(id);
            if (material != null)
                return Ok(material);
            return BadRequest();
        }

        // GET: api/Material/{name}
        [HttpGet]
        [Route("name")]
        public IActionResult GetMaterialByName(string name)
        {
            var material = _materialService.GetMaterialByName(name);
            if (material != null)
                return Ok(material);
            return BadRequest();
        }

        [HttpGet]
        [Route("download")]
        public IActionResult DownloadFile(string name, int? version)
        {
            var result = _materialService.DownloadFile(name, version);
            if (result != null)
            {
                return File(result, "application/octet-stream", name);
            }
            return BadRequest();
        }

        [HttpPatch]
        [Authorize(Roles = "initiator, admin")]
        public ActionResult<Material> ChangeMaterialCategory(string name, string category)
        {
            if (Categories.Contains(category))
            {
                var material = _materialService.ChangeMaterialCategory(name, category);
                if (material != null)
                {
                    return Ok(material);
                }
            }
            return BadRequest();
        }

        // POST: api/Material
        [HttpPost]
        [Authorize(Roles = "initiator, admin")]
        public IActionResult AddMaterial([FromForm] NewMaterialDto material, [FromForm] IFormFile file)
        {
            if (material.Name != null && material.Category != null && file != null
                && file.Length < 2147483648 && Categories.Contains(material.Category))
            {
                Material newMaterial = new Material { Name = material.Name, Category = material.Category };
                var result = _materialService.AddMaterial(newMaterial, file);
                if (result != null)
                    return Ok();
            }
            return BadRequest();
        }

        // POST: api/Material/add
        [HttpPost]
        [Route("add")]
        [Authorize(Roles = "initiator, admin")]
        public IActionResult AddVersion([FromForm] UpdateMaterialDto material, [FromForm] IFormFile file)
        {
            if (material.Name != null && file != null)
            {
                var result = _materialService.AddVersion(material.Name, file);
                if (result != null)
                    return Ok();
            }
            return BadRequest();
        }
    }
}
