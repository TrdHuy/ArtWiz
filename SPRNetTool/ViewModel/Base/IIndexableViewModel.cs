using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPRNetTool.ViewModel.Base
{
    public interface IIndexableViewModel
    {
        public long Index { get; set; }
    }

    public static class Extension
    {
        public static void Add<T>(this ObservableCollection<T> src, T item) where T : IIndexableViewModel
        {
            item.Index = src.Count;
            src.Add(item);
        }

        public static ObservableCollection<T> ToIndexableObservableCollection<T>(this IEnumerable<T> src) where T : IIndexableViewModel
        {
            var i = 0;
            foreach (var item in src)
            {
                item.Index = i++;
            }
            return new ObservableCollection<T>(src);
        }

        public static void ReIndexObservableCollection<T>(this IEnumerable<T> src) where T : IIndexableViewModel
        {
            var i = 0;
            foreach (var item in src)
            {
                item.Index = i++;
            }
        }
    }

}
