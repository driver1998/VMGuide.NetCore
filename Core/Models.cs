
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VMGuide
{
    public class ObservableList<T> : ObservableCollection<T>
    {
        public ObservableList() : base() {}
        public ObservableList(IEnumerable<T> collection) : base(collection) {}
        public ObservableList(IList<T> list) : base(list) {}

        public void AddRange(IEnumerable<T> range) {
            CheckReentrancy();

            foreach(var item in range) Items.Add(item);
            var changeList = new List<T>(range);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changeList));
        }
    }

    public class NotifyChanged<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyChanged() { _value = default(T); }
        public NotifyChanged(T defaultValue) { _value = defaultValue; }

        private T _value;

        [FormProperty]
        public T Value
        {
            get => _value;
            set
            {
                if (!value.Equals(_value)) {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public override string ToString() => _value.ToString();
    }
}
