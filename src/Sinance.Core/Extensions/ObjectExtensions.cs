namespace Sinance.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetGenericTypeName(this object @object)
        {
            return @object.GetType().GetGenericTypeName();
        }
    }
}
