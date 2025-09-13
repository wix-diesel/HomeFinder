using HomeFinder.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinderAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(ILogger<ItemsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetItems")]
    public IEnumerable<ItemDTO> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new ItemDTO
        {
            Id = index,
            Name = $"Item {index}",
            Price = Random.Shared.Next(10, 100)
        })
        .ToArray();
    }
}

