using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ABC.Infrastructure.ActivityBase;
using ABC.Infrastructure.Helpers;
using ABC.Model;
using Newtonsoft.Json.Linq;

namespace ABC.Infrastructure.Web.Controllers
{
    public class ActivitiesController : ApiController
    {
        private readonly ActivitySystem _system;


        public ActivitiesController(ActivitySystem system)
        {
            _system = system;
        }

        public List<IActivity> Get()
        {
            return _system.Activities.Values.ToList();
        }
        public IActivity Get(string id)
        {
            return _system.Activities[id];
        }
        public void Post(JObject activity)
        {
            _system.AddActivity(Json.ConvertFromTypedJson<IActivity>(activity.ToString()));
        }
        public void Delete(string id)
        {
            _system.RemoveActivity(id);
        }
        public void Put(JObject activity)
        {
            _system.UpdateActivity(Json.ConvertFromTypedJson<IActivity>(activity.ToString()));
        }
    }
}
