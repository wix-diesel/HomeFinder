using HomeFinder.API.Models;
using HomeFinder.API.Models.Service;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinderAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;
    private readonly ItemService _itemService;

    public ItemsController(ILogger<ItemsController> logger, ItemService itemService)
    {
        _logger = logger;
        _itemService = itemService;
    }

    [HttpPost]
    public ActionResult<ItemDTO> AddItem([FromBody] ItemDTO itemDto)
    {
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

