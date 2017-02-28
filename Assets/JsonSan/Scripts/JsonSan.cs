﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// reference: http://www.json.org/json-ja.html
/// </summary>
namespace JsonSan
{
    public enum ValueType
    {
        Unknown,

        String,
        Number,
        Object,
        Array,
        Boolean,
    }

    public struct StringSegment : IEnumerable<Char>
    {
        public string Value;
        public int Offset;
        public int Count;

        public char this[int index]
        {
            get
            {
                if (index >= Count) throw new ArgumentOutOfRangeException();
                return Value[Offset + index];
            }
        }

        public StringSegment(string value) : this(value, 0, value.Length) { }
        public StringSegment(string value, int offset) : this(value, offset, value.Length - offset) { }
        public StringSegment(string value, int offset, int count)
        {
            Value = value;
            Offset = offset;
            Count = count;
        }

        public bool IsMatch(string str)
        {
            if (Count != str.Length) return false;
            return Value.Substring(Offset, Count) == str;
        }

        public override bool Equals(object obj)
        {
            if (obj is StringSegment)
            {
                return this.Equals((StringSegment)obj);
            }
            return false;
        }

        public bool Equals(StringSegment p)
        {
            return (Value == p.Value) && (Offset == p.Offset) && (Count==p.Count);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ Offset ^ Count;
        }

        public static bool operator ==(StringSegment lhs, StringSegment rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(StringSegment lhs, StringSegment rhs)
        {
            return !(lhs.Equals(rhs));
        }

        public override string ToString()
        {
            return Value.Substring(Offset, Count);
        }

        public IEnumerator<char> GetEnumerator()
        {
            return Value.Skip(Count).Take(Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public StringSegment Take(int n)
        {
            if (n > Count) throw new ArgumentOutOfRangeException();
            return new StringSegment(Value, Offset, n);
        }

        public StringSegment Skip(int n)
        {
            if (n > Count) throw new ArgumentOutOfRangeException();
            return new StringSegment(Value, Offset + n, Count - n);
        }

        public bool TrySearch(Func<Char, bool> pred, out int pos)
        {
            pos = 0;
            for (; pos < Count; ++pos)
            {
                if (pred(this[pos]))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public struct Node: IEnumerable<Node>
    {
        StringSegment m_segment;
        public StringSegment Segment
        {
            get { return m_segment; }
        }

        public int Start
        {
            get { return m_segment.Offset; }
        }

        public int End
        {
            get { return m_segment.Offset + m_segment.Count; }
        }

        public ValueType ValueType
        {
            get;
            private set;
        }

        static StringSegment SearchTokenEnd(StringSegment segment)
        {
            // search token end
            int i = 1;
            if (segment[0] == '"')
            {
                // string
                for (; i < segment.Count; ++i)
                {
                    if (segment[i] == '\"')
                    {
                        return segment.Take(i+1);
                    }
                    else if(segment[i] == '\\')
                    {
                        switch(segment[i+1])
                        {
                            case '"': // fall through
                            case '\\': // fall through
                            case '/': // fall through
                            case 'b': // fall through
                            case 'f': // fall through
                            case 'n': // fall through
                            case 'r': // fall through
                            case 't': // fall through
                                // skip next
                                i+=1;
                                break;

                            case 'u': // unicode
                                // skip next 4
                                i += 4;
                                break;

                            default:
                                // unkonw escape
                                throw new FormatException("unknown escape: "+segment.Skip(i));
                        }                         
                    }
                }
                throw new FormatException("no close string: " + segment.Skip(i));
            }
            else
            {
                // exclude string
                for (; i < segment.Count; ++i)
                {
                    if (Char.IsWhiteSpace(segment[i])
                        || segment[i] == '}' 
                        || segment[i] == ']'
                        )
                    {
                        break;
                    }
                }
                return segment.Take(i);
            }
        }

        Node(StringSegment segment)
        {
            switch (segment[0])
            {
                case '{': ValueType = ValueType.Object; break;
                case '[': ValueType = ValueType.Array; break;
                case '"': ValueType = ValueType.String; break;
                case 't': ValueType = ValueType.Boolean; break;
                case 'f': ValueType = ValueType.Boolean; break;
                case 'n': ValueType = ValueType.Unknown; break;

                case '-': // fall through
                case '0': // fall through
                case '1': // fall through
                case '2': // fall through
                case '3': // fall through
                case '4': // fall through
                case '5': // fall through
                case '6': // fall through
                case '7': // fall through
                case '8': // fall through
                case '9': // fall through
                    ValueType = ValueType.Number; break;

                default:
                    ValueType = ValueType.Unknown;
                    throw new FormatException(segment.ToString() + " is not json");
            }

            switch(ValueType)
            {
                case ValueType.Array: // fall through
                case ValueType.Object: // fall through
                    m_segment = segment;
                    break;

                default:
                    m_segment = SearchTokenEnd(segment);
                    break;
            }
        }

        public static Node Parse(string json)
        {
            return Parse(new StringSegment(json));
        }

        public static Node Parse(StringSegment json)
        {
            // search non whitespace
            int pos;
            if(!json.TrySearch(x => !Char.IsWhiteSpace(x), out pos))
            {
                throw new FormatException("[" + json.ToString() + "] is only whitespace");
            }
            return new Node(json.Skip(pos));
        }

        #region PrimitiveType
        public bool IsNull
        {
            get
            {
                return m_segment.IsMatch("null");
            }
        }

        public bool GetBoolean()
        {
            var s = m_segment.ToString();
            switch (s)
            {
                case "true": return true;
                case "false": return false;
                default: throw new FormatException(s + " is not boolean");
            }
        }

        public double GetNumber()
        {
            return double.Parse(m_segment.ToString());
        }
        #endregion

        #region StringType
        public string GetString()
        {
            return Unquote(m_segment.ToString());
        }

        public static string Quote(string src)
        {
            return '"' + src + '"';
        }

        public static string Unquote(string src)
        {
            return src.Substring(1, src.Length - 2);
        }

        #endregion

        #region CollectionType
        // for string key object
        public Node this[string target]
        {
            get
            {
                var it = GetEnumerator();
                while (it.MoveNext())
                {
                    var key = it.Current;

                    if (!it.MoveNext())
                    {
                        throw new FormatException("no value");
                    }
                    var value = it.Current;

                    if(key.GetString()==target)
                    {
                        return value;
                    }
                }
                throw new KeyNotFoundException();
            }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            bool isFirst = true;
            var current = m_segment.Skip(1);
            while (true)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    // search ','
                    int keyPos;
                    if (!current.TrySearch(x => x == ',', out keyPos))
                    {
                        break;
                    }
                    current = current.Skip(keyPos + 1);
                }

                // skip white space
                int nextToken;
                if (!current.TrySearch(x => !Char.IsWhiteSpace(x), out nextToken))
                {
                    throw new KeyNotFoundException("no key node");
                }
                current = current.Skip(nextToken);

                if (current[0] == '}')
                {
                    // closed
                    yield break;
                }

                // key
                var key = Parse(current);
                if (key.ValueType != ValueType.String)
                {
                    throw new FormatException("no string key is not allowed: " + key.Segment);
                }
                current = current.Skip(key.Segment.Count);
                yield return key;

                if (ValueType == ValueType.Object)
                {
                    // search ':'
                    int valuePos;
                    if (!current.TrySearch(x => x == ':', out valuePos))
                    {
                        throw new FormatException(": is not found");
                    }
                    current = current.Skip(valuePos + 1);

                    // value
                    var value = Parse(current);
                    current = current.Skip(value.Segment.Count);
                    yield return value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
