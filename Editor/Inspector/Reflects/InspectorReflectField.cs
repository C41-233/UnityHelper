using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Inspector
{
    internal class InspectorReflectField
    {

        public string Name { get; }
        
        public string Descriptor { get; }

        private readonly Func<object, object> getter;
        
        public InspectorReflectType Type { get; }
        
        public InspectorReflectField(FieldInfo field)
        {
            Name = InspectorReflect.NormalizeName(field.Name);
            
            var descriptor = "[field] ";
            if (field.IsPublic)
            {
                descriptor += "public ";
            }
            else if (field.IsFamily)
            {
                descriptor+= "protected ";
            }
            else if (field.IsPrivate)
            {
                descriptor += "private ";
            }
            else if (field.IsAssembly)
            {
                descriptor += "internal ";
            }

            Type = InspectorReflectType.Get(field.FieldType);
            descriptor += Type.TypeName + " ";
            descriptor += Name;
            
            Descriptor = descriptor;

            var parameter = Expression.Parameter(typeof(object));
            getter = Expression.Lambda<Func<object, object>>(
                // ReSharper disable once AssignNullToNotNullAttribute
                Expression.Convert(
                    Expression.Field(
                        Expression.Convert(parameter, field.DeclaringType), 
                        field
                    ),
                typeof(object)
                ),
                parameter
            ).Compile();
        }

        public InspectorReflectField(PropertyInfo property)
        {
            Name = InspectorReflect.NormalizeName(property.Name);
            
            var descriptor = "[field] ";
            if (property.GetMethod.IsPublic)
            {
                descriptor += "public ";
            }
            else if (property.GetMethod.IsFamily)
            {
                descriptor+= "protected ";
            }
            else if (property.GetMethod.IsPrivate)
            {
                descriptor += "private ";
            }
            else if (property.GetMethod.IsAssembly)
            {
                descriptor += "internal ";
            }

            Type = InspectorReflectType.Get(property.PropertyType);
            descriptor += Type.TypeName + " ";
            descriptor += Name;
            
            Descriptor = descriptor;

            var parameter = Expression.Parameter(typeof(object));
            getter = Expression.Lambda<Func<object, object>>(
                // ReSharper disable once AssignNullToNotNullAttribute
                Expression.Convert(
                    Expression.Property(
                        Expression.Convert(parameter, property.DeclaringType), 
                        property
                    ),
                    typeof(object)
                ),
                parameter
            ).Compile();
        }

        public object GetValue(object host)
        {
            return getter(host);
        }
    }

}