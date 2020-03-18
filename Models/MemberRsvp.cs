using System;
using System.Collections.Generic;

namespace NotiflyV0._1.Models
{
    public partial class MemberRsvp
    {
        public MemberRsvp(int memberId, int eventId, bool rsvp)
        {
            MemberId = memberId;
            EventId = eventId;
            Rsvp = rsvp;
        }


        public int MemberId { get; set; }
        public int EventId { get; set; }
        public bool Rsvp { get; set; }
        public int Rsvpid { get; set; }

        public virtual EventTable Event { get; set; }
        public virtual GroupMembers Member { get; set; }
    }
}


