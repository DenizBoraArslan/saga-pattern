using Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Trigger.Api.Models;

namespace Trigger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public LoansController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> RequestLoan([FromBody] LoanRequest request)
    {
        var loanId = Guid.NewGuid();

        await _publishEndpoint.Publish(new LoanRequestEvent(loanId, request.MemberId, request.BookId, DateTime.UtcNow));

        return Accepted(new { LoanId = loanId, Message = "Loan request submitted" });
    }
}