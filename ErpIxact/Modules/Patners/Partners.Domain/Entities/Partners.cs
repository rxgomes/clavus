using Patners.Domain.Exceptions;
using Patners.Domain.Messages;
using Shared.Kernel.Entities;
using Shared.Kernel.ValueObjects;

namespace Patners.Domain.Entities;

public class Partners : BaseEntity
{
    public DocNumber DocNumber { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;
    
    public Partners(DocNumber docNumber, string name)
    {
        if (docNumber is null)
        {
            throw new DomainException(PartnersMessages.Errors.DocNumberRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(PartnersMessages.Errors.NameRequired);
        }

        DocNumber = docNumber;
        Name = name;
    }
  

    protected Partners() { }
}
