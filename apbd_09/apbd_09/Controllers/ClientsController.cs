using apbd_09.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_09.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClientWithId(int idClient)
    {
        var client = await _context.Clients.SingleOrDefaultAsync(e => e.IdClient == idClient);
        if (client == null)
        {
            return NotFound($"Client with id - {idClient} doesnt exist");
        }
        else if (client.ClientTrips.Count == 0)
        {
            return BadRequest("Client has assigned trips");
        }
        else
        {
            _context.Remove(client);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
    
}