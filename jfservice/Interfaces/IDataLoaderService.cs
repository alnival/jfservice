namespace jfservice.Interfaces
{
    public interface IDataLoaderService
    {
        List<T> LoadData<T>(string fileName);
    }
}