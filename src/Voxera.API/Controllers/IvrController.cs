using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;
using Voxera.Domain.Enums;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/ivr")]
[Authorize]
[Produces("application/json")]
public class IvrController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public IvrController(IApplicationDbContext db) => _db = db;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);

    /// <summary>List all IVR menus</summary>
    [HttpGet]
    public async Task<IActionResult> GetIvrMenus(CancellationToken ct)
    {
        var menus = await _db.IvrMenus
            .Where(m => m.CompanyId == CompanyId)
            .Include(m => m.Options)
            .Include(m => m.Schedules)
            .ToListAsync(ct);
        return Ok(menus);
    }

    /// <summary>Create a new IVR menu</summary>
    [HttpPost]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> CreateIvrMenu([FromBody] CreateIvrMenuRequest request, CancellationToken ct)
    {
        var menu = IvrMenu.Create(CompanyId, request.Name, request.GreetingText);
        await _db.IvrMenus.AddAsync(menu, ct);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetIvrMenus), new { id = menu.Id }, new { id = menu.Id });
    }

    /// <summary>Add an option to an IVR menu</summary>
    [HttpPost("{menuId}/options")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> AddIvrOption(Guid menuId, [FromBody] AddIvrOptionRequest request, CancellationToken ct)
    {
        var menu = await _db.IvrMenus.FirstOrDefaultAsync(m => m.Id == menuId && m.CompanyId == CompanyId, ct);
        if (menu is null) return NotFound();

        var option = IvrOption.Create(menuId, request.Digit, request.Description, request.ActionType, request.ActionTarget);
        await _db.IvrOptions.AddAsync(option, ct);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = option.Id });
    }

    /// <summary>Delete an IVR menu</summary>
    [HttpDelete("{menuId}")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> DeleteIvrMenu(Guid menuId, CancellationToken ct)
    {
        var menu = await _db.IvrMenus.FirstOrDefaultAsync(m => m.Id == menuId && m.CompanyId == CompanyId, ct);
        if (menu is null) return NotFound();
        menu.SoftDelete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateIvrMenuRequest(string Name, string? GreetingText = null, int Timeout = 5);
public record AddIvrOptionRequest(string Digit, string Description, IvrActionType ActionType, string? ActionTarget = null);
