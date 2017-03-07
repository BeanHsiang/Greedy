using System;

namespace Greedy.Toolkit.Sql
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class KeyAttribute : Attribute
    {
        public KeyType KeyType { get; set; }

        public KeyAttribute(KeyType type)
        {
            this.KeyType = type;
        }
    }

    public enum KeyType : byte
    {
        None,
        Identity,
        Snxowflake
    }
}