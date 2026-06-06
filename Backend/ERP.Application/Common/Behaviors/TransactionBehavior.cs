using MediatR;
using Microsoft.Extensions.Logging;
using ERP.Domain.Repositories;

namespace ERP.Application.Common.Behaviors;

/// <summary>
/// سلوك MediatR لفرض Transaction Scope على جميع عمليات الكتابة
/// يضمن أن جميع العمليات تتم في معاملة واحدة مع Rollback تلقائي عند الفشل
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // التحقق من أن الطلب هو Command (يغير البيانات)
        var isCommand = typeof(TRequest).Name.EndsWith("Command") ||
                        typeof(TRequest).Name.EndsWith("Create") ||
                        typeof(TRequest).Name.EndsWith("Update") ||
                        typeof(TRequest).Name.EndsWith("Delete") ||
                        typeof(TRequest).Name.Contains("Post");

        if (!isCommand)
        {
            // للـ Queries، لا نحتاج transaction
            return await next();
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogDebug(
                "Starting transaction for {RequestType}",
                typeof(TRequest).Name);

            var response = await next();

            await _unitOfWork.Complete();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Transaction committed for {RequestType}",
                typeof(TRequest).Name);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Transaction rolling back for {RequestType} due to error",
                typeof(TRequest).Name);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }
}
