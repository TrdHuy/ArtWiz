namespace SPRNetTool.ViewModel.Base
{
    public abstract class BaseSubViewModel : BaseViewModel, IArtWizViewModel
    {
        protected BaseViewModel Parents;
        protected IArtWizViewModelOwner? ViewModelOwner { get; private set; }
        protected bool IsViewModelDestroyed { get; private set; } = false;

        public BaseSubViewModel(BaseParentsViewModel parents)
        {
            Parents = parents;
            parents.RegisterSubViewModel(this);
        }

        protected virtual void OnDestroy()
        {
            IsViewModelDestroyed = true;
        }

        protected virtual void OnCreate(IArtWizViewModelOwner owner)
        {
            ViewModelOwner = owner;
        }

        void IArtWizViewModel.OnArtWizViewModelOwnerCreate(IArtWizViewModelOwner owner)
        {
            OnCreate(owner);
        }

        void IArtWizViewModel.OnDestroy()
        {
            OnDestroy();
        }
    }
}
