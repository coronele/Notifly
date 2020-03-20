using System;
using System.Collections.Generic;

namespace NotiflyV0._1.Models
{
    public partial class Groups
    {
        public Groups()
        {
            EventTable = new HashSet<EventTable>();
        }

        public Groups(string groupName, string userId)
        {
            GroupName = groupName;
            UserId = userId;
            
        }


        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string UserId { get; set; }
        public virtual AspNetUsers User { get; set; }
        public virtual ICollection<EventTable> EventTable { get; set; }
    }
}
