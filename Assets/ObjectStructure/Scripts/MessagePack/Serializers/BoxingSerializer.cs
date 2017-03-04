using System;


namespace ObjectStructure.MessagePack.Serializers
{
    public class BoxingSerializer : SerializerBase<Object>
    {
        protected override void NonNullSerialize(MsgPackWriter w, object o)
        {
            var s = Serializer.GetSerializer(o.GetType());
            s.Serialize(w, o);
        }
    }
}
