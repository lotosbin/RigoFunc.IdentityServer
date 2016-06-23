using System.Threading.Tasks;
using RigoFunc.OAuth;

namespace RigoFunc.IdentityServer.Api {
    /// <summary>
    /// Provides the interface(s) for the account service.
    /// </summary>
    public interface IAccountService {
        /// <summary>
        /// Registers a new user asynchronous.
        /// </summary>
        /// <param name="model">The register model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the register operation. Task result contains the register response.</returns>
        Task<IResponse> RegisterAsync(RegisterInputModel model);
        /// <summary>
        /// Sends the specified code asynchronous.
        /// </summary>
        /// <param name="model">The send code model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the send operation.</returns>
        Task<bool> SendCodeAsync(SendCodeInputModel model);
        /// <summary>
        /// Logins with the specified model asynchronous.
        /// </summary>
        /// <param name="model">The login model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the login operation.</returns>
        Task<IResponse> LoginAsync(LoginInputModel model);
        /// <summary>
        /// Verifies the specified code asynchronous.
        /// </summary>
        /// <param name="model">The veriry code model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the verify operation.</returns>
        Task<IResponse> VerifyCodeAsync(VerifyCodeInputModel model);
        /// <summary>
        /// Changes the password for the specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the change operation.</returns>
        Task<bool> ChangePasswordAsync(ChangePasswordModel model);
        /// <summary>
        /// Resets the password for specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the reset operation.</returns>
        Task<IResponse> ResetPasswordAsync(ResetPasswordModel model);
        /// <summary>
        /// Updates the specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the reset operation.</returns>
        Task<bool> UpdateAsync(OAuthUser model);
    }
}
