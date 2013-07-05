using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ABC.Model.Users;
using Newtonsoft.Json.Linq;
using ABC.Infrastructure.ActivityBase;
using ABC.Infrastructure.Helpers;

namespace ABC.Infrastructure.Web.Controllers
{
    public class UsersController : ApiController
    {
        private readonly ActivitySystem _system;

        public UsersController(ActivitySystem system)
        {
            _system = system;
        }
        public List<IUser> Get()
        {
            return _system.Users.Values.ToList();
        }
         public IUser Get(string id)
        {
            return _system.Users[id];
        }
        public void Post(JObject user)
        {
            _system.AddUser(Json.ConvertFromTypedJson<IUser>(user.ToString()));
        }
        public void Delete(string id)
        {
            _system.RemoveUser(id);
        }
        public void Put(JObject user)
        {
            _system.UpdateUser(Json.ConvertFromTypedJson<IUser>(user.ToString()));
        }
    }
}
