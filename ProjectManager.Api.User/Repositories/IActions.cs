namespace ProjectManager.Api.User.Repositories
{
    public interface IActions<T> where T : class
    {
        // Metodos CRUD
        Task<T> Create(T entity);
        Task<T?> ReadById(int id);
        Task<T> Update(T entity);
        Task<T> Delete(int id);
        
        // Metodos adicionales
        Task<IEnumerable<T>> ReadAll(); 
        Task<T?> ReadByEmailAddress(string emailAddress);
        Task<T> UpdatePasswordByEmail(string emailAddress, string newPassword);
    }
}


