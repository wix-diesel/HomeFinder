using Microsoft.AspNetCore.Mvc;

namespace HomeFinderAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AreasController : ControllerBase
{
    private readonly ILogger<AreasController> _logger;

    public AreasController(ILogger<AreasController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetAreas")]
    public IEnumerable<Area> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new Area
        {
            Id = index,
            Name = $"Area {index}",
            Description = $"Description for Area {index}"   
        })
        .ToArray();
    }
}

public class Area
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
