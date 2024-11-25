using backend.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Utilities
{
    public class UserSingleton
    {
        private static UserSingleton _instance;

        private readonly User _user;

        private UserSingleton()
        {
            _user = new User(new Guid("7097194a-84a3-4010-9bf8-028f4869c54f"), "BNW", "12amGANG");
        }

        public static UserSingleton GetInstance()
        {
            if (_instance == null)
            {
                _instance = new UserSingleton();
            }
            return _instance;
        }

        public User GetUser()
        {
            return _user;
        }
    }

}
