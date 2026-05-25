using MediatR;

namespace ERP.Application.Features.Expenses.Bills.Commands.Post;

public record PostExpenseBillCommand(Guid BillId, string UserId) : IRequest<bool>;
