using System;
using System.Collections.Generic;

namespace NotiflyV0._1.Models
{
    public partial class EventTable
    {
        public EventTable()
        {
            MemberRsvp = new HashSet<MemberRsvp>();
        }

        public EventTable(string eventName, string description, int groupId, DateTime dateAndTime, string venue, string venueLocation, string userId, string groupName, DateTime notificationDate)
        {
            
            EventName = eventName;
            EventDescription = description;
            GroupId = groupId;
            DateAndTime = dateAndTime;
            Venue = venue;
            VenueLocation = venueLocation;
            UserId = userId;
            GroupName = groupName;
            NotificationDate = notificationDate;
            
        }

        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public int GroupId { get; set; }
        public DateTime DateAndTime { get; set; }
        public string Venue { get; set; }
        public string VenueLocation { get; set; }
        public string UserId { get; set; }
        public string GroupName { get; set; }
        public DateTime? NotificationDate { get; set; }

        public virtual Groups Group { get; set; }
        public virtual AspNetUsers User { get; set; }
        public virtual ICollection<MemberRsvp> MemberRsvp { get; set; }
    }
}
