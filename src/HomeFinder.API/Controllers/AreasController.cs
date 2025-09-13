using HomeFinder.API.Models;
using HomeFinder.API.Models.Service;
using HomeFinder.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinderAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AreasController : ControllerBase
{
    private readonly ILogger<AreasController> _logger;

    private readonly AreaService _areaService;

    public AreasController(ILogger<AreasController> logger, AreaService areaService)
    {
        _logger = logger;
        _areaService = areaService;
    }

    // 一覧取得
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AreaDTO>>> GetAreas()
    {
        var areas = await _areaService.GetAllAreasAsync();
        return Ok(areas);
    }

    // 追加
    [HttpPost]
    public async Task<ActionResult<AreaDTO>> AddAreaDTO([FromBody] AreaDTO AreaDTO)
    {
        if (AreaDTO == null || string.IsNullOrWhiteSpace(AreaDTO.Name))
            return BadRequest();

        var addedArea = await _areaService.AddAreaAsync(AreaDTO);
        return CreatedAtAction(nameof(GetAreaDTOById), new { id = addedArea.Id }, addedArea);
    }


    // 編集
    [HttpPut("{id}")]
    public async Task<ActionResult<AreaDTO>> EditAreaDTO(int id, [FromBody] AreaDTO updatedAreaDTO)
    {
        var updatedCategory = await _areaService.UpdateAreaAsync(id, updatedAreaDTO);
        if (updatedCategory == null)
            return NotFound();
        return Ok(updatedCategory);
    }


    // 単一取得（追加のため）
    [HttpGet("{id}")]
    public async Task<ActionResult<AreaDTO>> GetAreaDTOById(int id)
    {
        var category = await _areaService.GetAreaByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }
}


