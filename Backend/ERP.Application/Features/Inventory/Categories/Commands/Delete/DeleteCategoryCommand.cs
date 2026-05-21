using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Categories.Commands.Delete;

public record DeleteCategoryCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);

        if (category == null)
        {
            throw new Exception($"Category with ID {request.Id} not found");
        }

        // Check if there are any items linked to this category
        var itemsCount = await _unitOfWork.Repository<Item>().CountAsync(new ERP.Application.Features.Inventory.Categories.Specifications.ItemsByCategoryIdSpecification(request.Id));
        if (itemsCount > 0)
        {
            throw new Exception("Cannot delete category with linked items");
        }

        _unitOfWork.Repository<Category>().Delete(category);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
