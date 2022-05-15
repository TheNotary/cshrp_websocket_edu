﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Usage:  using WebsocketEduTest.Extensions;
namespace WebsocketEduTest.Extensions
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }
        public static T[] SubArray<T>(this T[] array, int offset)
        {
            int length = array.Length - offset;
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        public static byte[] ToBytes(this string meh)
        {
            return Encoding.UTF8.GetBytes(meh);
        }
    }

}