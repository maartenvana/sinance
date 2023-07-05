using Sinance.Domain.Types;

namespace Sinance.Domain.Model;

public class Account : UserEntity
{
    public AccountType AccountType { get; set; }

    public decimal? CurrentBalance { get; set; }

    public bool Disabled { get; set; }

    public string Name { get; set; }

    public decimal StartBalance { get; set; }
}
