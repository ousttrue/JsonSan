﻿using System;
using System.Collections.Generic;


namespace ObjectStructure
{
    public enum JsonValueType
    {
        Unknown,

        String,
        Number,
        Object,
        Array,
        Boolean,

        Close, // internal use
    }

    public interface IParser
    {
        String GetString();

        Byte GetByte();
        UInt16 GetUInt16();
        UInt32 GetUInt32();
        UInt64 GetUInt64();

        SByte GetSByte();
        Int16 GetInt16();
        Int32 GetInt32();
        Int64 GetInt64();

        Single GetSingle();
        Double GetDouble();

        IEnumerable<IParser> ArrayItems { get; }
        IParser this[int index] { get; }

        IEnumerable<KeyValuePair<String, IParser>> ObjectItems { get; }
        IParser this[string key] { get; }

        JsonValueType ValueType { get; }
    }
}
