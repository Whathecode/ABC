using ABC.Model.Primitives;

namespace ABC.Model
{
    public class Metadata:Base
    {
        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged("header");
            }
        }
        private string _data;
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("data");
            }
        }
    }
}
