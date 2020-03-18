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
            string senderNumber = incomingMessage.From;

            // For validating response
            StringReader reader = new StringReader(incomingMessage.Body);
            string line = reader.ReadLine();
            List<string> textparts = new List<string>();
            
            while (line != null)
            {
                textparts.Add(line);
                line = reader.ReadLine();
            }

            string evalText = textparts.First().ToLower();

            if (evalText.Contains("yes") || evalText.Contains("no") || (evalText == "n") || (evalText == "y"))
            {
                AddRSVPToDataBase(incomingMessage);
            }
            else
            {
                //string id = "08bd85be-3531-4ddb-8814-4d554a016319";
                var messagingResponse = new MessagingResponse();

                //string dummyMessage = "Lunch with Clay \n 3/29/21 \n Big Boy's \n Taylor, MI \n The Boys";

                AddEventToDatabase(incomingMessage.Body);
                return TwiML(messagingResponse);
            }
        }

        public void AddRSVPToDataBase(SmsRequest incomingMessage) 
        {
            // Placeholder id
            string id = "08bd85be-3531-4ddb-8814-4d554a016319";

            StringReader reader = new StringReader(incomingMessage.Body);
            string line = reader.ReadLine();
            List<string> textparts = new List<string>();
            bool userRsvp;

            while (line != null)
            {
                textparts.Add(line);
                line = reader.ReadLine();
            }

            var founduser = _context.AspNetUsers.Where(x => x.PhoneNumber == incomingMessage.From).ToList();

            string recUserId = founduser.First().Id;

            var foundevent = _context.EventTable.Where(y => y.EventId == Int32.Parse(textparts[0]));

            int recEventId = foundevent.First().EventId;

            string rsvpResponse = textparts[1].ToLower();
            if ((rsvpResponse.Contains("yes") || (rsvpResponse == "y")))
            {
                userRsvp = true;
            }
            else // No validation yet for invalid response
            {
                userRsvp = false;
            }

            MemberRsvp newRsvp = new MemberRsvp(recUserId, recEventId, userRsvp);

            _context.MemberRsvp.Add(newRsvp);
            _context.SaveChanges();

        }


        public void AddEventToDatabase(string messageBody)
        {
            string id = "08bd85be-3531-4ddb-8814-4d554a016319";
            StringReader reader = new StringReader(messageBody);
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

            EventTable newEvent = new EventTable(userEvent, "Description", 2, eventDateTime, eventVenue, eventLoc, id);

            _context.EventTable.Add(newEvent);

            

            
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
            string[] groupNumbers = { "+12487196559", "+17348876670" };
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

        [HttpPost]
        public ActionResult SaveText([FromBody] string Body)
        {

            var response = new MessagingResponse();
            response.Message("sdf");
            return new ContentResult { Content = response.ToString(), ContentType = "application/xml" };
        }
    }
}


