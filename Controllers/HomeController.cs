using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotiflyV0._1.Models;



namespace NotiflyV0._1.Controllers
{
    public class HomeController : Controller
    {
        private readonly NotiflyDbContext _context;
        
        public HomeController(NotiflyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _context.AspNetUsers.Find(id);

            if(user.PhoneNumber == null)
            {
                return View("AddPhoneNumber");
            }
            else
            {
                return View();
            }

        }

        public IActionResult AddPhoneNumber(string phoneNumber)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _context.AspNetUsers.Find(id);

            user.PhoneNumber = phoneNumber;

            return RedirectToAction("Index");

        }

        public IActionResult Events()
        {
            return View(_context.EventTable.ToList());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}