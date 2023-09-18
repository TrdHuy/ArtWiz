using SPRNetTool.Domain.Base;
using SPRNetTool.ViewModel.Base;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SPRNetTool.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDomainObserver, IDomainAccessors, IArtWizViewModel
    {
        #region Modules
        protected IBitmapDisplayManager BitmapDisplayManager
        { get { return IDomainAccessors.DomainContext.GetDomain<IBitmapDisplayManager>(); } }

        protected ISprWorkManager SprWorkManager
        { get { return IDomainAccessors.DomainContext.GetDomain<ISprWorkManager>(); } }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected IArtWizViewModelOwner? ViewModelOwner { get; private set; }
        protected bool IsViewModelDestroyed { get; private set; } = false;

        protected void Invalidate([CallerMemberName] string caller = "")
        {
            OnPropertyChanged(caller);
        }

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

        protected virtual void OnDomainChanged(IDomainChangedArgs args) { }


        void IDomainObserver.OnDomainChanged(IDomainChangedArgs args)
        {
            OnDomainChanged(args);
        }

        void IArtWizViewModel.OnCreate(IArtWizViewModelOwner owner)
        {
            ViewModelOwner = owner;
        }

        void IArtWizViewModel.OnDestroy()
        {
            IsViewModelDestroyed = true;
        }
    }
}
