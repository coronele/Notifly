﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NotiflyV0._1.Models;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;


namespace Twillo_Test.Controllers
{

    public class SmsController : TwilioController
    {
        private readonly string TwilioAccountSid;
        private readonly string TwilioAuthToken;
        private readonly NotiflyDbContext _context;


        //TWILIO NEEDS THIS FORMAT FOR PHONE NUMBER +1734###6670




        public SmsController(IConfiguration configuration, NotiflyDbContext context)
        {
            TwilioAccountSid = configuration.GetSection("APIKeys")["TwilioAccountSid"];
            TwilioAuthToken = configuration.GetSection("APIKeys")["TwilioAuthToken"];
            _context = context;
        }





        [HttpPost]
        public TwiMLResult ReceiveText(SmsRequest incomingMessage)
        {

            string[] messageParts = incomingMessage.Body.Split(" ");
            if (messageParts[0].ToLower() == "yes" || messageParts[0].ToLower() == "no" || messageParts[0].ToLower() == "n" || messageParts[0].ToLower() == "y")
            {

                AddRSVPToDataBase(incomingMessage);

            }

            else if (incomingMessage.Body == "?" || incomingMessage.Body == "h")
            {
                SendText($"This is Notifly's format for creating events. Everything must be on a new line: \n \n Event Name \n Event Date, (ex. 05/29/2015 5:50 AM) \n The Event Venue (ex. 'Red Robin') \n Event Location (ex. 'Trenton', MI or '48183') \n The Group Name", incomingMessage.From);
                SendText($"Please refer to our documentation page on our app on how to create a group so that you can start making events!", incomingMessage.From);

            }

            else
            {
                AddEventToDatabase(incomingMessage);

            }

            //isn't doing anything
            var messagingResponse = new MessagingResponse();

            return TwiML(messagingResponse);

        }


        public void AddRSVPToDataBase(SmsRequest incomingMessage)
        {

            string[] textParts = incomingMessage.Body.Split(" ");

            bool userRsvp = false;

            var foundGroupMember = _context.GroupMembers.Where(x => x.PhoneNumber == incomingMessage.From).First();

            int memberId = foundGroupMember.MemberId;

            var foundevent = _context.EventTable.Where(y => y.EventId == Int32.Parse(textParts[1])).First();

            int eventId = foundevent.EventId;

            string rsvpResponse = textParts[0].ToLower();

            if (rsvpResponse.Contains("yes") || rsvpResponse == "y")
            {
                userRsvp = true;
            }

            //find if rsvp already exists in database
            List<MemberRsvp> rsvpDupes = _context.MemberRsvp.Where(x => x.MemberId == memberId).Where(x => x.EventId == eventId).ToList();

            //delete all found duplicates (should only be one).  
            if (rsvpDupes.Count > 0)
            {
                foreach (var r in rsvpDupes)
                {
                    _context.MemberRsvp.Remove(r);
                    _context.SaveChanges();
                }
            }

            MemberRsvp newRsvp = new MemberRsvp(memberId, eventId, userRsvp, foundGroupMember.MemberName);

            _context.MemberRsvp.Add(newRsvp);
            _context.SaveChanges();

        }


        public void AddEventToDatabase(SmsRequest message)
        {
            string userPhoneNumber = message.From;
            var user = _context.AspNetUsers.Where(x => x.PhoneNumber == userPhoneNumber).First();

            StringReader reader = new StringReader(message.Body);
            string line = reader.ReadLine();
            List<string> textparts = new List<string>();

            while (line != null)
            {
                textparts.Add(line);
                line = reader.ReadLine();
            }

            string userEvent = textparts[0];

            //Date Time Format: ("MM/dd/yyyy h:mm tt") (05/29/2015 5:50 AM)


            DateTime eventDateTime = DateTime.Parse(textparts[1]);
            string eventVenue = textparts[2];
            string eventLoc = textparts[3];
            string groupName = textparts[4];

            Groups group = _context.Groups.Where(x => x.GroupName == groupName).First();

            EventTable newEvent = new EventTable(userEvent, "Description", group.GroupId, eventDateTime, eventVenue, eventLoc, user.Id, group.GroupName, eventDateTime);

            _context.EventTable.Add(newEvent);
            _context.SaveChanges();

            //Code for sending group text

            List<GroupMembers> groupMembers = _context.GroupMembers.Where(x => x.Groups == group.GroupId).ToList();
            EventTable tempEvent = _context.EventTable.Where(x => x.EventName == userEvent).Where(x => x.GroupId == group.GroupId).First();

            if (user != null)
            {
                foreach (var g in groupMembers)
                {
                    SendText($"Hi, {g.MemberName}! You've just been invited to {userEvent} on {eventDateTime.ToString()}  at {eventVenue}, {eventLoc}. Respond with 'yes {tempEvent.EventId}' if you accept, and 'no {tempEvent.EventId}' if you decline.", g.PhoneNumber);
                }
            }
        }




        public void SendText(string body, string number)
        {
            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);

            var messageOptions = new CreateMessageOptions(
            new PhoneNumber(number));
            messageOptions.From = new PhoneNumber("+19854413010");
            messageOptions.Body = body;
            var message = MessageResource.Create(messageOptions);

        }

        public IActionResult SendReminder(List<EventTable> dueEvents)
        {
            foreach (var d in dueEvents)
            {
                if (d.NotificationDate < DateTime.Now)
                {

                    TimeSpan timeRemainingForEvent = DateTime.Now - d.DateAndTime;
                    string timeLeft;
                    if (timeRemainingForEvent.TotalDays < 1)
                    {
                        if(timeRemainingForEvent.Hours > 1)
                        {
                            timeLeft = timeRemainingForEvent.Hours.ToString();
                            timeLeft = timeLeft + " hours";
                        }
                        else
                        {
                            timeLeft = timeRemainingForEvent.Hours.ToString();
                            timeLeft = timeLeft + " hour";
                        }
                        
                    }
                    else
                    {
                        if(timeRemainingForEvent.Days > 1)
                        {
                            timeLeft = timeRemainingForEvent.Days.ToString();
                            timeLeft = timeLeft + " days";
                        }
                        else
                        {
                            timeLeft = timeRemainingForEvent.Days.ToString();
                            timeLeft = timeLeft + " day";
                        }
                        
                    }


                    List<GroupMembers> eventMembers = _context.GroupMembers.Where(x => x.Groups == d.GroupId).ToList();

                    foreach (var e in eventMembers)
                    {
                        SendText($"Hey, {e.MemberName}! Just a reminder: You have {timeLeft} until {d.EventName}. Are you still coming? You can still RSVP by texting back with 'yes {d.EventId}' or 'no {d.EventId}'", e.PhoneNumber);
                    }
                }

                EventTable foundEvent = _context.EventTable.Find(d.EventId);
                
                
                if(foundEvent != null)
                {
                    foundEvent.NotificationDate = DateTime.MinValue;

                    _context.Entry(foundEvent).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.EventTable.Update(foundEvent);
                    _context.SaveChanges();
                }



            }

            return RedirectToAction("Index");

        }







    }
}