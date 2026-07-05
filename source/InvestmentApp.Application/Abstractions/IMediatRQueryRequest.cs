using MediatR;

namespace InvestmentApp.Application.Abstractions;

public interface IMediatRQueryRequest<out TResponse> : IRequest<TResponse>;
