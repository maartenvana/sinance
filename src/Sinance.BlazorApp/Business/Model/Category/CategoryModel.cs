namespace Sinance.BlazorApp.Business.Model.Category
{
    public class CategoryModel
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string ColorCode { get; set; }

        public static CategoryModel All => new() { Id = -1, Name = "All" };
    }
}
