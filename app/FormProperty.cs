using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ReactiveUI;
using System.Reactive;

namespace app
{    
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
                .Where(p => p.Name != "Name" && p.Name != "Path" && p.Name != "IsLocked")
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
            else if (type.IsGenericType) {
                var elementType = type.GenericTypeArguments[0];
                var notifyElementType = elementType.GenericTypeArguments[0];

                var notifyBaseType = typeof(VMGuide.NotifyChanged<>).MakeGenericType(new Type[] {notifyElementType});
                var baseType = typeof(ICollection<>).MakeGenericType(new Type[] {notifyBaseType});
                
                if (baseType.IsAssignableFrom(type) && notifyElementType.IsEnum)
                    return new EnumCollectionFormProperty(obj, p);
            }
            
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
        
        public CollectionFormProperty(object obj, PropertyInfo prop): base(obj, prop) {}

        public ReactiveCommand<Unit, Unit> Add => ReactiveCommand.Create(() => {
            var type = Value.GetType();
            var elementType = type.GenericTypeArguments[0];
            var item = Activator.CreateInstance(elementType);
            type.GetMethod("Add").Invoke(Value, new object[] {item});
        });

        public ReactiveCommand<object, Unit> Remove => ReactiveCommand.Create<object, Unit>((item) => {
            var type = Value.GetType();
            var elementType = type.GenericTypeArguments[0];
            type.GetMethod("Remove").Invoke(Value, new object[] {item});
            return default(Unit);
        });
    }

    public class EnumCollectionFormProperty : CollectionFormProperty {
        public EnumCollectionFormProperty(object obj, PropertyInfo prop): base(obj, prop) {
            var type = Value.GetType();
            var elementType = type.GenericTypeArguments[0];
            var notifyElementType = elementType.GenericTypeArguments[0];

            Options = notifyElementType.GetEnumValues().OfType<Enum>().ToList();
        }
        public List<Enum> Options { get; }
    }
}