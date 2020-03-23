using System;
using System.Collections.Generic;

namespace NotiflyV0._1.Models
{
    public partial class UserInfo
    {
        public int UserInfoId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }


        public virtual AspNetUsers User { get; set; }

        public UserInfo()
        {

        }

        public UserInfo(string firstName, string lastName, string userId, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            UserId = userId;
            PhoneNumber = phoneNumber;
        }




    }
}
