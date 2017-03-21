﻿using System;


namespace Osaru.Json
{
    public static class ArraySegmentExtensions
    {
        public static JsonParser ParseAsJson(this ArraySegment<Byte> bytes)
        {
            return JsonParser.Parse(bytes);
        }
        public static JsonParser ParseAsJson(this string src)
        {
            return JsonParser.Parse(src);
        }
    }
}
