using Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Trigger.Api.Models;

namespace Trigger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILogger<LoansController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    public LoansController(ILogger<LoansController> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoan([FromBody] LoanRequest request)
    {
        var loanId = Guid.NewGuid();    
        _logger.LogInformation("Received loan request for {Amount} from {ApplicantName}", request.MemberId, request.BookId);
      
        await _publishEndpoint.Publish(new LoanRequestedEvent(loanId, request.MemberId, request.BookId,DateTime.UtcNow));
        return Accepted(new { Message = "Loan request submitted successfully." });
    }

}

