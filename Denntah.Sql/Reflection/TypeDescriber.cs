using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Denntah.Sql.Reflection
{
    public class TypeDescriber : List<PropertyDescriber>
    {
        public Type Type { get; private set; }
        public string Table { get; private set; }
        /*public PropertyDescriber[] All { get; private set; }
        public PropertyDescriber[] Keys { get; private set; }
        public PropertyDescriber[] NonKeys { get; private set; }
        public PropertyDescriber[] Writeable { get; private set; }
        public PropertyDescriber[] Readable { get; private set; }*/

        private Hashtable _properties = new Hashtable();

        public TypeDescriber(Type type)
        {
            Type = type;
            Table = Type.GetTypeInfo().GetCustomAttribute<TableAttribute>()?.Name ?? Type.Name.ToLower();

            var properties = Type.GetProperties();

            foreach (var property in properties.Where(x => (x.CanRead || x.CanWrite)))
                Add(new PropertyDescriber(property));

            foreach (var property in properties)
            {
                var prop = new PropertyDescriber(property);
                _properties[prop.Property.Name] = prop.Property;
                _properties[prop.DbName] = prop.Property;
            }
        }

        public IEnumerable<PropertyDescriber> Keys
        {
            get { return this.Where(x => x.IsKey); }
        }

        public IEnumerable<PropertyDescriber> NonKeys
        {
            get { return this.Where(x => !x.IsKey); }
        }

        public IEnumerable<PropertyDescriber> Writeable
        {
            get { return this.Where(x => x.IsWriteable); }
        }

        public IEnumerable<PropertyDescriber> Readable
        {
            get { return this.Where(x => x.IsReadable && !x.IsGenerated); }
        }

        public IEnumerable<PropertyDescriber> Arguments
        {
            get { return this.Where(x => x.IsArgument); }
        }

        public IEnumerable<PropertyDescriber> Generated
        {
            get { return this.Where(x => x.IsGenerated); }
        }

        private Hashtable setters = new Hashtable();
        private Hashtable getters = new Hashtable();

        public void SetValue<S, T>(string propertyName, S obj, T value)
        {
            PropertyInfo prop = (PropertyInfo)_properties[propertyName];

            if (prop != null)
            {
                if (prop.PropertyType.IsEnum)
                    prop.SetValue(obj, Enum.Parse(prop.PropertyType, value.ToString()), null);
                else
                    prop.SetValue(obj, value, null);
            }
        }

        public object GetValue<S>(string propertyName, S obj)
        {
            PropertyInfo prop = (PropertyInfo)_properties[propertyName];

            if (prop != null)
                return prop.GetValue(obj, null);

            return null;
        }
    }
}
