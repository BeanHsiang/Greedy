using System;
using System.Collections.Generic;
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
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            this.ColumnName = columnAttr == null ? this.Name : columnAttr.Name;
            var keyAttr = property.GetCustomAttribute<KeyAttribute>();
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

            return obj.Name.GetHashCode();
        }
    }
}