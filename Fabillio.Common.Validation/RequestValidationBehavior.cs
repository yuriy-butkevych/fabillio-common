using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Newtonsoft.Json;

namespace Fabillio.Common.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var context = new ValidationContext<TRequest>(request);

        var failures = _validators.Select(async validator => await validator.ValidateAsync(context))
                                    .Where(task => task is not null)
                                    .Select(task => task.Result)
                                    .SelectMany(validation => validation.Errors)
                                    .Where(failure => failure is not null)
                                    .ToList();

        if (failures.Count != 0)
        {
            var exc = new ValidationException(failures);
            exc.Data.Add("RequestName", request);
            exc.Data.Add("RequestContent", JsonConvert.SerializeObject(request));
            throw exc;
        }

        return await next();
    }
}
