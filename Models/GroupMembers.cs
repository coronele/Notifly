using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotiflyV0._1.Models
{
    public partial class GroupMembers
    { 
        public GroupMembers(string memberName, int groups, string phoneNumber)
        {
            MemberName = memberName;
            Groups = groups;
            PhoneNumber = phoneNumber; 
        }


        public GroupMembers()
        {
            MemberRsvp = new HashSet<MemberRsvp>();
        }

        public int MemberId { get; set; }

        [StringLength(30, MinimumLength = 3, ErrorMessage = "Must be between 3-30 characters")]
        public string MemberName { get; set; }
        public int Groups { get; set; }


        public string PhoneNumber { get; set; }

        public virtual ICollection<MemberRsvp> MemberRsvp { get; set; }
    }
}
