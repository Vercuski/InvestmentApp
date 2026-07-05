using MediatR;

namespace InvestmentApp.Application.Abstractions;

public interface IMediatRCommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : IMediatRCommandRequest<TResponse>;
