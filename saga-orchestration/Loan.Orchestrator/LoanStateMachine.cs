using Contracts.Commands;
using Contracts.Events;
using Loan.Orchestrator.Data;
using MassTransit;

namespace Loan.Orchestrator;
public class LoanStateMachine : MassTransitStateMachine<LoanSagaState>
{
    public State AwaitingMembershipVerification { get; private set; } = null!;
    public State AwaitingStockReservation { get; private set; } = null!;
    public State AwaitingLoanCreation { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<LoanRequestedEvent> LoanRequested { get; private set; } = null!;
    public Event<MembershipVerifiedEvent> MembershipVerified { get; private set; } = null!;
    public Event<MembershipVerificationFailedEvent> MembershipVerificationFailed { get; private set; } = null!;
    public Event<StockReservedEvent> StockReserved { get; private set; } = null!;
    public Event<StockReservationFailedEvent> StockReservationFailed { get; private set; } = null!;
    public Event<LoanCreatedEvent> LoanCreated { get; private set; } = null!;

    public LoanStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => LoanRequested, x => x.CorrelateById(context => context.Message.LoanId));
        Event(() => MembershipVerified, x => x.CorrelateById(context => context.Message.LoanId));
        Event(() => MembershipVerificationFailed, x => x.CorrelateById(context => context.Message.LoanId));
        Event(() => StockReserved, x => x.CorrelateById(ctx => ctx.Message.LoanId));
        Event(() => StockReservationFailed, x => x.CorrelateById(ctx => ctx.Message.LoanId));
        Event(() => LoanCreated, x => x.CorrelateById(ctx => ctx.Message.LoanId));

        Initially(
        When(LoanRequested)
            .Then(ctx =>
            {
                ctx.Saga.MemberId = ctx.Message.MemberId;
                ctx.Saga.BookId = ctx.Message.BookId;
                ctx.Saga.CreatedAt = DateTime.UtcNow;
            })
            .Publish(ctx => new VerifyMembershipCommand(ctx.Saga.CorrelationId, ctx.Saga.MemberId, ctx.Saga.BookId))
            .TransitionTo(AwaitingMembershipVerification)
    );
        During(AwaitingMembershipVerification,
        When(MembershipVerified)
            .Publish(ctx => new ReserveStockCommand(ctx.Saga.CorrelationId, ctx.Saga.MemberId, ctx.Saga.BookId))
            .TransitionTo(AwaitingStockReservation),
        When(MembershipVerificationFailed)
            .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
            .TransitionTo(Failed)
            .Finalize()
    );
        During(AwaitingStockReservation,
         When(StockReserved)
             .Publish(ctx => new CreateLoanCommand(ctx.Saga.CorrelationId, ctx.Saga.MemberId, ctx.Saga.BookId))
             .TransitionTo(AwaitingLoanCreation),
         When(StockReservationFailed)
             .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
             .TransitionTo(Failed)
             .Finalize()
     );

        During(AwaitingLoanCreation,
          When(LoanCreated)
              .TransitionTo(Completed)
              .Finalize()
      );

        SetCompletedWhenFinalized();

    }

}
