using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Unpost;

public record UnpostJournalEntryCommand(Guid JournalEntryId, string UnpostedBy) : IRequest<bool>;
