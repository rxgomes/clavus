using MediatR;
using Patners.Application.DTOs;
using Shared.Kernel;

namespace Patners.Application.Commands.CreatePartner;

public record CreatePartnerCommand(string DocNumber, string Name) : IRequest<Result<PartnerDto>>;