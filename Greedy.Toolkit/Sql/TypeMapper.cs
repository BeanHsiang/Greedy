using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Greedy.Toolkit.Sql
{
    interface ITypeMapper
    {
        int Code { get; }
        string Name { get; }
        string TableName { get; }
        IEnumerable<MemberMapper> AllMembers { get; }
        IEnumerable<MemberMapper> GetKeyMembers(bool withIdentityMember);
    }

    class TypeMapper : ITypeMapper
    {
        internal const string Anonymous_Name = "Anonymous";
        internal const string Dictionary_Name = "Dictionary";
        public int Code { get; set; }
        public string Name { get; set; }
        public string TableName { get; set; }
        public IEnumerable<MemberMapper> AllMembers { get; set; }

        public IEnumerable<MemberMapper> GetKeyMembers(bool withIdentityMember)
        {
            return withIdentityMember ? this.AllMembers.Where(m => m.IsKey) : this.AllMembers.Where(m => m.IsKey && !m.IsIdentity);
        }

        public TypeMapper()
        {
        }

        public TypeMapper(object obj)
        {
            if (obj is IEnumerable<KeyValuePair<string, object>>)
            {
                this.Name = Dictionary_Name;
                var dic = obj as IEnumerable<KeyValuePair<string, object>>;
                this.AllMembers = dic.Select(i => new MemberMapper() { Name = i.Key, ColumnName = i.Key });
            }
            else if (obj is Type)
            {
                InitTypeMapper(obj as Type);
            }
            else
            {
                InitTypeMapper(obj.GetType());
            }
            this.Code = GetIdentity();
        }

        private void InitTypeMapper(Type type)
        {
            if (type.Name.Contains(Anonymous_Name))
            {
                this.Name = Anonymous_Name;
                InitMemberMappers(type);
            }
            else
            {
                this.Name = type.Name;
                InitMemberMappers(type);
#if NET45
                var tableAttr = type.GetCustomAttribute<TableAttribute>();
#else
                var tableAttr = type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
#endif
                this.TableName = tableAttr == null ? this.Name : tableAttr.Name;
                if (!this.AllMembers.Any(m => m.IsKey))
                {
                    var id = this.AllMembers.SingleOrDefault(m => m.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
                    //if (id == null)
                    //{
                    //    throw new ArgumentException("there isn't any key property of the type to map", type.Name);
                    //}
                    if (id != null)
                    {
                        id.IsKey = true;
                        id.IsIdentity = true;
                    }
                }
            }
        }

        private void InitMemberMappers(Type type)
        {
            this.AllMembers = type.GetProperties().Where(p => !p.GetCustomAttributes<NoMapAttribute>().Any()).Select(p => new MemberMapper(p)).ToList();
        }

        private int GetIdentity()
        {
            var sb = new StringBuilder();
            sb.Append(this.Name);
            sb = this.AllMembers.OrderBy(i => i.Name).Aggregate(sb, (s, item) => { return s.AppendFormat("|{0}", item.Name.ToLower()); });
            return sb.ToString().GetHashCode();
        }
    }
}