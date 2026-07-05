using MediatR;

namespace InvestmentApp.Application.Abstractions;

public interface IMediatRCommandRequest<out TResponse>
    : IRequest<TResponse>;
