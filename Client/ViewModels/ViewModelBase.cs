using System.ComponentModel;

namespace Client.ViewModels
{
    // base viewmodel for supporting property notification
    // so that when you change the property value on the back end, 
    // auto magically update the UI as well
    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var prop = PropertyChanged;
                prop.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
