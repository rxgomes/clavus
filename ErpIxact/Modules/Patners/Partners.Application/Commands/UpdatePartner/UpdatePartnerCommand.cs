using MediatR;
using Patners.Application.DTOs;
using Shared.Kernel;
namespace Patners.Application.Commands.UpdatePartner;

public record UpdatePartnerCommand(Guid Id, string DocNum, string Name, bool Active) : IRequest<Result<PartnerDto>>;