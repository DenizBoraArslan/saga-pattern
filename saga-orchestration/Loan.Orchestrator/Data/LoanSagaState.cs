using MassTransit;

namespace Loan.Orchestrator.Data;
public class LoanSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid MemberId { get; set; }
    public Guid BookId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FailureReason { get; set; }
    public int Version { get; set; }

}

