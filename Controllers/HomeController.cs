using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            return View();
        }

        public IActionResult Events()
        {
            return View(_context.EventTable.ToList());
        }

        public IActionResult DeleteEvent(int id)
        {
            //Created a button in the Events View to use this function. 
            EventTable find = _context.EventTable.Find(id);
            if(find != null)
            {
                _context.Remove(find);
                _context.SaveChanges();
            }
            return RedirectToAction("Events");
        }

        //public IActionResult CreateEvent()
        //{
        //    return View();
        //}

        [HttpPost]
        //public IActionResult CreateEvent()
        //{


        //}

        public IActionResult ListOfGroups()
        {
            return View(_context.Groups.ToList());
        }

        [HttpGet] //Do i need this part?
        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateGroup(Groups addGroup)
        {
            //Create a form that will add a new group to the events table
            //but also would like it to display the list of groups
            _context.Groups.Add(addGroup);
            _context.SaveChanges();
            return RedirectToAction("ListOfGroups");
        }

        public IActionResult RemoveMember(int id)
        {
            //In the ListGroups View, plan is to create a details button that will display the list
            //of members participating in the group that was selected. 7
            //This way, you could remove any individuals off the list. 
            Groups findMember = _context.Groups.Find(id);

            if (findMember != null)
            {
                _context.Groups.Remove(findMember);
                _context.SaveChanges();
            }

            return RedirectToAction("Groups");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
