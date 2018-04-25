using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
		public bool ErrorMessage { get; private set; }
        private WeddingPlannerContext _context;
 
        public HomeController(WeddingPlannerContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegisterViewModel regUser)
        {
            Console.WriteLine("********** Enter Register Process ***********");

            // validate model
            TryValidateModel(regUser);
            // set user 
            //set login Id to session
            // int? LoginId = HttpContext.Session.GetInt32("LoggedIn");

            // checks results from user with the email

            // if model is valid create new user
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                // pass in whole object through params
                User NewUser = new User
                {
                    FirstName = regUser.FirstName,
                    LastName = regUser.LastName,
                    Email = regUser.Email,
                    Password = regUser.Password,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow

                };

                // Hash password
                Console.WriteLine(NewUser.Password);
                NewUser.Password = Hasher.HashPassword(NewUser, NewUser.Password);
                Console.WriteLine("Hashed? ********** " + NewUser.Password + " **********");

                // if all info check out proceed to Dashboard account page
                _context.Add(NewUser);
                _context.SaveChanges();

                //compare the emails in the database to the email just entered
                NewUser = _context.Users.SingleOrDefault(User => User.Email == NewUser.Email);
                HttpContext.Session.SetInt32("CurrentUser", NewUser.UserId);
                return RedirectToAction("Dashboard", "Wedding");

            }
            return View("Index");
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            ViewData["Message"] = "Planning a wedding~";
            return View("Login");
        }

        [HttpPost]
        [Route("LoginUser")]
        public IActionResult LoginUser(LoginViewModel thisUser)
        {
            // validate model
            TryValidateModel(thisUser);
            // Attempt to retrieve a user from your database based on the Email submitted
            User DbInfo = _context.Users.Where(User => User.Email == thisUser.Email).SingleOrDefault();

            // checked if password matches password in database
            if(DbInfo != null)
            {
                var Hasher = new PasswordHasher<User>();
                // Pass the user object, the hashed password, and the PasswordToCheck
                if(0 != Hasher.VerifyHashedPassword(DbInfo, DbInfo.Password, thisUser.Password))
                {
                    Console.WriteLine("************ SUCCESSFULL LOGIN ************");
                    HttpContext.Session.SetInt32("CurrentUser", DbInfo.UserId);
                    _context.SaveChanges();
                    return RedirectToAction("Dashboard", "Wedding");
                }
                else
                {
                    LoginState();
                    ViewData["Message"] = "Incorrect Email or Password";
                    return View("Login");
                }
            }
            else{
                LoginState();
                ViewData["Message"] = "ERROR";
                return View("Login");
            }
        }

    

        [HttpGet]
        [Route("Home")]
        public IActionResult Home()
        {
            // Make sure user is stored in session
            int? LoginId = HttpContext.Session.GetInt32("CurrentUser");
            //If user is logged in
            if((int)LoginId > 0)
            {
                // User currentUser = _context.Users.Include(u => u.Transactions).SingleOrDefault(User => User.UserId == LoginId);
                // ViewBag.UserInfo = currentUser;
                // ViewBag.UserTransactions = currentUser.Transactions.OrderByDescending(t => t.);
                if(TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                else
                {
                    ViewBag.Message = "Please enter an amount";
                }
                return View("Dashboard", "Wedding");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost]
        [Route("UserAction")]
        public IActionResult UserAction(Wedding NewWedding)
        {

            // User UserInfo = GetInfo();

            // 
            // if( UserInfo.Balance >= ( (-1) * NewTransaction.Amount ) )
            // {

            //     double? NewBalance = UserInfo.Balance + NewTransaction.Amount;
            //     UserInfo.Balance = (double)NewBalance;
            //     _context.SaveChanges();

            //     Console.WriteLine("TRANSACTION MADE, NEW BALANCE: " + NewBalance);

            //     Transaction CreatedTransaction = new Transaction
            //     {
            //         UserId = (int)HttpContext.Session.GetInt32("CurrentUser"),
            //         Amount = NewTransaction.Amount,
            //         Date = DateTime.UtcNow
            //     };
            //     TempData["Message"] = "Successful transaction";

            //     _context.Transactions.Add(CreatedTransaction);
            //     _context.SaveChanges();
            // }
            // else
            // {
            //     System.Console.WriteLine("ERROR OCCURRED, FAILED TRANSACTION");
            //     TempData["Message"] = "Insufficient funds";
            // }
            
            return RedirectToAction("Dashboard", "Weddings");
        }


        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            // assumes user is no longer connected to the server
            HttpContext.Session.SetInt32("LoginState", 0);
            return View("Index");
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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

        private User GetInfo()
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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
