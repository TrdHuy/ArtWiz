﻿using System.Collections.Generic;

namespace ArtWiz.ViewModel.Base
{
    public class BaseParentsViewModel : BaseViewModel, IArtWizViewModel
    {
        private List<BaseSubViewModel> subViewModels = new List<BaseSubViewModel>();
        protected IArtWizViewModelOwner? ViewModelOwner { get; private set; }
        protected bool IsViewModelDestroyed { get; private set; } = false;
        void IArtWizViewModel.OnArtWizViewModelOwnerCreate(IArtWizViewModelOwner owner)
        {
            ViewModelOwner = owner;
            foreach (var vm in subViewModels)
            {
                ((IArtWizViewModel)vm).OnArtWizViewModelOwnerCreate(owner);
            }
        }

        void IArtWizViewModel.OnDestroy()
        {
            IsViewModelDestroyed = true;
            foreach (var vm in subViewModels)
            {
                ((IArtWizViewModel)vm).OnDestroy();
            }
            subViewModels.Clear();
        }

        public void RegisterSubViewModel(BaseSubViewModel subViewModel)
        {
            if (!subViewModels.Contains(subViewModel))
            {
                subViewModels.Add(subViewModel);
            }
        }
    }
}
