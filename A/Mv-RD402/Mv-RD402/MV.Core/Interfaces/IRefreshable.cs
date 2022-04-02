using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Threading.Tasks;

namespace Mv.Core.Interfaces
{
    public interface IMvUser : IRefreshable
    {

        string Username { get; }
        MvRole Role { get; }

        Task<bool> SignOutAsync();

        Task<(int,string)> ChangePassword(string oldPassword,string newPassword);

        void Exit();
    }
    
    public interface IRefreshable
    {
        /// <summary>
        /// Try to refresh the object.
        /// </summary>
        /// <returns>Returns a <see cref="bool"/> type indicating whether the data was successfully fetched.</returns>
        Task<bool> RefreshAsync();
    }
}
