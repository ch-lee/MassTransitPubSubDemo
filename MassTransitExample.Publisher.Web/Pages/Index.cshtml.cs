using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransitExample.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MassTransitExample.Publisher.Web.Pages
{

    public class IndexModel : PageModel
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly Random _random;

        public IndexModel(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
        {
            _publishEndpoint = publishEndpoint;
            _sendEndpointProvider = sendEndpointProvider;

            _random = new Random();
        }

        public void OnGet()
        {
            
        }


        public async Task<IActionResult> OnPostPurchaseFlightAsync()
        {
            await _publishEndpoint.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = _random.Next(1, 999) });

            return Page();
        }

        public async Task<IActionResult> OnPostEnqueueMessageAsync()
        {
            var sendEndpoint =
                await _sendEndpointProvider.GetSendEndpoint(
                    new Uri("sb://dev-demo-test-01.servicebus.windows.net/flight-cancellation"));


            await sendEndpoint.Send<FlightCancellation>(new FlightCancellation { FlightId = Guid.NewGuid(), CancellationId = _random.Next(1, 999)});

            return Page();
        }

    }
}
