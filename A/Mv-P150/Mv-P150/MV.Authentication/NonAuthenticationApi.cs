using System;
using System.Threading.Tasks;
using Mv.Core.Interfaces;
using LiteDB;
using System.IO;
using Mv.Core;
using static Mv.Authentication.AuthenticationExtentions;

namespace Mv.Authentication
{
    

    public static class AuthenticationExtentions
    {
  
        public static string DbPath = Path.Combine(MvFolders.MainProgram, "authentication.db");

        public static TResult Using<TDisposable, TResult>(
            Func<TDisposable> factory,
            Func<TDisposable, TResult> map)
            where TDisposable : IDisposable
        {
            using var disposable = factory();
            return map(disposable);
        }

        public static TDisposable ToDisposable<TSource, TDisposable>(this TSource source,
            Func<TSource, TDisposable> factory) where TDisposable : IDisposable
        {
            return factory(source);
        }

        public static TResult Using<TDisposable, TResult>(this TDisposable disposable, Func<TDisposable, TResult> func)
            where TDisposable : IDisposable
        {
            using (disposable)
            {
                return func(disposable);
            }
        }

        public static TResult Db<TResult>(Func<LiteDatabase, TResult> func)
        {
            return DbPath.ToDisposable((path) => new LiteDatabase(path))
                .Using(
                    func);
        }
    }
    public class MvUserImpl : IMvUser
    {
        public string Username { get; set; }
        public MvRole Role { get; set; }
        public Task<bool> SignOutAsync()
        {
            // do something  
            Exit();
            return Task.FromResult<bool>(true);
        }
        public virtual void Exit()
        {
        }

        public virtual Task<bool> RefreshAsync()
        {
            return Task.FromResult(true);
        }

        public Task<(int,string)> ChangePassword(string oldpassword,string password)
        {
            return Task.Run(() => Db<(int, string)>((db) =>
              {
                  var col = db.GetCollection<MvUser>(nameof(MvUser));
                  col.EnsureIndex(x => x.Name);
                  var user = col.FindOne(x => x.Name == Username);
                  if (user.Password != oldpassword)
                      return (-2, "password is incorrect!");
                  user.Password = password;
                 if(col.Update(user))
                  {
                      return (0, "success!");
                  }
                  else
                  {
                      return (-1, "Change fail!");
                  }
              }));
        }
    }

    public class NonAuthenticationApi : INonAuthenticationApi
    {

        public NonAuthenticationApi()
        {
            //add root user
            Db<(int,string)>(db =>
            {
                var col = db.GetCollection<MvUser>(nameof(MvUser));
                col.EnsureIndex(x => x.Name);
               var root= col.FindOne(x => x.Name == "root");
               if (root == null)
               {
                   col.Insert(new MvUser
                   {
                       Name = "root",
                       Password = "tanac123",
                       Role = MvRole.Root
                   });
               }
               return (0,"user exits...");
            });
        }
        Task<(int,string,MvRole)> INonAuthenticationApi.LoginAsync(LoginArgs args)
                 {
            return Task.Run(()=>Db<(int,string,MvRole)>((db) =>
            {
                var col = db.GetCollection<MvUser>(nameof(MvUser));
                col.EnsureIndex(x => x.Name);
                var user = col.FindOne(x => x.Name == args.UserName);
                switch (user)
                {
                    case null:
                        return (0,"user not exists...",MvRole.User);
                    default:
                        return (user.Password == args.Password)?(1,"login success.",user.Role):(-1,"password is incorrect.",MvRole.User);
                }
            }));
        }

        Task<(int,string)> INonAuthenticationApi.SignUpAsync(SignUpArgs args)
        {
            return Task.Run(()=>Db((db) =>
            {
                var col = db.GetCollection<MvUser>(nameof(MvUser));
                col.EnsureIndex(x => x.Name);
                var mvUser = new MvUser
                {
                    Name = args.Username,
                    Password = args.Password,
                    Role = MvRole.User
                };
                if (!string.IsNullOrEmpty(args.VerifyCode))
                {
                    var root= col.FindOne(x => x.Name == "root");
                    if (root == null) return (-1,"root user not exit");
                    if (args.VerifyCode == root.Password)
                    {
                        mvUser.Role = MvRole.Admin;
                    }
                    else
                    {
                        return (-2,"password is incorrect.");
                    }
                }

                 var user = col.FindOne(x => x.Name==args.Username); 

                if (user == null)
                {
                    col.Insert(mvUser);
                    return (1,"sign up success.");
                }
                else
                {
                    return (-1,"user has exits.");
                }
            }));
        }
    }
}