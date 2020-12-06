using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VMGuide
{
    public class FormPropertyAttribute : Attribute {}

    public class FormProperty : INotifyPropertyChanged
    {
        protected object obj;
        protected PropertyInfo prop;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string Name { get; }
        public object Value { 
            get => prop.GetValue(obj);
            set => prop.SetValue(obj, value);
        }

        protected FormProperty(object obj, PropertyInfo prop) {
            var type = prop.GetMethod.ReturnType;

            this.obj = obj;
            this.prop = prop;

            var attr = prop.GetCustomAttribute<DescriptionAttribute>();
            Name = attr?.Description ?? prop.Name;

            // 将内部属性的 PropertyChanged 事件传出来
            var observe = obj as INotifyPropertyChanged;
            if (observe != null) {
                observe.PropertyChanged += (s,e) => {
                    if (e.PropertyName == this.prop.Name) {
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                    }
                };
            }
        }

        public static IEnumerable<FormProperty> GetFormProperties(object obj) {
            var t = obj.GetType();

            return t.GetProperties()
                .Where(p => p.GetCustomAttribute<FormPropertyAttribute>() != null)
                .Select(p => FormProperty.GetFormProperty(obj, p))
                .Where(p => p != null);
        }

        private static FormProperty GetFormProperty(object obj, PropertyInfo p) {
            var type = p.GetMethod.ReturnType;

            if (type == typeof(String))
                return new StringFormProperty(obj, p);
            else if (type == typeof(Boolean))
                return new BooleanFormProperty(obj, p);
            else if (type == typeof(DateTime))
                return new DateTimeFormProperty(obj, p);
            else if (type.IsEnum)
                return new EnumFormProperty(obj, p);
            else if (type.IsGenericTypeOf(typeof(ICollection<>), out _))
                return new CollectionFormProperty(obj, p);
            
            return null;
        }
    }

    public class StringFormProperty : FormProperty {
        public StringFormProperty(object obj, PropertyInfo prop): base(obj, prop) {}
    }

    public class BooleanFormProperty : FormProperty {
        public BooleanFormProperty(object obj, PropertyInfo prop): base(obj, prop) {}
    }

    public class DateTimeFormProperty : FormProperty {
        public DateTimeFormProperty(object obj, PropertyInfo prop): base(obj, prop) {}
    }

    public class EnumFormProperty : FormProperty {
        public EnumFormProperty(object obj, PropertyInfo prop): base(obj, prop) {
            var type = prop.GetMethod.ReturnType;
            Options = type.GetEnumValues().OfType<Enum>().ToList();
        }
        public List<Enum> Options { get; }
    }

    public class CollectionFormProperty : FormProperty {
        private dynamic list;
        public ObservableCollection<FormProperty> PropertyList { get; }

        public CollectionFormProperty(object obj, PropertyInfo prop): base(obj, prop) {
            list = Value;
            PropertyList = new ObservableList<FormProperty>();

            foreach (object item in list) {
                var props = GetFormProperties(item);
                foreach (var p in props) PropertyList.Add(p);
            };
        }

        public void Add() {
            var type = Value.GetType();
            var elementType = type.GenericTypeArguments[0];
            dynamic item = Activator.CreateInstance(elementType);
            Add(item);
        }

        public void Add(dynamic item) {
            list.Add(item);

            var props = GetFormProperties(item);
            foreach (var p in props) PropertyList.Add(p);
        }

        public void Remove(FormProperty prop) {
            var index = PropertyList.IndexOf(prop);

            PropertyList.RemoveAt(index);
            list.RemoveAt(index);
        }
    }
}