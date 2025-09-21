using HomeFinder.API.Models;
using HomeFinder.API.Models.Service;
using HomeFinderAPI.Models;
using HomeFinderAPI.Models.Service;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace HomeFinderAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;
    private readonly ItemService _itemService;
    private readonly IPictureService _pictureService;

    public ItemsController(ILogger<ItemsController> logger, ItemService itemService, IPictureService pictureService)
    {
        _logger = logger;
        _itemService = itemService;
        _pictureService = pictureService;
    }

    [HttpPost]
    public async Task<ActionResult<ItemDTO>> AddItemAsync([FromBody] ItemDTO itemDto, IFormFile picture)
    {
        if (picture != null)
        {
            var stream = picture.OpenReadStream();
            var pictureDTO = new PictureDTO
            {
                Description = itemDto.Description,
            };
            var addedPicture = await _pictureService.AddPictureAsync(pictureDTO, stream);
            itemDto.Picture = addedPicture;
        }
        var result = _itemService.AddItemAsync(itemDto);
        return CreatedAtAction(nameof(GetItem), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public ActionResult<ItemDTO> GetItem(int id)
    {
        var item = _itemService.GetItemAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ItemDTO>> GetAllItems()
    {
        var items = _itemService.GetAllItemsAsync();
        return Ok(items);
    }

    [HttpPut("{id}")]
    public ActionResult<ItemDTO> UpdateItem(int id, [FromBody] ItemDTO itemDto)
    {
        var updated = _itemService.UpdateItemAsync(id, itemDto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemAsync(int id)
    {
        // ItemServiceにDeleteItemメソッドが必要です
        var item = _itemService.GetItemAsync(id);
        if (item == null) return NotFound();

        await _itemService.DeleteItemAsync(id);
        return NoContent();
    }
}

