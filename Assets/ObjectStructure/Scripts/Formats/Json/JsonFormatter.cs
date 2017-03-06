﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectStructure.Json
{
    public class JsonFormatException : ArgumentException
    {
        public JsonFormatException(string msg) : base(msg) { }
    }

    public class JsonFormatter : IFormatter<String>
    {
        IWriteStream m_w;

        enum Current
        { 
            NONE,
            ARRAY,
            OBJECT           
        }

        struct Context
        {
            public Current Current;
            public int Count;

            public Context(Current current)
            {
                Current = current;
                Count = 0;
            }
        }

        Stack<Context> m_stack = new Stack<Context>();

        public JsonFormatter()
            :this(new StringBuilderStream(new StringBuilder()))
        {}

        public JsonFormatter(IWriteStream w)
        {
            m_w = w;
            m_stack.Push(new Context(Current.NONE));
        }

        public IStore<String> GetStore()
        {
            return m_w;
        }

        public void Clear()
        {
            //m_w.Clear();
            m_stack.Clear();
            m_stack.Push(new Context(Current.NONE));
        }

        public string Buffer()
        {
            return m_w.Buffer();
        }

        void CommaCheck(bool isKey=false)
        {
            var top = m_stack.Pop();
            switch (top.Current)
            {
                case Current.NONE:
                    {
                        if (top.Count != 0) throw new JsonFormatException("multiple root value");
                    }
                    break;

                case Current.ARRAY:
                    {
                        if (top.Count != 0)
                        {
                            m_w.Write(',');
                        }
                    }
                    break;

                case Current.OBJECT:
                    {
                        if(top.Count%2==0)
                        {
                            if (!isKey) throw new JsonFormatException("key exptected");
                            if (top.Count != 0)
                            {
                                m_w.Write(',');
                            }
                        }
                        else
                        {
                            if (isKey) throw new JsonFormatException("key not exptected");
                        }
                    }
                    break;
            }
            top.Count += 1;
            m_stack.Push(top);
        }

        public void Null()
        {
            CommaCheck();
            m_w.Write("null");
        }

        public void BeginList(int n)
        {
            CommaCheck();
            m_w.Write('[');
            m_stack.Push(new Context(Current.ARRAY));
        }

        public void EndList()
        {
            m_w.Write(']');
            m_stack.Pop();
        }

        public void BeginMap(int n)
        {
            CommaCheck();
            m_w.Write('{');
            m_stack.Push(new Context(Current.OBJECT));
        }

        public void EndMap()
        {
            m_w.Write('}');
            m_stack.Pop();
        }

        public void Key(String key)
        {
            CommaCheck(true);
            m_w.Write(JsonString.Quote(key));
            m_w.Write(':');
        }

        public void Value(String key)
        {
            CommaCheck();
            m_w.Write(JsonString.Quote(key));
        }

        public void Value(Boolean x)
        {
            CommaCheck();
            m_w.Write(x ? "true" : "false");
        }

        public void Value(SByte x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Int16 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Int32 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Int64 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }

        public void Value(Byte x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(UInt16 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(UInt32 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(UInt64 x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }

        public void Value(Single x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }
        public void Value(Double x)
        {
            CommaCheck();
            m_w.Write(x.ToString());
        }

        public void Bytes(IEnumerable<byte> raw, int count)
        {
            throw new NotImplementedException();
            
            // ToDo: Base64 encoding
        }

        public void Dump(object o)
        {
            CommaCheck();
            m_w.Write((String)o);
        }
    }
}
