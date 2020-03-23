using System;
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
            try
            {
                string[] messageParts = incomingMessage.Body.Split(" ");
                if (messageParts[0].ToLower() == "yes" || messageParts[0].ToLower() == "no" || messageParts[0].ToLower() == "n" || messageParts[0].ToLower() == "y")
                {

                    AddRSVPToDataBase(incomingMessage);

                }

                else if (messageParts[0] == "?" || messageParts[0].ToLower() == "h")
                {
                    SendText($"This is Notifly's format for creating events. Everything must be on a new line: \nEvent Name \nEvent Date, (ex. 05/29/2015 5:50 AM) \nThe Event Venue (ex. 'Red Robin') \nEvent Location (ex. 'Trenton', MI or '48183') \nThe Group Name \nBefore you create an event, you'll need to create a group. Visit notifly.azurewebsites.net to learn more.", incomingMessage.From);
                    SendText($"", incomingMessage.From);

                }
                else if (incomingMessage.Body.ToLower() == "events" || incomingMessage.Body.ToLower() == "event" || incomingMessage.Body.ToLower() == "e")
                {
                    SendListOfEvents(incomingMessage);
                }
                else if (messageParts[0].ToLower() == "r" || messageParts[0].ToLower() == "remind")
                {
                    SendReminder(incomingMessage, messageParts[1]);
                }
                else if(messageParts[0] == "create" || messageParts[0] == "new")
                {
                    CreateGroup(incomingMessage);
                }
                //else if(messageParts[0] == "delete")
                //{
                //    if(messageParts[1] == "event")
                //    {
                //        DeleteEvent();
                //    }
                //    else if(messageParts[1] == "group")
                //    {
                //        DeleteGroup();
                //    }
                //}
                else
                {
                    AddEventToDatabase(incomingMessage);
                }

                //isn't doing anything
                var messagingResponse = new MessagingResponse();

                return TwiML(messagingResponse);
            }
            catch (Exception)
            {
                SendErrorText(incomingMessage, null);
                var messagingResponse = new MessagingResponse();
                return TwiML(messagingResponse);
            }


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


        public void AddEventToDatabase(SmsRequest incomingMessage)
        {
            bool badDate = false;
            bool unknownGroup = false;
            bool badFormat = false;
            bool badNumber = false;

            try
            {
                string userPhoneNumber = incomingMessage.From;
                var user = _context.AspNetUsers.Where(x => x.PhoneNumber == userPhoneNumber).First();
                if (user == null)
                {
                    Exception unknownUser = new Exception();
                    badNumber = true;
                    throw unknownUser;
                }

                StringReader reader = new StringReader(incomingMessage.Body);
                string line = reader.ReadLine();
                List<string> textParts = new List<string>();

                while (line != null)
                {
                    textParts.Add(line);
                    line = reader.ReadLine();
                }

                string userEvent = textParts[0];

                //Date Time Format: ("MM/dd/yyyy h:mm tt") (05/29/2015 5:50 AM)


                DateTime eventDateTime = DateTime.Parse(textParts[1]);
                string eventVenue = textParts[2];
                string eventLoc = textParts[3];
                string groupName = textParts[4];

                //list of user's groups
                List<Groups> userGroups = _context.Groups.Where(x => x.UserId == user.Id).ToList();

                //list of group names
                List<string> groupNames = new List<string>();
                foreach (var u in userGroups)
                {
                    groupNames.Add(u.GroupName);
                }

                //exception handling!
                if (DateTime.Now.Subtract(eventDateTime).TotalSeconds > 0)
                {
                    Exception dateError = new Exception();
                    badDate = true;
                    throw dateError;
                }
                else if (!groupNames.Contains(groupName))
                {
                    Exception nullGroup = new Exception();
                    unknownGroup = true;
                    throw nullGroup;
                }
                else if (textParts.Count != 5)
                {
                    Exception formatError = new Exception();
                    badFormat = true;
                    throw formatError;
                }
                



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
            catch (Exception)
            {
                if (badDate)
                {
                    SendErrorText(incomingMessage, "The date you entered is too early.");
                }
                else if (badFormat)
                {
                    SendErrorText(incomingMessage, "The format you entered is incorrect.");
                }
                else if (unknownGroup)
                {
                    SendErrorText(incomingMessage, "That group doesn't exist.");
                }
                else if (badNumber)
                {
                    SendText("Looks like you don't have an account with us. \nIn order to create events or groups, please sign up at www.notifly.azurewebsites.net \n Have a nice day!", incomingMessage.From);
                }
                else
                {
                    SendErrorText(incomingMessage, null);
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

        public void SendReminder(SmsRequest incomingMessage, string userNumberString)
        {
            bool badNumber = false;
            bool outOfRange = false;


            try
            {
                //check if number entered is in fact a number.
                if (!int.TryParse(userNumberString, out int userNumber))
                {
                    Exception badParse = new Exception();
                    badNumber = true;
                    throw badParse;
                }



                var user = _context.AspNetUsers.Where(x => x.PhoneNumber == incomingMessage.From).First();

                List<EventTable> userEvents = _context.EventTable.Where(x => x.UserId == user.Id).ToList();

                //check if the number sent is too high or too low, since it might be out of range.
                if (userNumber > userEvents.Count || userNumber < 1)
                {
                    outOfRange = true;
                    Exception badRange = new Exception();
                    throw badRange;

                }

                int foundIndex = 0;

                for (int i = 0; i < userEvents.Count; i++)
                {
                    if (i + 1 == userNumber)
                    {
                        foundIndex = i;
                    }
                }

                EventTable userEvent = userEvents[foundIndex];


                TimeSpan timeRemainingForEvent = userEvent.DateAndTime.Subtract(DateTime.Now);
                string timeLeft;
                if (timeRemainingForEvent.TotalDays < 1)
                {
                    if (timeRemainingForEvent.Hours > 1)
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
                    if (timeRemainingForEvent.Days > 1)
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


                List<GroupMembers> eventMembers = _context.GroupMembers.Where(x => x.Groups == userEvent.GroupId).ToList();

                foreach (var e in eventMembers)
                {
                    SendText($"Hey, {e.MemberName}! Just a reminder: You have {timeLeft} until {userEvent.EventName}. Are you still coming? You can still RSVP by texting back with 'yes {userEvent.EventId}' or 'no {userEvent.EventId}'", e.PhoneNumber);
                }
            }
            catch (Exception)
            {
                if (badNumber)
                {
                    SendErrorText(incomingMessage, "That's not a valid event number.");
                    SendListOfEvents(incomingMessage);
                }
                else if (outOfRange)
                {
                    SendErrorText(incomingMessage, "That event doesn't exist.");
                    SendListOfEvents(incomingMessage);
                }


            }

        }

        public void CreateGroup(SmsRequest incomingMessage)
        {
            bool badNumber = false;

            try
            {
                
                var user = _context.AspNetUsers.Where(x => x.PhoneNumber == incomingMessage.From).First();
                if(user == null)
                {
                    Exception unknownUser = new Exception();
                    badNumber = true;
                    throw unknownUser;
                }

                StringReader reader = new StringReader(incomingMessage.Body);
                string line = reader.ReadLine();
                List<string> textParts = new List<string>();

                while (line != null)
                {
                    textParts.Add(line);
                    line = reader.ReadLine();
                }

                List<string> memberNames = new List<string>();
                List<string> memberNumbers = new List<string>();
                for (int i = 2; i < textParts.Count; i++)
                {
                    if(i % 2 == 0)
                    {
                        memberNames.Add(textParts[i]);
                    }
                    else
                    {
                        memberNumbers.Add(textParts[i]);
                    }
                }

                Groups newGroup = new Groups(textParts[1], user.Id);
                _context.Groups.Add(newGroup);
                _context.SaveChanges();

                for (int i = 0; i < memberNames.Count; i++)
                {
                    GroupMembers newMember = new GroupMembers(memberNames[i], newGroup.GroupId, memberNumbers[i]);
                    _context.GroupMembers.Add(newMember);
                    _context.SaveChanges();
                }

            }
            catch (Exception)
            {
                if (badNumber)
                {
                    SendText("Looks like you don't have an account with us. \nIn order to create events or groups, please sign up at www.notifly.azurewebsites.net \n Have a nice day!", incomingMessage.From);
                }
                
            }
        }






        public void SendListOfEvents(SmsRequest incomingMessage)
        {
            var user = _context.AspNetUsers.Where(x => x.PhoneNumber == incomingMessage.From).First();
            List<EventTable> userEvents = _context.EventTable.Where(x => x.UserId == user.Id).ToList();
            string reminderBody = "Hi! These are your events. Would you like to send out reminders? If so, reply to this text with the letter r and the number of the event (ex. 'r 12'). \n";



            for (int i = 0; i < userEvents.Count; i++)
            {
                reminderBody = reminderBody + $"{i + 1}. {userEvents[i].EventName} {userEvents[i].GroupName} {userEvents[i].DateAndTime.ToShortDateString()}\n";
            }

            SendText(reminderBody, incomingMessage.From);

        }

        public void SendErrorText(SmsRequest incomingMessage, string issue)
        {
            if (issue == null)
            {
                SendText("Oh no! :( \nThat didn't work, but no worries! Try again! \nIf you need help, text back 'h' or '?'", incomingMessage.From);
            }
            else
            {
                SendText($"Oh no! :( \n{issue} Try again! \nIf you need help, text back 'h' or '?'", incomingMessage.From);

            }
        }
















    }
}