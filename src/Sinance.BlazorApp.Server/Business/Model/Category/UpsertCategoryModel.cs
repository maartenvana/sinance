namespace Sinance.BlazorApp.Business.Model.Category
{
    public class UpsertCategoryModel
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string ColorCode { get; set; }
        public bool IsNew => Id == 0;
    }
}
