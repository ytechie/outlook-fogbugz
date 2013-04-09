#region

using System;
using System.Collections;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

#endregion

namespace YTech.Fogbugz
{
    public class XmlDynamo : DynamicObject, IEnumerable
    {
        #region Fields

        protected readonly XElement Element;

        #endregion

        #region Constructors

        protected XmlDynamo(string uri)
        {
            Element = XElement.Load(uri);
        }

        protected XmlDynamo(string uri, LoadOptions options)
        {
            Element = XElement.Load(uri, options);
        }

        public XmlDynamo(XElement xElement)
        {
            Element = xElement;
        }

        protected XmlDynamo(Stream stream)
        {
            Element = XElement.Load(stream);
        }

        protected XmlDynamo(Stream stream, LoadOptions options)
        {
            Element = XElement.Load(stream, options);
        }

        protected XmlDynamo(TextReader textReader)
        {
            Element = XElement.Load(textReader);
        }

        protected XmlDynamo(TextReader textReader, LoadOptions options)
        {
            Element = XElement.Load(textReader, options);
        }

        protected XmlDynamo(XmlReader reader)
        {
            Element = XElement.Load(reader);
        }

        protected XmlDynamo(XmlReader reader, LoadOptions options)
        {
            Element = XElement.Load(reader, options);
        }

        #endregion

        #region Properties

        public string this[string attr]
        {
            get
            {
                return Element == null ? null : Element.Attribute(attr).Value;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return Element.Elements().GetEnumerator();
        }

        #endregion

        #region Public Members

        public dynamic Elements(string xName)
        {
            return Element.Elements(xName).Select(x => new XmlDynamo(x));
        }

        /// <summary>
        ///   Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///   true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((XmlDynamo)obj);
        }

        /// <summary>
        ///   Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///   A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            return (Element != null ? Element.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Element.ToString();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Element == null)
            {
                result = null;
                return false;
            }

            var child = Element.Element(binder.Name);

            if (child == null)
            {
                var attrib = Element.Attribute(binder.Name);
                if (attrib == null)
                {
                    result = null;
                    return true;
                }

                result = attrib.Value;
                return true;
            }

            result = new XmlDynamo(child);
            return true;
        }

        #endregion

        #region Protected Members

        protected bool Equals(XmlDynamo other)
        {
            return Equals(Element, other.Element);
        }

        #endregion

        #region Operators

        public static bool operator ==(XmlDynamo x, string y)
        {
            return (string)x == y;
        }

        public static bool operator ==(XmlDynamo x, int y)
        {
            return (int)x == y;
        }

        public static bool operator ==(XmlDynamo x, int? y)
        {
            return (int)x == y;
        }

        public static bool operator ==(XmlDynamo x, long y)
        {
            return (long)x == y;
        }

        public static bool operator ==(XmlDynamo x, long? y)
        {
            return (long?)x == y;
        }

        public static bool operator ==(XmlDynamo x, bool y)
        {
            return (bool)x == y;
        }

        public static bool operator ==(XmlDynamo x, bool? y)
        {
            return (bool?)x == y;
        }

        public static bool operator ==(XmlDynamo x, Guid y)
        {
            return (Guid)x == y;
        }

        public static bool operator ==(XmlDynamo x, Guid? y)
        {
            return (Guid?)x == y;
        }

        public static bool operator ==(XmlDynamo x, DateTimeOffset y)
        {
            return (DateTimeOffset)x == y;
        }

        public static bool operator ==(XmlDynamo x, DateTimeOffset? y)
        {
            return (DateTimeOffset?)x == y;
        }

        public static bool operator ==(XmlDynamo x, DateTime y)
        {
            return (DateTime)x == y;
        }

        public static bool operator ==(XmlDynamo x, DateTime? y)
        {
            return (DateTime?)x == y;
        }

        public static bool operator ==(XmlDynamo x, decimal y)
        {
            return (decimal)x == y;
        }

        public static bool operator ==(XmlDynamo x, decimal? y)
        {
            return (decimal?)x == y;
        }

        public static bool operator ==(XmlDynamo x, float y)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (float)x == y;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool operator ==(XmlDynamo x, float? y)
        {
            return (float?)x == y;
        }

        public static bool operator ==(XmlDynamo x, double y)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (double)x == y;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool operator ==(XmlDynamo x, double? y)
        {
            return (double?)x == y;
        }

        public static explicit operator XElement(XmlDynamo x)
        {
            return x.Element;
        }

        public static explicit operator int(XmlDynamo x)
        {
            return int.Parse(x.Element.Value);
        }

        public static explicit operator int?(XmlDynamo x)
        {
            return x.Element == null ? (int?)null : int.Parse(x.Element.Value);
        }

        public static explicit operator long(XmlDynamo x)
        {
            return long.Parse(x.Element.Value);
        }

        public static explicit operator long?(XmlDynamo x)
        {
            return x.Element == null ? (long?)null : long.Parse(x.Element.Value);
        }

        public static explicit operator decimal(XmlDynamo x)
        {
            return decimal.Parse(x.Element.Value);
        }

