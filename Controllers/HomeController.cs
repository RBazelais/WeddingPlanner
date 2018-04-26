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
            User exists = _context.Users.Where(User => User.Email == regUser.Email).SingleOrDefault();
            // if statment that checks results from new user with the email 
            // if (_context.users.Where(u => u.Email == regUser.Email).ToList().Count() > 0)
            if (exists != null)
            {
                //you have to make custom error message it does not exist in previous logic. 
                ViewBag.err = "Email already exist, please try new one.";
                return View("Index");
            }


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
            Console.WriteLine("********** Enter Login Process ***********");
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
                    // custom error message it does not exist in previous logic
                    Console.WriteLine("Incorrect Email or Password");
                    ViewBag.err = "Incorrect Email or Password";
                    return View("Login");
                }
            }
            else{
                LoginState();
                Console.WriteLine("You're not logged in properly. Try Again");
                ViewBag.err = "You're not logged in properly. Try Again";
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
                return View("Dashboard", "Wedding");
            }
            else
            {
                return View("Index");
            }
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
