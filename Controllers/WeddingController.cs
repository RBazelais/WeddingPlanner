using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;
using System;
using System.Collections.Generic;

namespace WeddingPlanner.Controllers
{
	public class WeddingController : Controller
	{
		public bool ErrorMessage { get; private set; }
		
		private WeddingPlannerContext _context;
 
		public WeddingController(WeddingPlannerContext context)
		{
			_context = context;
		}

		[HttpGet]
		[Route("Dashboard")]
		public IActionResult Dashboard()
		{
			var AreTheyLoggedIn = LoginState();
			if (AreTheyLoggedIn == true){
				// Make sure user is stored in session
				int? LoginId = HttpContext.Session.GetInt32("CurrentUser");
				//If user is logged in
				if((int)LoginId != 0)
				{
					User thisUser = GetUserInfo();
					List<Wedding> AllWeddings = _context.Weddings.Include(a => a.Guests).ToList();
					ViewBag.UserInfo = thisUser;
					ViewBag.UserId = thisUser.UserId;
					ViewBag.AllWeddings = AllWeddings;

					// ----- Test Razor foreach loops here
					// foreach(var wedding in ViewBag.AllWeddings)
					// {
					// 	Console.WriteLine(wedding.Guests.Count);
					// 	if(wedding.UserId == ViewBag.UserId)
					// 	{
					// 		Console.WriteLine("Delete");
					// 	}
					// 	else
					// 	{
					// 		bool attending = false;
					// 		foreach(var guest in wedding.Guests)
					// 		{
					// 			if( guest.UserId == ViewBag.UserId)
					// 			{
					// 				attending = true;
					// 				break;
					// 			}
					// 		}
					// 		if(attending == true)
					// 		{
					// 			Console.WriteLine("UN-RSVP");

					// 		}
					// 		else
					// 		{
					// 			Console.WriteLine("RSVP");
					// 		}
					// 	}
					// }
					// ViewBag.UserTransactions = currentUser.Transactions.OrderByDescending(t => t.);
					
				}
				return View("Dashboard");
			}
			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		[Route("NewWedding")]
		public IActionResult NewWedding()
		{
			ViewData["Message"] = "Your application description page.";
			GetUserInfo();
			int? LoginId = HttpContext.Session.GetInt32("CurrentUser");
			return View("NewWedding");
		}

		[HttpGet]
		[Route("DisplayWedding/{id}")]
		public IActionResult DisplayWedding(int id)
		{

			// Wedding thisWedding = _context.Weddings.Include(a => a.Guests).SingleOrDefault(b => b.WeddingId == id);
			List<Wedding> thisWedding = _context.Weddings.Where(a => a.WeddingId == id).Include(b => b.Guests).ThenInclude(c => c.User).ToList();
			// List<Wedding> AllGuestsAttendingWedding = _context.Weddings.Where(a => a.WeddingId == id).Include(b => b.User).ToList();
			
			if(thisWedding.Count == 1)
			{
				ViewBag.EventInfo = thisWedding[0];
				Console.WriteLine("****** FOUND the wedding ******");
				return View("DisplayWedding");
			}
			else{
				TempData["Message"] = "Unable to display this wedding, double check if it is still happening";
				return RedirectToAction("Dashboard");
			}
			
		}

		[HttpPost]
		[Route("CreateWedding")]
		public IActionResult CreateWedding(WeddingViewModel AddWedding)
		{
			Console.WriteLine("********** Enter Create Wedding Process ***********");
			// HttpContext.Session.GetInt32("CurrentEvent");

			TryValidateModel(AddWedding);

			if(ModelState.IsValid)
			{
				//Get in session UserId
				User UserInfo = GetUserInfo();
				int? CurrUserId = HttpContext.Session.GetInt32("CurrentUser");
				ViewData["UserId"] = UserInfo.UserId;
				ViewData["FirstName"] = UserInfo.FirstName;

				Wedding OneWedding = new Wedding
				{
					P1Name = AddWedding.P1Name,
					P2Name = AddWedding.P2Name,
					Address = AddWedding.Address,
					Date = AddWedding.Date,
					UserId = UserInfo.UserId,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now
				};

				_context.Weddings.Add(OneWedding);
				_context.SaveChanges();
				HttpContext.Session.SetInt32("CurrentEvent", OneWedding.WeddingId);
				// Console.WriteLine();
				Wedding EventInfo = GetEventInfo();
				// return RedirectToAction("DisplayWedding", "Wedding");
				return RedirectToAction("DisplayWedding", new { id = OneWedding.WeddingId });
			}
			else
			{
				Console.WriteLine("ERROR: Wedding NOT created, Fail");
				TempData["Message"] = "Something is invalid here...";
				return View("NewWedding");
			}
			
		}

		[HttpGet]
		[Route("AddRSVP/{id}")]
		public IActionResult AddRSVP(int id)
		{
			Console.WriteLine("********** Enter Add Guest ***********");
			int? CurrUserId = HttpContext.Session.GetInt32("CurrentUser");
			
			// Construct the guest object
			Guest AttendingGuest = new Guest { 
				WeddingId = id, 
				UserId = (int)CurrUserId
			};

			// Guest RetrievedGuest = _context.Guests.SingleOrDefault(user => user.UserId == CurrUserId);
			
			_context.Guests.Add(AttendingGuest);
			_context.SaveChanges();

			Console.WriteLine("********** Success ***********");
			Console.WriteLine(AttendingGuest);
			Console.WriteLine("**********  has RSVP to Wedding ***********");
			return RedirectToAction("Dashboard");
			
		}

		[HttpGet]
		[Route("RemoveRSVP/{id}")]
		public IActionResult RemoveRSVP(int id)
		{
			int? CurrUserId = HttpContext.Session.GetInt32("CurrentUser");
			Guest thisGuest = _context.Guests.Where(a => a.WeddingId == id && a.UserId == CurrUserId).SingleOrDefault();
			_context.Guests.Remove(thisGuest);
			_context.SaveChanges();
			Console.WriteLine("********* You Un-RSVPed *********");
			return RedirectToAction("Dashboard");

		}

		[HttpGet]
		[Route("Delete/{id}")]
		public IActionResult Delete(int id)
		{
			Wedding thisWedding = _context.Weddings.SingleOrDefault(a => a.WeddingId == id);
			_context.Weddings.Remove(thisWedding);
			_context.SaveChanges();
			Console.WriteLine("********* Wedding Deleted *********");
			return RedirectToAction("Dashboard");
		}

		private Wedding GetEventInfo()
		{
			int? EventId = HttpContext.Session.GetInt32("CurrentEvent");
			if((int)EventId != 0)
			{
				Wedding thisWedding = _context.Weddings.SingleOrDefault(Event => Event.WeddingId == EventId);
				@ViewBag.EventInfo = thisWedding;
				return thisWedding;
			}
			else
			{
				return null;
			}
		}

		private User GetUserInfo()
		{
			int? LoginId = HttpContext.Session.GetInt32("CurrentUser");
			if((int)LoginId != 0)
			{
				User thisUser = _context.Users.SingleOrDefault(User => User.UserId == LoginId);
				@ViewBag.UserInfo = thisUser;
				return thisUser;
			}
			else
			{
				return null;
			}
		}

		public bool LoginState()
		{
			// convert null to 0 aka false, noone is logged in
			if (HttpContext.Session.GetInt32("CurrentUser") == null)
			{
				@ViewBag.LoginState = false;
				HttpContext.Session.SetInt32("CurrentUser", 0);
				return false;
			}
			// if login state is greater than zero a user is currently logged in
			else if(HttpContext.Session.GetInt32("CurrentUser") > 0)
			{
				@ViewBag.LoginState = true;
				return true;
			}
			// for weird senarios where you're not logged in at all
			else
			{
				@ViewBag.LoginState = false;
				return false;
			}
		}

		public bool AttendWedding()
        {
            // convert null to 0 aka false, noone is logged in
            if (@ViewBag.AttendWedding == null)
            {
                @ViewBag.AttendWedding = false;
                return false;
            }
            // if RSVP hass been added set to truw
            else if(@ViewBag.AttendWedding == true)
            {
                @ViewBag.AttendingWedding = true;
                return true;
            }
            // for weird senarios where you're not logged in at all
            else
            {
                @ViewBag.AttendWedding = false;
                return false;
            }
        }

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

	}
}