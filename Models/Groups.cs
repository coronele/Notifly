using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [StringLength(30, MinimumLength = 3, ErrorMessage = "Must be between 3-30 characters")]
        public string GroupName { get; set; }
        public string UserId { get; set; }
        public virtual AspNetUsers User { get; set; }
        public virtual ICollection<EventTable> EventTable { get; set; }
    }
}
