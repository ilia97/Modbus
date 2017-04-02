namespace Core.DataAccess.Interfaces
{
    public interface IRepository
    {
        bool Save<T>(T Entity);

        bool Get<T>(T Entity);
    }
}
