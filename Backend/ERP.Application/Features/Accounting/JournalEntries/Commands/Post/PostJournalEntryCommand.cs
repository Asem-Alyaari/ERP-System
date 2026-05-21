using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Post;

public record PostJournalEntryCommand(Guid JournalEntryId, string PostedBy) : IRequest<bool>;
