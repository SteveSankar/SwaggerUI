using Swashbuckle.AspNetCore.Filters;
using System.Collections;
using System.Reflection;

namespace SwaggerUI
{
    public class Employee 
    {
        [ExampleValue("Steve Rogers")]
        public string Name { get; set; }
        [ExampleValue(23)]
        public int Age { get; set; }
        [ExampleValue("234-424-4334")]
        public string Number { get; set; }
        public Address Address { get; set; }
        [ExampleValue(null)]
        public Department Department { get; set; }
    }

    public class Department
    {
        [ExampleValue("JBK-Department")]
        public string DepartmentName { get; set; }
        [ExampleValue("B-202")]
        public string BlockNumber { get; set; }
    }
    public class Address
    {
        [ExampleValue("456 Oak Avenue")]
        public string StreetName { get; set; }
        [ExampleValue("Los Angeles")]
        public string City { get; set; }
        [ExampleValue("California")]
        public string State { get; set; }
        [ExampleValue("United States")]
        public string Country { get; set; }

    }

   

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ExampleValueAttribute : Attribute
    {
        public object Value { get; }

        public ExampleValueAttribute(object value)
        {
            Value = value;
        }
    }



public class AttributeExampleProvider<T> : IExamplesProvider<T>
    {
        public T GetExamples()
        {
            var type = typeof(T);

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                // Note:- Handles the  collections...
                var elementType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
                var listType = typeof(List<>).MakeGenericType(elementType);
                var listInstance = (IList)Activator.CreateInstance(listType);

                //=.=.=.. Adding an example elements or element to the list...=.=.=.=
                var exampleElement = CreateExample(elementType);
                listInstance.Add(exampleElement);

                return (T)listInstance;
            }
            else
            {
                // Note:-  Handling single objects and adding example element
                return (T)CreateExample(type);
            }
        }

        private static object CreateExample(Type type)
        {
            if (type == typeof(string))
            {
                return "string"; 
            }
            else if (type.IsValueType)
            {
                // setting Default value for value types (e.g:  0  for int)
                return Activator.CreateInstance(type);
            }
            else
            {
                var example = Activator.CreateInstance(type);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (property.CanWrite)
                    {
                        var attribute = property.GetCustomAttributes(typeof(ExampleValueAttribute), false)
                                                .FirstOrDefault() as ExampleValueAttribute;

                        if (attribute != null)
                        {
                            try
                            {
                                var propertyType = property.PropertyType;
                                var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                                object value;

                                if (attribute.Value is string strValue && strValue.Equals("null", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = null;
                                }
                                else if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                                {
                                    //Note:-  Handling the collections inside objects
                                    var elementType = propertyType.IsGenericType ? propertyType.GetGenericArguments()[0] : propertyType.GetElementType();
                                    var listType = typeof(List<>).MakeGenericType(elementType);
                                    var listInstance = (IList)Activator.CreateInstance(listType);

                                    var exampleElement = CreateExample(elementType);
                                    listInstance.Add(exampleElement);

                                    value = listInstance;
                                }
                                else
                                {
                                    //Note:-  Handling the single values
                                    value = Convert.ChangeType(attribute.Value, targetType);
                                }

                                property.SetValue(example, value);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error setting property '{property.Name}': {ex.Message}");
                            }
                        }
                        else
                        {
                            try
                            {
                                // Handling the properties that are without ExampleValueAttribute
                                var propertyType = property.PropertyType;

                                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                                {
                                    //Note:-  Handling the collections inside objects
                                    var elementType = propertyType.IsGenericType ? propertyType.GetGenericArguments()[0] : propertyType.GetElementType();
                                    var listType = typeof(List<>).MakeGenericType(elementType);
                                    var listInstance = (IList)Activator.CreateInstance(listType);

                                    var exampleElement = CreateExample(elementType);
                                    listInstance.Add(exampleElement);

                                    property.SetValue(example, listInstance);
                                }
                                else
                                {
                                    // Handle complex objects or nested objects
                                    var nestedExample = CreateExample(propertyType);
                                    property.SetValue(example, nestedExample);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error setting property '{property.Name}': {ex.Message}");
                            }

                        }
                    }
                }

                return example;
            }
        }
    }






}
