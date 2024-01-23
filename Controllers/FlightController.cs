using FlightBookingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingApp.Controllers;

public class FlightController : Controller
{
    private readonly PostgresContext _db;
    private readonly ILogger<FlightController> _logger;

    public FlightController(PostgresContext db,ILogger<FlightController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(_db.Flights.OrderBy(f=>f.Name));
    }
    
    public IActionResult Add()
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };
        return View();
    }

    [HttpPost]
    public IActionResult Add(Flight newFlight)
    {
        
        //* repopulating
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };
        try
        {
            if (!ModelState.IsValid)
            {
                return View(newFlight);
            }
            Flight? existingFlight = _db.Flights.FirstOrDefault(f => f.Code.ToLower().Equals(newFlight.Code.ToLower()));
            if (existingFlight != null)
            {
                ModelState.AddModelError("Code","A flight with this code already exists");
                return View(existingFlight);
            }

            if (newFlight.Arrivaltime <= newFlight.Departuretime)
            {
                ViewBag.ErrorMessage = "Arrival time cannot be less than departure time";
                return View(existingFlight);

            }

            if (newFlight.Departuretime < DateTime.Now)
            {
                ModelState.AddModelError("Departuretime","Select a future date");
                return View(existingFlight);
            }
            if (newFlight.Arrivaltime < DateTime.Now)
            {
                ModelState.AddModelError("Arrivaltime","Select a future date");
                return View(existingFlight);

            }
            
            // Console.WriteLine(newFlight);
            newFlight.Code = newFlight.Code.ToUpper();
            _db.Flights.Add(newFlight);
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Flight Added successfully.";
            return RedirectToAction("Admin","Customer");
        }
        catch (Exception e)
        {
            ViewBag.ErrorMessage="An error occurred while creating the account. Please try again.";
            return View(newFlight);
        }
    }
    
    public IActionResult Details(int id)
    {
        try
        {
            Flight? f = _db.Flights.Find(id);

            if (f == null)
            {
                TempData["ErrorMessage"] = "Flight does not exist";
                return RedirectToAction("Index");
            }
            else
            {
                return View(f);
            }
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the flight details. Please try again.";
            return RedirectToAction("Index");
        }
    }
    
    public IActionResult Edit(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };
        try
        {
            Flight? f = _db.Flights.Find(id);

            if (f == null)
            {
                TempData["ErrorMessage"] = "Flight does not exist";
                return RedirectToAction("Index");
            }
            else
            {
                return View(f);
            }
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the flight details. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult Edit(Flight upflight)
    {
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };

        try
        {
            Flight? existingFlight = _db.Flights.AsNoTracking().FirstOrDefault(f => f.Flightid == upflight.Flightid);
            if (existingFlight == null)
            {
                TempData["ErrorMessage"] = "Flight does not exist";
                return RedirectToAction("Index");
            }
            
            Flight? existingFlightWithSameCode = _db.Flights.FirstOrDefault(f => f.Code.ToLower().Equals(upflight.Code.ToLower()) && f.Flightid!=upflight.Flightid);
            if (existingFlightWithSameCode != null)
            {
                ModelState.AddModelError("Code","A flight with this code already exists");
                return View(upflight);
            }

            _db.Flights.Update(upflight);
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Flight updated successfully.";


            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"]="An error occurred while fetching the flight details. Please try again.";
            return RedirectToAction("Index");
        }
        
    }

    public IActionResult Cancel(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        } 
        try
        {
            Flight? f = _db.Flights.Find(id);

            if (f == null)
            {
                TempData["ErrorMessage"] = "Flight does not exist";
                return RedirectToAction("Index");
            }
            else
            {
                return View(f);
            }
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the flight details. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ActionName("Cancel")]
    public IActionResult CancelConfirmed(int id)
    {
        try
        {
            Flight? f = _db.Flights.Find(id);
    
            if (f == null)
            {
                TempData["ErrorMessage"] = "Flight does not exist";
                return RedirectToAction("Index");
            }

            f.Status = "Cancelled";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Flight Cancelled successfully.";
                return RedirectToAction("Index");
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the flight details. Please try again.";
            return RedirectToAction("Index");
        }
    }
    
}