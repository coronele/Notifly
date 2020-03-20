using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotiflyV0._1.Models
{
    public class TextService
    {
        private readonly string TwilioAccountSid;
        private readonly string TwilioAuthToken;

        public TextService()
        {

        }
        public TextService(IConfiguration configuration)
        {
            TwilioAccountSid = configuration.GetSection("APIKeys")["TwilioAccountSid"];
            TwilioAuthToken = configuration.GetSection("APIKeys")["TwilioAuthToken"];
        }


        
    }
}


