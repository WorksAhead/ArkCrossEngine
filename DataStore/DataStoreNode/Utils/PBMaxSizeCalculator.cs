using System;
using System.Collections.Generic;

using DashFire.DataStore;
using Google.ProtocolBuffers;
using Google.ProtocolBuffers.Descriptors;

/* 
 * calculate the max size which a message occupied
 * note that for these field below:
 *  1. string/bytes field, sz_option.max_size MUST BE SET
 *  2. repeated field, sz_option.max_length MUST BE SET, if the element type is
 *     string/bytes, sz_option.max_size MUST BE SET TOO
 */
internal static class PBMaxSizeCalculator
{
    internal static UInt32 ComputeMaxSize ( IMessage data )
    {
        UInt32 max_size;
        if ( !max_size_cache_.TryGetValue(data.GetType(), out max_size) )
        {
            max_size = (UInt32)ComputeMaxSize(data.DescriptorForType);
            max_size_cache_.Add(data.GetType(), max_size);
        }
        return max_size;
    }

    private static int ComputeMaxSize ( MessageDescriptor md )
    {
        int size = 0;
        foreach ( FieldDescriptor fd in md.Fields )
        {
            System.Diagnostics.Debug.Assert(!fd.IsRepeated);
            size += ComputeFieldMaxSize(fd);
        }
        return size;
    }

    private static int ComputeFieldMaxSize ( FieldDescriptor fd )
    {
        int size = CodedOutputStream.ComputeTagSize(fd.FieldNumber);
        switch ( fd.FieldType )
        {
            case FieldType.Double:
            case FieldType.Fixed64:
            case FieldType.SFixed64:
            {
                size += 8;
                break;
            }
            case FieldType.Float:
            case FieldType.Fixed32:
            case FieldType.SFixed32:
            {
                size += 4;
                break;
            }
            case FieldType.Bool:
            {
                ++size;
                break;
            }
            case FieldType.Int64:
            case FieldType.UInt64:
            case FieldType.SInt64:
            {
                size += CodedOutputStream.ComputeRawVarint64Size(ulong.MaxValue);
                break;
            }
            case FieldType.Int32:
            case FieldType.Enum:
            {
                size += 10;
                break;
            }
            case FieldType.UInt32:
            case FieldType.SInt32:
            {
                size += CodedOutputStream.ComputeRawVarint32Size(uint.MaxValue);
                break;
            }
            case FieldType.String:
            case FieldType.Bytes:
            {
                int max_size = fd.Options.GetExtension<int>(Data.MaxSize);
                size += CodedOutputStream.ComputeRawVarint32Size((uint)max_size) + max_size;
                break;
            }
            case FieldType.Message:
            {
                int max_size = ComputeMaxSize(fd.MessageType);
                size += CodedOutputStream.ComputeRawVarint32Size((uint)max_size) + max_size;
                break;
            }
            default:
            {
                System.Diagnostics.Debug.Assert(false, string.Format("{0} is not supported", fd.FieldType.ToString()));
                break;
            }
        }
        return size;
    }

    private static Dictionary<Type, UInt32> max_size_cache_ = new Dictionary<Type, UInt32>();
}