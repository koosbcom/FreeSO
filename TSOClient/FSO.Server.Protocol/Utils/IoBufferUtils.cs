﻿using Mina.Core.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Protocol.Utils
{
    public static class IoBufferUtils
    {
        public static void PutSerializable(this IoBuffer buffer, object obj)
        {
            buffer.PutSerializable(obj, false);
        }


        public static IoBuffer SerializableToIoBuffer(object obj)
        {
            if (obj is IoBuffer)
            {
                var ioBuffer = (IoBuffer)obj;
                return (IoBuffer)ioBuffer;
            }
            else if (obj is byte[])
            {
                var byteArray = (byte[])obj;
                return IoBuffer.Wrap(byteArray);
            }
            else if (obj is IoBufferSerializable)
            {
                var serializable = (IoBufferSerializable)obj;
                var ioBuffer = serializable.Serialize();
                ioBuffer.Flip();
                return ioBuffer;
            }

            throw new Exception("Unknown serializable type: " + obj);
        }

        public static void PutSerializable(this IoBuffer buffer, object obj, bool writeLength)
        {
            if(obj is IoBuffer)
            {
                var ioBuffer = (IoBuffer)obj;
                if (writeLength){
                    buffer.PutUInt32((uint)ioBuffer.Remaining);
                }
                buffer.Put(ioBuffer);
            }else if(obj is byte[])
            {
                var byteArray = (byte[])obj;
                if (writeLength)
                {
                    buffer.PutUInt32((uint)byteArray.Length);
                }
                buffer.Put(byteArray);
            }else if(obj is IoBufferSerializable)
            {
                var serializable = (IoBufferSerializable)obj;
                var ioBuffer = serializable.Serialize();
                ioBuffer.Flip();

                if (writeLength)
                {
                    buffer.PutUInt32((uint)ioBuffer.Remaining);
                }
                buffer.Put(ioBuffer);
            }
        }


        public static void PutUInt32(this IoBuffer buffer, uint value)
        {
            int converted = unchecked((int)value);
            buffer.PutInt32(converted);
        }

        public static uint GetUInt32(this IoBuffer buffer)
        {
            return (uint)buffer.GetInt32();
        }

        public static void PutUInt16(this IoBuffer buffer, ushort value)
        {
            buffer.PutInt16((short)value);
        }

        public static void PutUInt64(this IoBuffer buffer, ulong value)
        {
            buffer.PutInt64((long)value);
        }


        public static void PutEnum<T>(this IoBuffer buffer, T enumValue)
        {
            ushort value = Convert.ToUInt16((object)enumValue);
            buffer.PutUInt16(value);
        }


        public static void PutUTF8(this IoBuffer buffer, string value)
        {
            if (value == null)
            {
                buffer.PutInt16(-1);
            }
            else
            {
                buffer.PutInt16((short)value.Length);
                buffer.PutString(value, Encoding.UTF8);
            }
        }

        public static string GetUTF8(this IoBuffer buffer)
        {
            short len = buffer.GetInt16();
            if (len == -1)
            {
                return null;
            }
            return buffer.GetString(len, Encoding.UTF8);
        }

        public static ushort GetUInt16(this IoBuffer buffer)
        {
            return (ushort)buffer.GetInt16();
        }

        public static ulong GetUInt64(this IoBuffer buffer)
        {
            return (ulong)buffer.GetInt64();
        }

        public static T GetEnum<T>(this IoBuffer buffer)
        {
            return (T)Enum.Parse(typeof(T), buffer.GetUInt16().ToString());
        }

        public static String GetPascalVLCString(this IoBuffer buffer)
        {
            byte lengthByte = 0;
            uint length = 0;
            int shift = 0;

            do
            {
                lengthByte = buffer.Get();
                length |= (uint)((lengthByte & (uint)0x7F) << shift);
                shift += 7;
            } while (
                (lengthByte >> 7) == 1
            );

            if (length > 0)
            {
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    str.Append((char)buffer.Get());
                }
                return str.ToString();

            }
            else
            {
                return "";
            }
        }

        public static byte[] GetPascalVLCString(String value)
        {
            if(value == null)
            {
                return new byte[] { 0x00 };
            }

            //TODO: Support strings bigger than 128 chars
            var buffer = new byte[1 + value.Length];
            buffer[0] = (byte)value.Length;

            var chars = value.ToCharArray();

            for(int i=0; i < chars.Length; i++){
                buffer[i + 1] = (byte)chars[i];
            }

            return buffer;
        }

        public static void PutPascalVLCString(this IoBuffer buffer, String value)
        {
            long strlen = 0;
            if (value != null)
            {
                strlen = value.Length;
            }

            //TODO: VLC
            buffer.Put((byte)strlen);

            if (strlen > 0)
            {
                foreach (char ch in value.ToCharArray())
                {
                    buffer.Put((byte)ch);
                }
            }
        }

        public static String GetPascalString(this IoBuffer buffer)
        {
            byte len1 = buffer.Get();
            byte len2 = buffer.Get();
            byte len3 = buffer.Get();
            byte len4 = buffer.Get();
            len1 &= 0x7F;

            long len = len1 << 24 | len2 << 16 | len3 << 8 | len4;
            if (len > 0)
            {
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    str.Append((char)buffer.Get());
                }
                return str.ToString();

            }
            else
            {
                return "";
            }
        }

        public static void PutPascalString(this IoBuffer buffer, String value)
        {

            long strlen = 0;
            if (value != null)
            {
                strlen = value.Length;
            }

            byte len1 = (byte)((strlen >> 24) | 0x80);
            byte len2 = (byte)((strlen >> 16) & 0xFF);
            byte len3 = (byte)((strlen >> 8) & 0xFF);
            byte len4 = (byte)(strlen & 0xFF);

            buffer.Put(len1);
            buffer.Put(len2);
            buffer.Put(len3);
            buffer.Put(len4);

            if (strlen > 0)
            {
                foreach (char ch in value.ToCharArray())
                {
                    buffer.Put((byte)ch);
                }
            }
        }
    }
}