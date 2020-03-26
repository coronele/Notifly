using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NotiflyV0._1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Twillo_Test.Controllers;

namespace NotiflyV0._1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly NotiflyDbContext _context;
        private readonly string YelpKey;
   
        public HomeController(NotiflyDbContext context, IConfiguration configuration)
        {
            _context = context;
            YelpKey = configuration.GetSection("APIKeys")["Yelp"];
        }

        public IActionResult Index()
        {

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                UserInfo userInfo = _context.UserInfo.Where(x => x.UserId == id).First();
                if (userInfo == null)
                {
                    throw new System.Exception();
                }
                else
                {
                    return View();
                }

            }
            catch (System.Exception)
            {

                return RedirectToAction("AddUserInfo");
            }

        }

        [HttpGet]
        public IActionResult AddUserInfo()
        {

            return View();
        }
        [HttpPost]
        public IActionResult AddUserInfo(string firstName, string lastName, string phoneNumber)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserInfo userInfo = new UserInfo(firstName, lastName, id, phoneNumber);
            _context.UserInfo.Add(userInfo);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }



        public IActionResult Events()
        {
            try
            {
                string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                UserInfo userInfo = _context.UserInfo.Where(x => x.UserId == id).First();
                if (userInfo == null)
                {
                    throw new System.Exception();
                }
                else
                {
                    List<EventTable> events = _context.EventTable.Where(x => x.UserId == id).ToList();
                    return View(events);
                }

            }
            catch (System.Exception)
            {

                return RedirectToAction("AddUserInfo");
            }



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

        public async Task<IActionResult> EventDetails(int eventId)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.yelp.com/v3/businesses/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {YelpKey}");

            EventTable foundEvent = _context.EventTable.Where(x => x.EventId == eventId).First();

            var searchResponse = await client.GetAsync($"search?term={foundEvent.Venue}&location={foundEvent.VenueLocation}&sortby=best_match");
            var foundLocation = await searchResponse.Content.ReadAsAsync<YelpSearchObject>();

            ViewBag.thisEvent = foundEvent;

            if ((foundLocation == null) || (foundLocation.total == 0))
            {
                YelpDetailObject ydo = new YelpDetailObject();
                return View(ydo);
            }
            else
            {
                var searchDetailResponse = await client.GetAsync($"{foundLocation.businesses.First().id}");
                var foundDetails = await searchDetailResponse.Content.ReadAsAsync<YelpDetailObject>();
                return View(foundDetails);
            }
        }

        [HttpGet]
        public IActionResult AddEventToDatabase()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<Groups> groupList = _context.Groups.Where(x => x.UserId == id).ToList();
            return View(groupList);
        }

        [HttpPost]
        public IActionResult AddEventToDatabase(EventTable newEvent, DateTime eventDate, TimeSpan eventTime, int groupId)
        {
            newEvent.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            bool badDate = false;

            List<EventTable> eventList = _context.EventTable.Where(x => x.UserId == newEvent.UserId).ToList();

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<Groups> groupList = _context.Groups.Where(x => x.UserId == id).ToList();

            try
            {
                if (ModelState.IsValid)
                {
                    Groups selectedGroup = _context.Groups.Where(x => x.GroupId == groupId).FirstOrDefault();
                    newEvent.GroupId = selectedGroup.GroupId;
                    newEvent.GroupName = selectedGroup.GroupName;
                    newEvent.DateAndTime = eventDate + eventTime;

                    TimeZoneInfo myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);

                    // Date and Time check
                    if (newEvent.DateAndTime.CompareTo(currentDateTime) < 1)
                    {
                        badDate = true;
                        throw new Exception();
                    }

                    _context.EventTable.Add(newEvent);
                    _context.SaveChanges();

                    return RedirectToAction("Events");
                }
                else
                {
                    ViewBag.ErrorMsg = "Invalid Data. Please try again.";
                    return View(groupList);
                }
            }
            catch
            {
                if (badDate)
                {
                    ViewBag.ErrorMsg = "Date and time entered are before the current date and time. Please try again.";
                    return View(groupList);
                }
                else
                {
                    ViewBag.ErrorMsg = "Unknown error";
                    return View(groupList);
                }
            }
        }

        [HttpGet]
        public IActionResult EditEvent(int eventid)
        {
            EventTable findEvent = _context.EventTable.Find(eventid);
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.groupList = _context.Groups.Where(x => x.UserId == id).ToList();

            if (findEvent != null)
            {
                return View(findEvent);
            }
            return RedirectToAction("Events");
        }

        [HttpPost]
        public IActionResult EditEvent(EventTable editEvent, DateTime eventDate, TimeSpan eventTime, int groupId)
        {
            EventTable dbEvent = _context.EventTable.Find(editEvent.EventId);
            List<EventTable> eventTableList = _context.EventTable.ToList();

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            bool badDate = false;

            DateTime oldDateandTime = DateTime.MinValue;

            try
            {
                if (ModelState.IsValid)
                {
                    Groups selectedGroup = _context.Groups.Where(x => x.GroupId == editEvent.GroupId).FirstOrDefault();
                    dbEvent.GroupId = selectedGroup.GroupId;
                    dbEvent.GroupName = selectedGroup.GroupName;
                    oldDateandTime = dbEvent.DateAndTime;
                    dbEvent.DateAndTime = eventDate + eventTime;
                    dbEvent.Venue = editEvent.Venue;
                    dbEvent.VenueLocation = editEvent.VenueLocation;
                    dbEvent.UserId = editEvent.UserId;
                    dbEvent.EventName = editEvent.EventName;

                    TimeZoneInfo myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);

                    // Date and Time check
                    if (dbEvent.DateAndTime.CompareTo(currentDateTime) < 1)
                    {
                        badDate = true;
                        throw new Exception();
                    }
                    _context.Entry(dbEvent).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.Update(dbEvent);
                    _context.SaveChanges();
                    return RedirectToAction("Events");
                }
                else
                {
                    ViewBag.groupList = _context.Groups.Where(x => x.UserId == id).ToList();
                    ViewBag.ErrorMsg = "Invalid Data. Please try again.";
                    return View(editEvent);
                }
            }
            catch
            {
                if (badDate)
                {
                    editEvent.DateAndTime = oldDateandTime;
                    ViewBag.groupList = _context.Groups.Where(x => x.UserId == id).ToList();
                    ViewBag.ErrorMsg = "Date and time entered are before the current date and time. Please try again.";
                    return View(editEvent);
                }
                else
                {
                    ViewBag.groupList = _context.Groups.Where(x => x.UserId == id).ToList();
                    ViewBag.ErrorMsg = "Unknown error(0)";
                    return View(editEvent);
                }
            }
        }

        public IActionResult Groups()
        {

            try
            {
                string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                UserInfo userInfo = _context.UserInfo.Where(x => x.UserId == id).First();
                if (userInfo == null)
                {
                    throw new System.Exception();
                }
                else
                {
                    List<Groups> groups = _context.Groups.Where(x => x.UserId == id).ToList();

                    return View(groups);
                }

            }
            catch (System.Exception)
            {

                return RedirectToAction("AddUserInfo");
            }

        }

        [HttpGet]
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



        public IActionResult RemoveMember(int memberId)
        {
            //In the ListGroups View, plan is to create a details button that will display the list
            //of members participating in the group that was selected. 7
            //This way, you could remove any individuals off the list. 
            GroupMembers findMember = _context.GroupMembers.Find(memberId);

            if (findMember != null)
            {
                _context.GroupMembers.Remove(findMember);
                _context.SaveChanges();
            }

            return RedirectToAction("GroupDetails");
        }

        [HttpGet]
        public IActionResult EditMember(int memberid)
        {
            GroupMembers findMember = _context.GroupMembers.Find(memberid);
            if (findMember != null)
            {
                return View(findMember);
            }
            return View();
        }

        [HttpPost]
        public IActionResult EditMember(GroupMembers editMember)
        {
            GroupMembers dbGroupMember = _context.GroupMembers.Find(editMember.MemberId);

            if (ModelState.IsValid)
            {
                dbGroupMember.MemberName = editMember.MemberName;
                dbGroupMember.PhoneNumber = editMember.PhoneNumber;

                _context.Entry(dbGroupMember).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.Update(dbGroupMember);
                _context.SaveChanges();
                int groupId = editMember.Groups;
                return GroupDetails(groupId);
            }
            return GroupDetails(editMember.Groups);
        }

        [HttpGet]
        public IActionResult GroupDetails(int groupId)
        {
            List<GroupMembers> members = _context.GroupMembers.Where(x => x.Groups == groupId).ToList();
            return View(members);
        }



        public IActionResult RemoveGroup(int groupId)
        {
            Groups foundGroup = _context.Groups.Find(groupId);

            List<EventTable> events = _context.EventTable.Where(x => x.GroupId == groupId).ToList();

            if (events.Count > 0)
            {
                foreach (var e in events)
                {
                    _context.EventTable.Remove(e);
                    _context.SaveChanges();
                }
            }


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


        public IActionResult Tutorial()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddMember(int groupId)
        {

            return View(groupId);
        }

        [HttpPost]
        public IActionResult AddMember(GroupMembers newMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(newMember);
                _context.SaveChanges();
                return RedirectToAction("Groups");
            }
            else
            {
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }






    }
}

