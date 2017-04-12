using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;

namespace DesktopTestData
{
    [DataContract]
    public struct VT
    {
        [DataMember]
        public int b;

        public VT(int v)
        {
            b = v;
        }
    }

    public struct NotSer
    {
        public int a;
    }

    [DataContract]
    public enum MyEnum1 : byte
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum2 : sbyte
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum3 : short
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum4 : ushort
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum5 : int
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum6 : uint
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum7 : long
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    [DataContract]
    public enum MyEnum8 : ulong
    {
        [EnumMember]
        red,
        [EnumMember]
        blue,
        [EnumMember]
        black
    }

    public class SeasonsEnumContainer
    {

        public Seasons1 member1 = Seasons1.Autumn;

        public Seasons2 member2 = Seasons2.Spring;

        public Seasons3 member3 = Seasons3.Winter;
    }

    [Flags]
    public enum Seasons1 : byte
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons2 : sbyte
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons3 : short
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons4 : ushort
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons5 : int
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons6 : uint
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons7 : long
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    [Flags]
    public enum Seasons8 : ulong
    {
        None = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 4,
        Spring = 8,
        All = Summer | Autumn | Winter | Spring,
    }

    public struct MyStruct
    {
        public int value;
        public string globName;

        public MyStruct(bool init)
        {
            value = 10;
            globName = "ﺥﺣﺨﺪﺣﺻﻂﻃ";
        }
    }
}
