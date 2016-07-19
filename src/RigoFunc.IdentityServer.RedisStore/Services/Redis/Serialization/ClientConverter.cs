﻿using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Newtonsoft.Json;

namespace RigoFunc.IdentityServer.Services.Redis.Serialization
{
    public class ClientConverter : JsonConverter
    {
        private readonly IClientStore _clientStore;

        public ClientConverter(IClientStore clientStore)
        {
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));
            _clientStore = clientStore;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var source = (Client)value;
            var target = new ClientLite { ClientId = source.ClientId };
            serializer.Serialize(writer, target);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = serializer.Deserialize<ClientLite>(reader);
            var factory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
            return factory.StartNew(async () => await _clientStore.FindClientByIdAsync(source.ClientId)).Unwrap().GetAwaiter().GetResult();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (Client) == objectType;
        }
    }
}