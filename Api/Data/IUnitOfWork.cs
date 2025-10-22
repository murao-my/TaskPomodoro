namespace TaskPomodoro.Api.Data;

public interface IUnitOfWork
{
    /// <summary>
    /// Save changes to the database
    /// </summary>
    /// <returns>The number of changes saved</returns>
    Task<int> SaveChangesAsync();

}