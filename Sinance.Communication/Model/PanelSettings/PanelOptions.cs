namespace Sinance.Communication.Model.PanelSettings
{
    public class PanelOptions<T>
    {
        public string Type { get; set; }

        public int Version { get; set; }

        public T Settings { get; set; }
    }
}
