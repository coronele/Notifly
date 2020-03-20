using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotiflyV0._1.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

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
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<EventTable> events = _context.EventTable.Where(x => x.UserId == id).ToList();

            return View(events);
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

        [HttpGet] 
        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateGroup(string groupName)
        {
           

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Groups newGroup = new Groups(groupName, id);


            _context.Groups.Add(newGroup);

            _context.SaveChanges();

            return RedirectToAction("CreateGroupMember", newGroup);



        }

        



        [HttpGet]
        public IActionResult CreateGroupMember(Groups newGroup)
        {
            ViewBag.GroupMembers = _context.GroupMembers.Where(x => x.Groups == newGroup.GroupId).ToList(); 
            
            ViewBag.GroupId = newGroup.GroupId;

            ViewBag.Counter = 0;

            return View(newGroup);

        }


        
        [HttpPost]
        public IActionResult CreateGroupMember(string memberName, string phoneNumber, int groupId, int counter)
        {

            GroupMembers newMember = new GroupMembers(memberName, groupId, phoneNumber);

            _context.GroupMembers.Add(newMember);
            _context.SaveChanges();

            
            Groups group = _context.Groups.Find(groupId);
            
            ViewBag.GroupMembers = _context.GroupMembers.Where(x => x.Groups == group.GroupId).ToList();
            ViewBag.GroupId = groupId;
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

            //!!!!!!!!!!!!NEEDS VALIDATION FOR SEEING IF CURRENT EVENT EXISTS AND DELETING ALL EVENTS ASSOCIATED WITH GROUP!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            


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

        public IActionResult CheckRsvp(int groupId, int eventId)
        {
            ViewBag.GroupMembers = _context.GroupMembers.Where(x => x.Groups == groupId).ToList();
            ViewBag.Event = _context.EventTable.Find(eventId);

            List<MemberRsvp> total = _context.MemberRsvp.Where(x => x.EventId == eventId).ToList();
            ViewBag.NumberOfYes = _context.MemberRsvp.Where(x => x.Rsvp == true).Where(x => x.EventId == eventId).ToList().Count;
            ViewBag.NumberOfNo = _context.MemberRsvp.Where(x => x.Rsvp == false).Where(x => x.EventId == eventId).ToList().Count;


            return View(total);

        }


        public int GetRsvpPercentage(int replies, int totalResponses)
        {
            if(totalResponses > 0)
            {
                return (replies / totalResponses) * 100;

            }
            else
            {
                return 0;
            }
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
