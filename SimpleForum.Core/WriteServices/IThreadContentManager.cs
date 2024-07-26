using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.ViewModels;
using System.Threading.Tasks;

namespace SimpleForum.Core.WriteServices;
public interface IThreadContentManager
{
    /// <summary>
    /// Creates a new thread asynchronously.
    /// </summary>
    /// <param name="createThreadViewModel">The view model containing information for creating a thread.</param>
    /// <param name="userName">The name of the user creating the thread.</param>
    /// <returns>
    /// A tuple containing the result code and the newly created thread's identifier.
    /// The result code indicates the outcome of the operation.
    /// </returns>
    Task<(ServiceResultCode, int?)> CreateThreadAsync(CreateThreadViewModel createThreadViewModel, string userName);

    /// <summary>
    /// Deletes a thread asynchronously.
    /// </summary>
    /// <param name="threadId">The identifier of the thread to be deleted.</param>
    /// <param name="userName">The name of the user attempting to delete the thread.</param>
    /// <returns>
    /// The result code indicating the outcome of the delete operation.
    /// </returns>
    Task<ServiceResultCode> DeleteThreadAsync(int threadId, string userName);

    /// <summary>
    /// Updates an existing thread asynchronously.
    /// </summary>
    /// <param name="editThreadViewModel">The view model containing updated information for the thread.</param>
    /// <param name="userName">The name of the user updating the thread.</param>
    /// <returns>
    /// The result code indicating the outcome of the update operation.
    /// </returns>
    Task<ServiceResultCode> UpdateThreadAsync(EditThreadViewModel editThreadViewModel, string userName);
}
