namespace GenshinPatchTools.Config.Converters
{
    public interface IConfigConverter<T>
    {
        T Convert(object obj);
    }
}