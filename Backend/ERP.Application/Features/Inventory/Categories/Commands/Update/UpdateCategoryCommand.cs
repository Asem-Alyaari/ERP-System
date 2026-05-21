using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Categories.Commands.Update;

public record UpdateCategoryCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);

        if (category == null)
        {
            throw new Exception($"Category with ID {request.Id} not found");
        }

        category.Update(request.NameAr, request.NameEn);

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