        public static explicit operator decimal?(XmlDynamo x)
        {
            return x.Element == null ? (decimal?)null : decimal.Parse(x.Element.Value);
        }

        public static explicit operator float(XmlDynamo x)
        {
            return float.Parse(x.Element.Value);
        }

        public static explicit operator float?(XmlDynamo x)
        {
            return x.Element == null ? (float?)null : float.Parse(x.Element.Value);
        }

        public static explicit operator double(XmlDynamo x)
        {
            return double.Parse(x.Element.Value);
        }

        public static explicit operator double?(XmlDynamo x)
        {
            return x.Element == null ? (double?)null : double.Parse(x.Element.Value);
        }

        public static explicit operator bool(XmlDynamo x)
        {
            return bool.Parse(x.Element.Value);
        }

        public static explicit operator bool?(XmlDynamo x)
        {
            return x.Element == null ? (bool?)null : bool.Parse(x.Element.Value);
        }

        public static explicit operator Guid(XmlDynamo x)
        {
            return Guid.Parse(x.Element.Value);
        }

        public static explicit operator Guid?(XmlDynamo x)
        {
            return x.Element == null ? (Guid?)null : Guid.Parse(x.Element.Value);
        }

        public static explicit operator DateTime(XmlDynamo x)
        {
            return DateTime.Parse(x.Element.Value);
        }

        public static explicit operator DateTime?(XmlDynamo x)
        {
            return x.Element == null ? (DateTime?)null : DateTime.Parse(x.Element.Value);
        }

        public static explicit operator DateTimeOffset(XmlDynamo x)
        {
            return DateTimeOffset.Parse(x.Element.Value);
        }

        public static explicit operator DateTimeOffset?(XmlDynamo x)
        {
            return x.Element == null ? (DateTimeOffset?)null : DateTimeOffset.Parse(x.Element.Value);
        }

        public static implicit operator string(XmlDynamo x)
        {
            return x.Element.Value;
        }

        public static bool operator !=(XmlDynamo x, string y)
        {
            return (string)x != y;
        }

        public static bool operator !=(XmlDynamo x, int y)
        {
            return (int)x != y;
        }

        public static bool operator !=(XmlDynamo x, int? y)
        {
            return (int?)x != y;
        }

        public static bool operator !=(XmlDynamo x, long y)
        {
            return (long)x != y;
        }

        public static bool operator !=(XmlDynamo x, long? y)
        {
            return (long?)x != y;
        }

        public static bool operator !=(XmlDynamo x, bool y)
        {
            return (bool)x != y;
        }

        public static bool operator !=(XmlDynamo x, bool? y)
        {
            return (bool?)x != y;
        }

        public static bool operator !=(XmlDynamo x, Guid y)
        {
            return (Guid)x != y;
        }

        public static bool operator !=(XmlDynamo x, Guid? y)
        {
            return (Guid?)x != y;
        }

        public static bool operator !=(XmlDynamo x, DateTimeOffset y)
        {
            return (DateTimeOffset)x != y;
        }

        public static bool operator !=(XmlDynamo x, DateTimeOffset? y)
        {
            return (DateTimeOffset?)x != y;
        }

        public static bool operator !=(XmlDynamo x, DateTime y)
        {
            return (DateTime)x != y;
        }

        public static bool operator !=(XmlDynamo x, DateTime? y)
        {
            return (DateTime?)x != y;
        }

        public static bool operator !=(XmlDynamo x, decimal y)
        {
            return (decimal)x != y;
        }

        public static bool operator !=(XmlDynamo x, decimal? y)
        {
            return (decimal?)x != y;
        }

        public static bool operator !=(XmlDynamo x, float y)
        {
            return Math.Abs((float)x - y) > 0;
        }

        public static bool operator !=(XmlDynamo x, float? y)
        {
            return (float?)x != y;
        }

        public static bool operator !=(XmlDynamo x, double y)
        {
            return Math.Abs((double)x - y) > 0;
        }

        public static bool operator !=(XmlDynamo x, double? y)
        {
            return (double?)x != y;
        }

        #endregion

        #region Public Static Members

        public static dynamic Load(string uri)
        {
            return new XmlDynamo(uri);
        }

        public static dynamic Load(string uri, LoadOptions options)
        {
            return new XmlDynamo(uri, options);
        }

        public static dynamic Load(Stream stream)
        {
            return new XmlDynamo(stream);
        }

        public static dynamic Load(Stream stream, LoadOptions options)
        {
            return new XmlDynamo(stream, options);
        }

        public static dynamic Load(TextReader textReader)
        {
            return new XmlDynamo(textReader);
        }

        public static dynamic Load(TextReader textReader, LoadOptions options)
        {
            return new XmlDynamo(textReader, options);
        }

        public static dynamic Load(XmlReader reader)
        {
            return new XmlDynamo(reader);
        }

        public static dynamic Load(XmlReader reader, LoadOptions options)
        {
            return new XmlDynamo(reader, options);
        }

        #endregion
    }
}
