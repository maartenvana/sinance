namespace Sinance.Domain.Model;

public class Category : UserEntity
{
    public string ColorCode { get; set; }

    public bool IsRegular { get; set; }

    public bool IsStandard { get; set; }

    public string Name { get; set; }

    public Category ParentCategory { get; set; }

    public int? ParentCategoryId { get; set; }

    public string ShortName { get; set; }
}
