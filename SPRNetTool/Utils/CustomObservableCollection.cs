using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SPRNetTool.Utils
{
    public class CustomNotifyCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
    {
        public bool IsSwitchAction { get; private set; }
        public int Switched1stIndex { get; private set; }
        public int Switched2ndIndex { get; private set; }

        public CustomNotifyCollectionChangedEventArgs(int index1, int index2) : base(NotifyCollectionChangedAction.Reset)
        {
            IsSwitchAction = true;
            Switched1stIndex = index1;
            Switched2ndIndex = index2;
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action) : base(action)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList? changedItems) : base(action, changedItems)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem) : base(action, changedItem)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems) : base(action, newItems, oldItems)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList? changedItems, int startingIndex) : base(action, changedItems, startingIndex)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem, int index) : base(action, changedItem, index)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object? newItem, object? oldItem) : base(action, newItem, oldItem)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex) : base(action, newItems, oldItems, startingIndex)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList? changedItems, int index, int oldIndex) : base(action, changedItems, index, oldIndex)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem, int index, int oldIndex) : base(action, changedItem, index, oldIndex)
        {
        }

        public CustomNotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object? newItem, object? oldItem, int index) : base(action, newItem, oldItem, index)
        {
        }

    }
    public class CustomObservableCollection<T> : ObservableCollection<T>
    {
        public void SwitchItem(int index1, int index2)
        {
            if (index1 == index2 || index1 >= Count || index2 >= Count)
            {
                return;
            }
            var firstItem = this[index1];
            Items[index1] = this[index2];
            Items[index2] = firstItem;
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new CustomNotifyCollectionChangedEventArgs(index1, index2));
        }
    }
}
