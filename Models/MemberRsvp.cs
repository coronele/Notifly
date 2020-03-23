using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotiflyV0._1.Models
{
    public partial class MemberRsvp
    {
        public MemberRsvp(int memberId, int eventId, bool rsvp, string memberName)
        {
            MemberId = memberId;
            EventId = eventId;
            Rsvp = rsvp;
            MemberName = memberName;
        }


        public int MemberId { get; set; }

        [StringLength(30, MinimumLength = 3, ErrorMessage = "Must be between 3-40 characters")]
        public string MemberName { get; set; }
        public int EventId { get; set; }
        public bool Rsvp { get; set; }
        public int Rsvpid { get; set; }
        public virtual EventTable Event { get; set; }
        public virtual GroupMembers Member { get; set; }

    }
}


