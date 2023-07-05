using Sinance.BlazorApp.Business.Model.Category;

namespace Sinance.BlazorApp.Model.Events;

public class CategoryUpsertedEvent
{
    public bool Created { get; set; }

    public CategoryModel Model { get; set; }
}
