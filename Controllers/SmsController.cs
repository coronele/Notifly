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

            else if (messageParts[0] == "?" || messageParts[0].ToLower() == "h" || messageParts[0].ToLower() == "help")
            {
                if (messageParts.Length == 1)
                {
                    SendHelpText(incomingMessage, "0");
                }
                else
                {
                    SendHelpText(incomingMessage, messageParts[1]);
                }



            }
            else if (incomingMessage.Body.ToLower() == "events" || incomingMessage.Body.ToLower() == "event" || incomingMessage.Body.ToLower() == "e")
            {
                SendListOfEvents(incomingMessage);
            }
            else if (messageParts[0].ToLower() == "r" || messageParts[0].ToLower() == "remind")
            {
                SendReminder(incomingMessage, messageParts[1]);
            }
            else if (messageParts[0] == "create" || messageParts[0] == "new")
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


        public void AddRSVPToDataBase(SmsRequest incomingMessage)
        {
            bool notANumber = false;
            try
            {
                string[] textParts = incomingMessage.Body.Split(" ");

                bool userRsvp = false;

                var foundGroupMember = _context.GroupMembers.Where(x => x.PhoneNumber == incomingMessage.From).First();

                int memberId = foundGroupMember.MemberId;

                var foundEvent = _context.EventTable.Where(y => y.EventId == Int32.Parse(textParts[1])).First();
                if (foundEvent == null)
                {
                    notANumber = true;
                    throw new Exception();
                }
                int eventId = foundEvent.EventId;

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
            catch (Exception)
            {
                if (notANumber)
                {
                    SendText("That event number you entered wasn't valid. Could you try that again, please?", incomingMessage.From);
                }
                else
                {
                    SendErrorText(incomingMessage, null);
                }
            }


        }


        public void AddEventToDatabase(SmsRequest incomingMessage)
        {
            bool badDate = false;
            bool unknownGroup = false;
            bool badFormat = false;
            bool unknownUser = false;

            try
            {
                UserInfo userInfo = _context.UserInfo.Where(x => x.PhoneNumber == incomingMessage.From).First();
                if (userInfo == null)
                {
                    unknownUser = true;
                    throw new Exception();
                }

                AspNetUsers user = _context.AspNetUsers.Where(x => x.Id == userInfo.UserId).First();




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

                    badDate = true;
                    throw new Exception();

                }
                else if (!groupNames.Contains(groupName))
                {

                    unknownGroup = true;
                    throw new Exception();

                }
                else if (textParts.Count != 5)
                {
                    badFormat = true;
                    throw new Exception();
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
                        SendText($"Hi, {g.MemberName}! {userInfo.FirstName} just invited you to {userEvent} on {eventDateTime.ToString()}  at {eventVenue}, {eventLoc}. Respond with 'yes {tempEvent.EventId}' if you accept, and 'no {tempEvent.EventId}' if you decline.", g.PhoneNumber);
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
                else if (unknownUser)
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
            bool unknownUser = false;


            try
            {
                //check if number entered is in fact a number.
                if (!int.TryParse(userNumberString, out int userNumber))
                {
                    Exception badParse = new Exception();
                    badNumber = true;
                    throw badParse;
                }



                UserInfo userInfo = _context.UserInfo.Where(x => x.PhoneNumber == incomingMessage.From).First();
                if (userInfo == null)
                {
                    unknownUser = true;
                    throw new Exception();
                }

                AspNetUsers user = _context.AspNetUsers.Where(x => x.PhoneNumber == incomingMessage.From).First();

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
                    SendText($"Hey, {e.MemberName}! Just a reminder from {userInfo.FirstName} You have {timeLeft} until {userEvent.EventName}. Are you still coming? You can still RSVP by texting back with 'yes {userEvent.EventId}' or 'no {userEvent.EventId}'", e.PhoneNumber);
                }
            }
            catch (Exception)
            {
                if (badNumber)
                {
                    SendErrorText(incomingMessage, $"{userNumberString} isn't a number ಠ_ಠ.");
                    SendListOfEvents(incomingMessage);
                }
                else if (outOfRange)
                {
                    SendErrorText(incomingMessage, "That event doesn't exist.");
                    SendListOfEvents(incomingMessage);
                }
                else if (unknownUser)
                {
                    SendText("Looks like you don't have an account with us. \nIn order to create events or groups, please sign up at www.notifly.azurewebsites.net \n Have a nice day!", incomingMessage.From);
                }


            }

        }

        public void SendHelpText(SmsRequest incomingMessage, string helpNumberString)
        {
            bool parseWorked = true;
            bool outOfRange = false;
            try
            {
                //int helpNumber = int.Parse(helpNumberString);
                parseWorked = int.TryParse(helpNumberString, out int helpNumber);
                if (parseWorked == false)
                {
                    throw new Exception();
                }
                if (helpNumber == 0)
                {
                    SendText("Hi! Need some help? No problem! What would you like to know how to do? Text back the letter 'h' and the number of the topic you would like to learn about. \n1. Getting started. \n2. Creating Events \n3. Creating Groups \n4. Viewing Events and Sending Reminders", incomingMessage.From);
                }
                else if (helpNumber == 1)
                {
                    SendText("Ready to start Notiflying? To get started, you first need to sign up. Just go to notifly.azurewebsites.net. Once you're registered, you can do everything right from your phone's messaging app. Text back 'h' or '?' to see the help menu again.", incomingMessage.From);
                }
                else if (helpNumber == 2)
                {
                    SendText("To create an event you need to have this format. Be sure to have everything on a new line and have the same date format as listed. \nEvent Name \nEvent Date (ex. 05/29/2020 4:40 PM) \nEvent Venue (ex. Denny's) \nEvent Location (ex. 'Detroit, MI' or '48127') \nThe Group Name", incomingMessage.From);
                }
                else if (helpNumber == 3)
                {
                    SendText("To create a group, you need to follow this format (Hint: be sure to follow the exact format for phone number, including the '+'): \nCreate Group \nGroup Member 1 (ex. John Smith) \nMember 1 Phone Number (ex. +1734###6565) \nGroup Member 2 \nMember 2 Phone Number (ex. +1313###2495) \n \nText back 'h' or '?' to see the help menu again", incomingMessage.From);
                }
                else if (helpNumber == 4)
                {
                    SendText("To see your events, just text back either 'e' or 'events.' After you see your events, you can send reminders for each event by responding with 'r' and the number of the event. \nText back 'h' or '?' to see the help menu again", incomingMessage.From);
                }
                else if (helpNumber > 4 || helpNumber < 1)
                {
                    outOfRange = true;
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                if (parseWorked == false)
                {
                    SendErrorText(incomingMessage, $"{helpNumberString} isn't a number. ಠ_ಠ");
                }
                else if (outOfRange)
                {
                    SendErrorText(incomingMessage, "That number you chose is out of range.");
                }

            }




        }



        public void CreateGroup(SmsRequest incomingMessage)
        {
            bool badNumber = false;

            try
            {

                var user = _context.AspNetUsers.Where(x => x.PhoneNumber == incomingMessage.From).First();
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

                List<string> memberNames = new List<string>();
                List<string> memberNumbers = new List<string>();
                for (int i = 2; i < textParts.Count; i++)
                {
                    if (i % 2 == 0)
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
            UserInfo userInfo = _context.UserInfo.Where(x => x.UserId == user.Id).First();
            List<EventTable> userEvents = _context.EventTable.Where(x => x.UserId == user.Id).ToList();
            string reminderBody = $"These are your events, {userInfo.FirstName}. If you want to send out reminders, reply to this text with the letter r and the number of the event (ex. 'r 12'). \n";



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