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

            string[] messageParts = incomingMessage.Body.Split(" ");
            if (messageParts[0].ToLower() == "yes" || messageParts[0].ToLower() == "no" || messageParts[0].ToLower() == "n" || messageParts[0].ToLower() == "y")
            {

                AddRSVPToDataBase(incomingMessage);

            }
            else
            {
                AddEventToDatabase(incomingMessage);

            }


            var messagingResponse = new MessagingResponse();



            return TwiML(messagingResponse);

        }


        public void AddRSVPToDataBase(SmsRequest incomingMessage)
        {

            string[] textParts = incomingMessage.Body.Split(" ");
            bool userRsvp;

            var founduser = _context.GroupMembers.Where(x => x.PhoneNumber == incomingMessage.From).ToList();

            int memberId = founduser[0].MemberId;

            var foundevent = _context.EventTable.Where(y => y.EventId == Int32.Parse(textParts[1])).ToList();

            int eventId = foundevent[0].EventId;

            string rsvpResponse = textParts[0].ToLower();
            if (rsvpResponse.Contains("yes") || rsvpResponse == "y")
            {
                userRsvp = true;
            }
            else if (rsvpResponse.Contains("no") || rsvpResponse == "n")
            {
                userRsvp = false;
            }
            userRsvp = false;

            MemberRsvp newRsvp = new MemberRsvp(memberId, eventId, userRsvp);

            _context.MemberRsvp.Add(newRsvp);
            _context.SaveChanges();

        }


        public void AddEventToDatabase(SmsRequest message)
        {
            string userPhoneNumber = message.From;
            List<AspNetUsers> user = _context.AspNetUsers.Where(x => x.PhoneNumber == userPhoneNumber).ToList();

            StringReader reader = new StringReader(message.Body);
            string line = reader.ReadLine();
            List<string> textparts = new List<string>();

            while (line != null)
            {
                textparts.Add(line);
                line = reader.ReadLine();
            }

            string userEvent = textparts[0];

            DateTime eventDateTime = DateTime.Parse(textparts[1]);
            string eventVenue = textparts[2];
            string eventLoc = textparts[3];
            string groupName = textparts[4];

            //List<Groups> foundGroups = _context.Groups.Where(x => x.GroupName == groupName).ToList();

            EventTable newEvent = new EventTable(userEvent, "Description", 2, eventDateTime, eventVenue, eventLoc, user[0].Id);

            _context.EventTable.Add(newEvent);
            _context.SaveChanges();

        }




        public ActionResult SendText(string body, string number)
        {
            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);

            var messageOptions = new CreateMessageOptions(
            new PhoneNumber("+17348876670"));

            messageOptions.From = new PhoneNumber("+19854413010");
            messageOptions.Body = body;

            var message = MessageResource.Create(messageOptions);
            return new OkResult();
        }

        public ActionResult SendGroupText()
        {
            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);
            string[] groupNumbers = { "+12487196559", "+12488541947" };
            foreach (var n in groupNumbers)
            {
                var messageOptions = new CreateMessageOptions(
                new PhoneNumber(n));

                messageOptions.From = new PhoneNumber("+19854413010");
                messageOptions.Body = "Proof for Erwin";

                var message = MessageResource.Create(messageOptions);
            }
            return new OkResult();
        }




        public ActionResult SendWelcomeText()
        {

            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);

            var messageOptions = new CreateMessageOptions(
            new PhoneNumber("+17348876670"));

            messageOptions.From = new PhoneNumber("+19854413010");
            messageOptions.Body = $"Hey there, Jon. Clay set up an event at 2:00 P.M. Would you like to come? Reply with 'yes' if you would and 'no' if you don't want to. Reply with 'stop' if you don't want anymore texts.";

            var message = MessageResource.Create(messageOptions);

            return new OkResult();
        }

        
    }
}


