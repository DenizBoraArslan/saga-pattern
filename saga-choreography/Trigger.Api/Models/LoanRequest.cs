namespace Trigger.Api.Models;

public class LoanRequest
{
    public Guid MemberId { get; set; }
    public Guid BookId { get; set; }
}