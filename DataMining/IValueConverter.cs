namespace DataMining
{
    public interface IValueConverter<out T>
    {
        T Convert(object toConvert);
    }
}