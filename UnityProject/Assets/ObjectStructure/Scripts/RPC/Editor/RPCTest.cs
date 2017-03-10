﻿using NUnit.Framework;
using ObjectStructure.Json;
using ObjectStructure.RPC;
using ObjectStructure.Serialization;

namespace ObjectStructureTest.RPC
{
    public class RpcTests
    {
        [Test]
        public void JsonRpcTest()
        {
            // setup
            var typeRegistory = new TypeRegistory();
            var method = typeRegistory.RPCFunc((int a, int b) => a + b);
            var dispatcher = new RPCDispatcher();
            dispatcher.AddMethod("Add", method);

            // request
            var request = "{\"jsonrpc\":\"2.0\",\"method\":\"Add\",\"params\":[1,2],\"id\":1}";
            var response = dispatcher.DispatchJsonRPC20(request).GetString();
            Assert.AreEqual("{\"jsonrpc\":\"2.0\",\"result\":3,\"id\":1}", response);
        }
    }
}
