using SPRNetTool.Domain.Base;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SPRNetTool.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDomainObserver, IDomainAccessors
    {
        protected IBitmapDisplayManager BitmapDisplayManager
        {
            get
            {
                return IDomainAccessors.DomainContext.GetDomain<IBitmapDisplayManager>();
            }
        }

        protected ISprWorkManager SprWorkManager
        { get { return IDomainAccessors.DomainContext.GetDomain<ISprWorkManager>(); } }

        protected void Invalidate([CallerMemberName] string caller = "")
        {
            OnPropertyChanged(caller);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        void IDomainObserver.OnDomainChanged(IDomainChangedArgs args)
        {
            OnDomainChanged(args);
        }

        protected virtual void OnDomainChanged(IDomainChangedArgs args) { }

    }
}
