using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotiflyV0._1.Models
{
    public class TimeManager
    {
        
        public static List<EventTable> ListDueEvents(List<EventTable> events)
        {
            List<EventTable> dueEvents = new List<EventTable>();

            foreach (var e in events)
            {
                if (e.DateAndTime > DateTime.Now)
                {
                    dueEvents.Add(e);
                }
            }

            return dueEvents;
        }





    }
}

