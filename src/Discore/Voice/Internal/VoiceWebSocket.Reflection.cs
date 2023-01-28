using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Voice.Internal
{
    partial class VoiceWebSocket
    {
        delegate void PayloadSynchronousCallback(JsonElement payload, JsonElement data);
        delegate Task PayloadAsynchronousCallback(JsonElement payload, JsonElement data);

        class PayloadCallback
        {
            public PayloadSynchronousCallback? Synchronous { get; }
            public PayloadAsynchronousCallback? Asynchronous { get; }

            public PayloadCallback(PayloadSynchronousCallback synchronous)
            {
                Synchronous = synchronous;
            }

            public PayloadCallback(PayloadAsynchronousCallback asynchronous)
            {
                Asynchronous = asynchronous;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class PayloadAttribute : Attribute
        {
            public VoiceOPCode OPCode { get; }

            public PayloadAttribute(VoiceOPCode opCode)
            {
                OPCode = opCode;
            }
        }

        Dictionary<VoiceOPCode, PayloadCallback> InitializePayloadHandlers()
        {
            var payloadHandlers = new Dictionary<VoiceOPCode, PayloadCallback>();

            Type taskType = typeof(Task);
            Type webSocketType = typeof(VoiceWebSocket);
            Type payloadSynchronousType = typeof(PayloadSynchronousCallback);
            Type payloadAsynchronousType = typeof(PayloadAsynchronousCallback);


            foreach (MethodInfo method in webSocketType.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PayloadAttribute? attr = method.GetCustomAttribute<PayloadAttribute>();
                if (attr != null)
                {
                    PayloadCallback payloadCallback;
                    if (method.ReturnType == taskType)
                    {
                        Delegate callback = method.CreateDelegate(payloadAsynchronousType, this);
                        payloadCallback = new PayloadCallback((PayloadAsynchronousCallback)callback);
                    }
                    else
                    {
                        Delegate callback = method.CreateDelegate(payloadSynchronousType, this);
                        payloadCallback = new PayloadCallback((PayloadSynchronousCallback)callback);
                    }

                    payloadHandlers[attr.OPCode] = payloadCallback;
                }
            }

            return payloadHandlers;
        }
    }
}
