using System;
using System.Collections.Generic;

namespace NotiflyV0._1.Models
{
    public partial class GroupMembers
    {
        public GroupMembers()
        {
            MemberRsvp = new HashSet<MemberRsvp>();
        }

        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string Groups { get; set; }

        public virtual ICollection<MemberRsvp> MemberRsvp { get; set; }
    }
}
