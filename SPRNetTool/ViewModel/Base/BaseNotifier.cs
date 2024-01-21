using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArtWiz.ViewModel.Base
{
    public class BaseNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void OnPropertyChanged(object sender, string propertyName)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        protected void Invalidate([CallerMemberName] string caller = "")
        {
            OnPropertyChanged(caller);
        }

        protected void InvalidateAll()
        {
            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                BindableAttribute? attribute = Attribute.GetCustomAttribute(property, typeof(BindableAttribute)) as BindableAttribute;

                if (attribute != null && attribute.Bindable)
                {
                    OnPropertyChanged(property.Name);
                }
            }
        }
    }
}
