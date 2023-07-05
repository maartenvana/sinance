using Sinance.Domain.Types;

namespace Sinance.Domain.Model;

public class CategoryMapping : UserEntity
{
    public Category Category { get; set; }

    public int CategoryId { get; set; }

    public TransactionMappingValueType ColumnTypeId { get; set; }

    public string MatchValue { get; set; }
}
