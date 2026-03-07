using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Tools.DynamicsCRM.Extensions
{
    public class Entity
    {
        public Dictionary<string, object> Attributes { get; set; }

        public Dictionary<string, string> FormattedValues { get; set; }

        public Entity()
        {
            Attributes = [];
            FormattedValues = [];
        }

        public Entity(ExpandoObject expandoObject)
        {
            Attributes = [];
            FormattedValues = [];

            string formattedAttributePostFix = "@OData.Community.Display.V1.FormattedValue";
            KeyValuePair<string, object>[] attributes = expandoObject.ToArray();

            for (int i = 0; i < attributes.Length; i++)
            {
                KeyValuePair<string, object> keyValuePair = attributes[i];
                object value = keyValuePair.Value;
                string valueFieldName = keyValuePair.Key;
                Type convertValueToType = null;

                if (value != null)
                {
                    if (value is string stringValue && Guid.TryParse(stringValue, out Guid id))
                    {
                        value = id;
                        PropertyInfo propertyWithEntityRefAttribute = GetPublicInstanceProperties()
                            .FirstOrDefault(x => x.GetCustomAttribute<EntityReferenceAttribute>()?.ValueField?.Equals(valueFieldName, StringComparison.OrdinalIgnoreCase) == true);

                        if (propertyWithEntityRefAttribute != null)
                        {
                            EntityReferenceAttribute entityReferenceAttribute = propertyWithEntityRefAttribute.GetCustomAttribute<EntityReferenceAttribute>();
                            propertyWithEntityRefAttribute.SetValue(this, new EntityReference(entityReferenceAttribute.EntitySetName, id));
                        }
                    }
                    else
                    {
                        if (value is ExpandoObject valueExpandoObject)
                        {
                            PropertyInfo propertyWithEntityAttribute = GetPublicInstanceProperties()
                                .FirstOrDefault(x => x.GetCustomAttribute<EntityAttribute>()?.AttributeName?.Equals(valueFieldName, StringComparison.OrdinalIgnoreCase) == true);

                            if (propertyWithEntityAttribute != null)
                            {
                                value = Activator.CreateInstance(propertyWithEntityAttribute.PropertyType, valueExpandoObject);
                            }
                        }
                        else
                        {
                            PropertyInfo propertyForField = GetPublicInstanceProperties()
                                .FirstOrDefault(x => x.Name.Equals(valueFieldName, StringComparison.OrdinalIgnoreCase));

                            if (propertyForField != null)
                            {
                                convertValueToType = propertyForField.PropertyType;
                            }
                        }
                    }

                    if (convertValueToType != null)
                    {
                        if (convertValueToType.IsGenericType && convertValueToType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            convertValueToType = Nullable.GetUnderlyingType(convertValueToType);
                        }

                        value = Convert.ChangeType(value, convertValueToType, CultureInfo.InvariantCulture);
                    }

                    if (valueFieldName.EndsWith(formattedAttributePostFix, StringComparison.Ordinal))
                    {
                        FormattedValues.Add(valueFieldName.Replace(formattedAttributePostFix, string.Empty), value?.ToString());
                    }
                    else
                    {
                        Attributes.Add(valueFieldName, value);
                    }
                }
            }
        }

        public Guid Id
        {
            get => GetAttributeValue<Guid>(GetIdAttribute());
            set => SetAttributeValue(GetIdAttribute(), value);
        }

        public string EntitySetName => GetType()
            .GetField(nameof(EntitySetName), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null)?.ToString();

        public string EntityLogicalName => GetType()
            .GetField(nameof(EntityLogicalName), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null)?.ToString();

        public string PrimaryIdAttribute => GetType()
            .GetField(nameof(PrimaryIdAttribute), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null)?.ToString();

        public T GetAttributeValue<T>(string attributeName)
        {
            KeyValuePair<string, object> keyValuePair = Attributes
                .FirstOrDefault(x => x.Key.Equals(attributeName, StringComparison.Ordinal));

            return keyValuePair.Value != null ? (T)keyValuePair.Value : default;
        }

        public void SetAttributeValue(string attributeName, object value)
        {
            if (!Attributes.TryAdd(attributeName, value))
            {
                Attributes[attributeName] = value;
            }
        }

        public ExpandoObject ToExpandoObject()
        {
            dynamic expando = new ExpandoObject();
            IDictionary<string, object> expandoObject = (IDictionary<string, object>)expando;
            KeyValuePair<string, object>[] attributes = Attributes.ToArray();

            for (int i = 0; i < attributes.Length; i++)
            {
                KeyValuePair<string, object> keyValuePair = attributes[i];
                object value = keyValuePair.Value;
                string key = keyValuePair.Key;

                if (value is EntityReference entityReference)
                {
                    value = $"/{entityReference.EntitySetName}({entityReference.EntityId})";
                }
                else
                {
                    key = key.ToLowerInvariant();

                    if (value is DateTime dateTimeValue)
                    {
                        PropertyInfo propertyForAttribute = GetPublicInstanceProperties()
                            .FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

                        if (propertyForAttribute != null)
                        {
                            OnlyDateAttribute onlyDateAttr = propertyForAttribute.GetCustomAttribute<OnlyDateAttribute>();

                            if (onlyDateAttr != null)
                            {
                                value = dateTimeValue.ToString(OnlyDateAttribute.Format, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }

                expandoObject.Add(key, value);
            }

            return (ExpandoObject)expandoObject;
        }

        public EntityReference ToEntityReference()
        {
            return new EntityReference(EntitySetName, Id);
        }

        private PropertyInfo[] GetPublicInstanceProperties()
        {
            return GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private string GetIdAttribute()
        {
            return GetType()
                .GetField("PrimaryIdAttribute", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                ?.GetValue(null)?.ToString();
        }
    }

    public class EntityReference
    {
        public EntityReference(string entitySetName, Guid entityId)
        {
            EntitySetName = entitySetName;
            EntityId = entityId;
        }

        public string EntitySetName { get; set; }

        public Guid EntityId { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class EntityReferenceAttribute : Attribute
    {
        public EntityReferenceAttribute(string entitySetName, string valueField)
        {
            EntitySetName = entitySetName;
            ValueField = valueField;
        }

        public string EntitySetName { get; set; }

        public string ValueField { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute(string entityLogicalName, string attributeName)
        {
            EntityLogicalName = entityLogicalName;
            AttributeName = attributeName;
        }

        public string EntityLogicalName { get; set; }

        public string AttributeName { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class OnlyDateAttribute : Attribute
    {
        public const string Format = "yyyy-MM-dd";
    }
}