using System.Collections.ObjectModel;
using apbd_09.Data;
using apbd_09.DTOs;
using apbd_09.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_09.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTripsSortedByStartDate([FromQuery] int? pageNum, int pageSize = 10)
    {
        
        var trips = await _context.Trips.Select(e => new TripDto()
            {
                Name = e.Name,
                Description = e.Description,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                MaxPeople = e.MaxPeople,
                Countries = e.IdCountries.Select(c => new CountryDto()
                {
                    Name = c.Name
                }),
                Clients = e.ClientTrips.Select(c => new ClientDto()
                {
                    FirstName = c.IdClientNavigation.FirstName,
                    LastName = c.IdClientNavigation.LastName
                })
            })
            .OrderBy(e => e.DateFrom)
            .ToListAsync();
        return Ok(trips);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, AddClientToTripDto newClient)
    {
        var receivedAt = DateTime.Now;
        var client = await _context.Clients.SingleOrDefaultAsync(e => e.Pesel == newClient.Pesel);
        var trip = await _context.Trips.SingleOrDefaultAsync(e => e.IdTrip == idTrip);
         if (client != null)
        {
            if (client.ClientTrips.Any(e => e.IdTrip == idTrip))
            {
                return BadRequest($"Client with given PESEL - {newClient.Pesel} already has trip with given id ({idTrip}) assigned");
            }
            return BadRequest($"Client with PESEL - {newClient.Pesel} already exists");
        }

        if (trip != null)
        {
            if (trip.DateFrom > DateTime.Now)
            {
                client = new Client()
                {
                    IdClient = await _context.Clients.CountAsync() + 1,
                    FirstName = newClient.FirstName,
                    LastName = newClient.LastName,
                    Email = newClient.Email,
                    Pesel = newClient.Pesel,
                    Telephone = newClient.Telephone
                };
                client.ClientTrips.Add(new ClientTrip()
                {
                    IdClient = client.IdClient,
                    IdTrip = trip.IdTrip,
                    PaymentDate = newClient.PaymentDate,
                    RegisteredAt = receivedAt,
                    IdClientNavigation = client,
                    IdTripNavigation = trip
                });
                return Ok();
            }
            else return BadRequest($"Trip has already started {trip.DateFrom}");
        }
        else return NotFound($"Trip with given Id - {idTrip} doesn't exist");
    }
    
}