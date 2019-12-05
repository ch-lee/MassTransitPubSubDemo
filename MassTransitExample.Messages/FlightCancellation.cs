using System;

namespace MassTransitExample.Messages
{
    public class FlightCancellation
    {
        public Guid FlightId { get; set; }

        public int CancellationId { get; set; }
    }
}