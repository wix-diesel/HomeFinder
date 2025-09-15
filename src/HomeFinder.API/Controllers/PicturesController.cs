using HomeFinderAPI.Models;
using HomeFinderAPI.Models.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        private readonly ILogger<PicturesController> _logger;

        private readonly IPictureService _pictureService;

        public PicturesController(ILogger<PicturesController> logger, IPictureService pictureService)
        {
            _logger = logger;
            _pictureService = pictureService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PictureDTO>> GetPicture(int id)
        {
            var picture = await _pictureService.GetPictureAsync(id);
            if (picture == null)
                return NotFound();
            return picture;
        }

        [HttpPost]
        public async Task<ActionResult<PictureDTO>> AddPicture([FromForm] PictureDTO pictureDTO, IFormFile file)
        {
            if (pictureDTO == null)
                return BadRequest();
            Stream? stream = null;
            if (file != null)
            {
                stream = file.OpenReadStream();
            }
            var addedPicture = await _pictureService.AddPictureAsync(pictureDTO, stream);
            return CreatedAtAction(nameof(GetPicture), new { id = addedPicture.Id }, addedPicture);
        }
    }
}
