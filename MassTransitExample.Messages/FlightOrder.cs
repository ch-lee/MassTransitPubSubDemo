using System;

namespace MassTransitExample.Messages
{
    public class FlightOrder
    {
        public Guid FlightId { get; set;  }
        public int OrderId { get; set;  }
    }

}
