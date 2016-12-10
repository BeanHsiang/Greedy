using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Greedy.Toolkit.Sql
{
    class MemberMapper
    {
        internal string Name { get; set; }
        internal string ColumnName { get; set; }
        internal bool IsKey { get; set; }
        internal bool IsIdentity { get; set; }
        //internal bool IsBasicType { get; set; }

        public MemberMapper()
        {

        }

        public MemberMapper(PropertyInfo property)
        {
            this.Name = property.Name;
#if NET45
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
#else
            var columnAttr = property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
#endif

            this.ColumnName = columnAttr == null ? this.Name : columnAttr.Name;

#if NET45
            var keyAttr = property.GetCustomAttribute<KeyAttribute>();
#else
            var keyAttr = property.GetCustomAttributes(typeof(KeyAttribute), true).FirstOrDefault() as KeyAttribute;
#endif
            if (keyAttr != null)
            {
                this.IsKey = true;
                this.IsIdentity = keyAttr.KeyType == KeyType.Identity;
            }
        }
    }

    class MemberMapperComparer : IEqualityComparer<MemberMapper>
    {

        public bool Equals(MemberMapper x, MemberMapper y)
        {
            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(MemberMapper obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            return obj.Name.ToLower().GetHashCode();
        }
    }
}