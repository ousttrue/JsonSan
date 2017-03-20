﻿using Osaru.MessagePack;
using Osaru.Serialization;
using Osaru.Serialization.Serializers;
using System;


namespace Osaru.RPC
{
    public interface IRPCResponseContext<T>
        where T : IParser<T>, new()
    {
        RPCRequest<T> Request { get; }
        RPCResponse<T> Response { get; }
        void Success();
        void Success<R>(R result, SerializerBase<R> s);
        void Error(Exception ex);
    }

    public class RPCResponseContext<T> : IRPCResponseContext<T>
        where T : IParser<T>, new()
    {
        RPCRequest<T> m_request;
        public RPCRequest<T> Request
        {
            get { return m_request; }
        }
        RPCResponse<T> m_response;
        public RPCResponse<T> Response
        {
            get { return m_response; }
        }
        IFormatter m_formatter;

        public RPCResponseContext(RPCRequest<T> request, IFormatter formatter)
        {
            m_request = request;
            m_formatter = formatter;
            m_response.Id = request.Id;
        }

        public void Error(Exception ex)
        {
            m_response.Error = ex.Message;
        }

        public void Success()
        {
            m_formatter.Null();
            m_response.ResultBytes = m_formatter.GetStore().Bytes;
        }

        public void Success<R>(R result, SerializerBase<R> s)
        {
            s.Serialize(result, m_formatter);
            m_response.ResultBytes = m_formatter.GetStore().Bytes;
        }
    }

    public interface IRPCMethod
    {
        void Call<T>(IRPCResponseContext<T> f)
            where T : IParser<T>, new()
            ;
    }
}
