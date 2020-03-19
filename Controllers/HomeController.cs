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
            if (find != null)
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
        
        
        //public IActionResult CreateEvent()
        //{


        //}

        public IActionResult Groups()
        {
            
            return View(_context.Groups.ToList());


        }

        [HttpGet] //Do i need this part?
        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateGroup(string groupName)
        {
            //Create a form that will add a new group to the events table
            //but also would like it to display the list of groups

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Groups newGroup = new Groups(groupName, id);


            _context.Groups.Add(newGroup);

            _context.SaveChanges();

            return RedirectToAction("CreateGroupMember", newGroup);



        }



        [HttpGet]
        public IActionResult CreateGroupMember(Groups newGroup)
        {
            ViewBag.GroupMembers = _context.GroupMembers.Where(x => x.Groups == newGroup.GroupId.ToString()).ToList(); 
            
            ViewBag.GroupId = newGroup.GroupId;

            ViewBag.Counter = 0;

            return View(newGroup);

        }


        
        [HttpPost]
        public IActionResult CreateGroupMember(string memberName, string phoneNumber, string groupId, int counter)
        {

            GroupMembers newMember = new GroupMembers(memberName, groupId, phoneNumber);

            _context.GroupMembers.Add(newMember);
            _context.SaveChanges();

            int groupIdNumber = int.Parse(groupId);
            Groups group = _context.Groups.Find(groupIdNumber);
            
            ViewBag.GroupMembers = _context.GroupMembers.Where(x => x.Groups == group.GroupId.ToString()).ToList();
            ViewBag.GroupId = groupIdNumber;
            ViewBag.Counter = counter;

            return View("CreateGroupMember");

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


       
        public IActionResult RemoveGroup(int groupId) 
        {
            Groups foundGroup = _context.Groups.Find(groupId);

            if (foundGroup != null)
            {
                _context.Groups.Remove(foundGroup);
                _context.SaveChanges();
            }

            return RedirectToAction("Groups");
        }

        public IActionResult GetExplanation()
        {
            return View();
        }

        public IActionResult CheckRsvp()
        {
            return View(_context.MemberRsvp.ToList());
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
