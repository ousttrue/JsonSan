##############################################################################
# RPCMethod.cs
##############################################################################
def rpcmethod_write_header(f):
    f.write('''
//
// this code is generated by cogegenerate.py
//
using Osaru.Serialization;
using Osaru.Serialization.Deserializers;
using Osaru.Serialization.Serializers;
using System;


namespace Osaru.RPC
{
    public interface IRPCMethod
    {
        void Call<T>(T args)
            where T : IParser<T>, new()
            ;
        void Call<T>(RPCContext<T> f)
            where T : IParser<T>, new()
            ;
    }

''')


def rpcmethod_write_actions(f, num):
    # action
    for i in range(num):
        if i==0:
            generic_params=""
        else:
            generic_params="<"+", ".join((f"A{x}" for x in range(i)))+">"

        deserializers="".join((
            f"        IDeserializerBase<A{x}> m_d{x};\n" for x in range(i)))
        delegate="public delegate void Method("+", ".join((
            f"A{x} a{x}" for x in range(i)))+");"
        get_deserializers="".join((
            f"            m_d{x} = r.GetDeserializer<A{x}>();\n" for x in range(i)))
        arg_vars="".join((
            f"                var a{x} = default(A{x});\n" for x in range(i)))
        deserializes="".join((
            f"                m_d{x}.Deserialize(args[{x}], ref a{x});\n" 
            for x in range(i)))
        invoke="m_method("+", ".join((f"a{x}" for x in range(i)))+");"

        f.write(f"""
    // Action{generic_params}
    public class RPCAction{generic_params} : IRPCMethod
    {{
{deserializers}
        {delegate}
        Method m_method;
        public RPCAction(TypeRegistry r, Method method)
        {{
            m_method = method;
{get_deserializers}
        }}

        public void Call<T>(T args)
            where T : IParser<T>, new()
        {{
{arg_vars}
{deserializes}

                {invoke}
        }}

        public void Call<T>(RPCContext<T> f)
            where T : IParser<T>, new()
        {{
            try
            {{
                Call(f.Request.Params);
                f.Success();
            }}
            catch (Exception ex)
            {{
                f.Error(ex);
            }}
        }}
    }}

""")


def rpcmethod_write_funcs(f, num):
    # func
    for i in range(num):
        generic_params="<"+"".join((f"A{x}, " for x in range(i)))+"R>"

        deserializers="".join((
            f"        IDeserializerBase<A{x}> m_d{x};\n" for x in range(i)))
        delegate="public delegate R Method("+", ".join((
            f"A{x} a{x}" for x in range(i)))+");"
        get_deserializers="".join((
            f"            m_d{x} = r.GetDeserializer<A{x}>();\n" for x in range(i)))
        arg_vars="".join((
            f"                var a{x} = default(A{x});\n" for x in range(i)))
        deserializes="".join((
            f"                m_d{x}.Deserialize(args[{x}], ref a{x});\n" 
            for x in range(i)))
        invoke="m_method("+", ".join((f"a{x}" for x in range(i)))+")"


        f.write(f"""
    // Func{generic_params}
    public class RPCFunc{generic_params} : IRPCMethod
    {{
{deserializers}
        {delegate}
        Method m_method;
        SerializerBase<R> m_s;
        public RPCFunc(TypeRegistry r, Method method)
        {{
            m_method = method;
{get_deserializers}
            m_s = r.GetSerializer<R>();
        }}

        [Obsolete("Use RPCAction.Call")]
        public void Call<T>(T args)
            where T : IParser<T>, new()
        {{
{arg_vars}
{deserializes}

                {invoke};
        }}

        public void Call<T>(RPCContext<T> f)
            where T : IParser<T>, new()
        {{
            try
            {{
                var args=f.Request.Params;

{arg_vars}
{deserializes}

                f.Success({invoke}, m_s);
            }}
            catch (Exception ex)
            {{
                f.Error(ex);
            }}
        }}
    }}
""")


##############################################################################
# TypeRegistryExtensions.cs
##############################################################################
def typeregistry_extensions_write_header(f):
    f.write("""
//
// this code is generated by cogegenerate.py
//
using System;
using Osaru.Serialization;


namespace Osaru.RPC
{
    public static class TypeRegistryExtensions
    {

""")


def typeregistry_extensions_write_actions(f, num):
    f.write("        #region Action")
    for i in range(num):

        if i==0:
            generic_params=""
        else:
            generic_params="<"+", ".join((f"A{x}" for x in range(i))) + ">"

        f.write(f"""
        public static RPCAction{generic_params} RPCAction{generic_params}(
            this TypeRegistry r, Action{generic_params} p)
        {{
            return new RPCAction{generic_params}(r
                , new RPCAction{generic_params}.Method(p));
        }}
""")
    f.write("        #endregion\n\n")


def typeregistry_extensions_write_funcs(f, num):
    f.write("        #region Func")
    for i in range(num):
        generic_params="<"+"".join((f"A{x}, " for x in range(i))) + "R>"

        f.write(f"""
        public static RPCFunc{generic_params} RPCFunc{generic_params}(
            this TypeRegistry r, Func{generic_params} p)
        {{
            return new RPCFunc{generic_params}(r
                , new RPCFunc{generic_params}.Method(p));
        }}
""")
    f.write("        #endregion\n")


def params_formatter(num):
    body=[]

    for i in range(1, num):
        generic_params='<' + ', '.join([f'A{x}' for x in range(i)])+ '>'
        args=', '.join([f'A{x} a{x}' for x in range(i)])
        serializers=''.join([f'            m_r.GetSerializer<A{x}>().Serialize(a{x}, f);\n' for x in range(i)])
        body.append(f'''
        public ArraySegment<Byte> Params{generic_params}({args})
        {{
            var f = new FORMATTER();
            f.BeginList({i});
{serializers}
            f.EndList();
            return f.GetStore().Bytes;
        }} 
''')
    return '\n'.join(body)


##############################################################################
# main
##############################################################################
if __name__ == "__main__":
    path="Assets/Osaru/Scripts/RPC/Dispatcher/RPCMethod.cs"
    with open(path, 'w') as f:
        rpcmethod_write_header(f)
        rpcmethod_write_actions(f, 5)
        rpcmethod_write_funcs(f, 5)
        f.write('}')

    path="Assets/Osaru/Scripts/RPC/Extensions/TypeRegistryExtensions.cs"
    with open(path, 'w') as f:
        typeregistry_extensions_write_header(f)
        typeregistry_extensions_write_actions(f, 5)
        typeregistry_extensions_write_funcs(f, 5)
        f.write('    }\n')
        f.write('}\n')

    path="Assets/Osaru/Scripts/RPC/RPCParamsFormatter.cs"
    with open(path, 'w') as f:

        body=params_formatter(5)

        f.write(f'''
//
// this code is generated by cogegenerate.py
//
using Osaru.Serialization;
using System;


namespace Osaru.RPC
{{
    public class RPCParamsFormatter<FORMATTER>
        where FORMATTER : IFormatter, new()
    {{
        TypeRegistry m_r;
        public RPCParamsFormatter()
        {{
            m_r = new TypeRegistry();
        }}

        public ArraySegment<Byte> Params()
        {{
            var f = new FORMATTER();
            f.BeginList(0);
            f.EndList();
            return f.GetStore().Bytes;
        }}

        {body}
    }}
}}
''')

