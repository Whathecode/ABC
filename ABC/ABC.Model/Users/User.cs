using ABC.Model.Primitives;
using System.Collections.Generic;

namespace ABC.Model.Users
{
    public class User : Noo,IUser
    {
        #region Properties

        private string _tag;
        public string Tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
                OnPropertyChanged("tag");
            }
        }

        private string _image;
        public string Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("image");
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("email");
            }
        }

        private Rgb _color;
        public Rgb Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("color");
            }
        }


        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged("selected");
            }
        }

        private int _state;
        public int State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("state");
            }
        }

        private string _cid;
        public string Cid
        {
            get { return _cid; }
            set
            {
                _cid = value;
                OnPropertyChanged("cid");
            }
        }

        private List< Activity> _activities;
        public List<Activity> Activities
        {
            get { return _activities; }
            set 
            {
                _activities = value;
                OnPropertyChanged("activities");
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Name;
        }
        #endregion

        public User()
        {
            BaseType = typeof(IUser).Name;
        }

    }
}
