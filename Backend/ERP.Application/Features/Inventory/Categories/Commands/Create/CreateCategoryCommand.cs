using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Categories.Commands.Create;

public record CreateCategoryCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(
            Guid.NewGuid(),
            request.NameAr,
            request.NameEn
        );

        _unitOfWork.Repository<Category>().Add(category);
        await _unitOfWork.Complete();

        return category.Id;
    }
}
