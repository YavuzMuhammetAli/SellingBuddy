using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Features.Queries.GetOrderDetailById;
using OrderService.Application.Features.Queries.GetOrdersByUser;
using OrderService.Application.Features.Queries.ViewModels;
using System.Net;
using System.Security.Claims;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //orders of the current user
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderSummaryViewModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var orders = await _mediator.Send(new GetOrdersByUserQuery(userName));

            return Ok(orders);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderDetailViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetOrderDetailsById(Guid id)
        {
            var result = await _mediator.Send(new GetOrderDetailsQuery(id));

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
