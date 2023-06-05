namespace Sinance.Web.Model;

/// <summary>
/// Basic checkbox item to use for display and returning data
/// </summary>
public class BasicCheckBoxItem
{
    /// <summary>
    /// State of the checkbox
    /// </summary>
    public bool Checked { get; set; }

    /// <summary>
    /// Identifier of the entity/object represented
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name to display
    /// </summary>
    public string Name { get; set; }
}