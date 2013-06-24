using System.Collections.Generic;
using ABC.Model.Primitives;

namespace ABC.Model
{
    public class Action : Noo
    {
        public Action()
        {
            InitializeProperties();
        }

        #region Initializers
        private void InitializeProperties()
        {
            Resources = new List<Resource>();
        }
        #endregion

        #region Properties
        private List<Resource> _resources;
        public List<Resource> Resources
        {
            get { return _resources; }
            set
            {
                _resources = value;
                OnPropertyChanged("Resources");
            }
        }
        #endregion
    }
}
