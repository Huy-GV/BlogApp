﻿using SimpleForum.Core.Communication;
using System.Threading.Tasks;

namespace SimpleForum.Core.WriteServices;
public interface IPostModerationService
{
    /// <summary>
    /// Hides a thread asynchronously.
    /// </summary>
    /// <param name="threadId">The identifier of the thread to be hidden.</param>
    /// <param name="name">The name of the user hiding the thread.</param>
    /// <returns>The result code indicating the outcome of the hide operation.</returns>
    Task<ServiceResultCode> HideThreadAsync(int threadId, string name);

    /// <summary>
    /// Hides a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be hidden.</param>
    /// <param name="name">The name of the user hiding the comment.</param>
    /// <returns>The result code indicating the outcome of the hide operation.</returns>
    Task<ServiceResultCode> HideCommentAsync(int commentId, string name);

    /// <summary>
    /// Forcibly deletes a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be forcibly deleted.</param>
    /// <param name="deletorName">The name of the user forcibly deleting the comment.</param>
    /// <returns>The result code indicating the outcome of the delete operation.</returns>
    Task<ServiceResultCode> ForciblyDeleteCommentAsync(int commentId, string deletorName);

    /// <summary>
    /// Forcibly deletes a thread asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the thread to be forcibly deleted.</param>
    /// <param name="deletorName">The name of the user forcibly deleting the thread.</param>
    /// <returns>The result code indicating the outcome of the delete operation.</returns>
    Task<ServiceResultCode> ForciblyDeleteThreadAsync(int commentId, string deletorName);

    /// <summary>
    /// Unhides a thread asynchronously.
    /// </summary>
    /// <param name="threadId">The identifier of the thread to be unhidden.</param>
    /// <param name="name">The name of the user unhiding the thread.</param>
    /// <returns>The result code indicating the outcome of the unhide operation.</returns>
    Task<ServiceResultCode> UnhideThreadAsync(int threadId, string name);

    /// <summary>
    /// Unhides a comment asynchronously.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to be unhidden.</param>
    /// <param name="name">The name of the user unhiding the comment.</param>
    /// <returns>The result code indicating the outcome of the unhide operation.</returns>
    Task<ServiceResultCode> UnhideCommentAsync(int commentId, string name);
}
