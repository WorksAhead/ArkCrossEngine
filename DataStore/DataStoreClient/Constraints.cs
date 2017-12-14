using System;
using System.Reflection;

using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Descriptors;

namespace DashFire.DataStore
{
    public class ConstraintException : ApplicationException
    {
        public ConstraintException ( string message ) : base(message) { }
    }

    internal class Constraints
    {
        public static void MaxSize ( IMessage msg )
        {
            MessageDescriptor md = msg.DescriptorForType;
            foreach ( FieldDescriptor fd in md.Fields )
            {
                if ( fd.FieldType == FieldType.String )
                {
                    int max_size = fd.Options.GetExtension<int>(Data.MaxSize);
                    string value = (string)msg.GetType().InvokeMember(fd.CSharpOptions.PropertyName,
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                      null, msg, null);
                    if ( value.Length > max_size )
                        throw new ConstraintException(string.Format("MaxSize Contraint break: {0}.{1} size({2}) max_size({3}", md.Name, fd.Name, value.Length, max_size));
                }

                if ( fd.FieldType == FieldType.Bytes )
                {
                    int max_size = fd.Options.GetExtension<int>(Data.MaxSize);
                    ByteString value = (ByteString)msg.GetType().InvokeMember(fd.CSharpOptions.PropertyName,
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                      null, msg, null);
                    if ( value.Length > max_size )
                        throw new ConstraintException(string.Format("MaxSize Contraint break: {0}.{1} size({2}) max_size({3}", md.Name, fd.Name, value.Length, max_size));
                }
            }
        }
    }
}