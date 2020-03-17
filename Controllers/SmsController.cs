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

       
        public SmsController(IConfiguration configuration, NotiflyDbContext context)
        {
            TwilioAccountSid = configuration.GetSection("APIKeys")["TwilioAccountSid"];
            TwilioAuthToken = configuration.GetSection("APIKeys")["TwilioAuthToken"];
            _context = context;
        }


        [HttpPost]
        public TwiMLResult ReceiveText(SmsRequest incomingMessage)
        {
            string loggedInUser = "clay";


            var messagingResponse = new MessagingResponse();

            StringReader reader = new StringReader(incomingMessage.Body);
            string line = reader.ReadLine();
            List<string> textparts = new List<string>();
            while (line != null)
            {
                textparts.Add(line);
                line = reader.ReadLine();
            }
            string userEvent = textparts[0];
            string eventDateTime = textparts[1];
            string eventVenue = textparts[2];
            string eventLoc = textparts[3];
            string groupName = textparts[4];

            //List<Groups> foundGroups = _context.Groups.Where(x => x.GroupName == groupName).ToList();


            EventTable newEvent = new EventTable(userEvent, 1, eventDateTime, eventVenue, eventLoc, loggedInUser);

            _context.EventTable.Add(newEvent);
            _context.SaveChanges();

            return TwiML(messagingResponse);

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
                messageOptions.Body = "Rawr xD";

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