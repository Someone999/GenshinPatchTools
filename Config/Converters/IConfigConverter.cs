namespace GenshinPatchTools.Config.Converters
{
    
    public interface IConfigConverter
    {
        object Convert(object obj);
    }
    
    public interface IConfigConverter<out T> : IConfigConverter
    {
        new T Convert(object obj);
    }
}