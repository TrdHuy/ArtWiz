using ArtWiz.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.ViewModel.PakEditor.Base
{
   
    internal abstract class PakItemViewModel : BaseSubViewModel
    {
        private PakItemLoadingStatus _loadingStatus = PakItemLoadingStatus.NONE;
        protected long _itemSizeInBytes;

        public virtual PakItemLoadingStatus LoadingStatus
        {
            get => _loadingStatus;
            set
            {
                _loadingStatus = value;

            }
        }

        public PakItemViewModel(BaseParentsViewModel parents) : base(parents)
        {

        }

        protected string FormatFileSize(long bytes)
        {
            const int scale = 1024;
            string[] units = { "Bytes", "KB", "MB", "GB", "TB" };

            if (bytes < scale)
            {
                return $"{bytes} {units[0]}";
            }

            int unitIndex = (int)Math.Log(bytes, scale);
            double adjustedSize = bytes / Math.Pow(scale, unitIndex);

            return $"{adjustedSize:0.##} {units[unitIndex]}";
        }

    }

}
