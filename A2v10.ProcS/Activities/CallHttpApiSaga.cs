// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class CallApiRequestMessage : MessageBase<Guid>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallApiRequestMessage);

		[RestoreWith]
		public CallApiRequestMessage(Guid id) : base(id)
		{
		}

		public String Method { get; set; }
		public String Url { get; set; }

		public override void Store(IDynamicObject storage)
		{
			storage.Set(nameof(Method), Method);
			storage.Set(nameof(Url), Url);
		}

		public override void Restore(IDynamicObject store)
		{
			Method = store.Get<String>(nameof(Method));
			Url = store.Get<String>(nameof(Url));
		}

	}

	[ResourceKey(ukey)]
	public class CallApiResponseMessage : MessageBase<Guid>
	{
		public const string ukey = ProcS.ResName + ":" + nameof(CallApiResponseMessage);

		[RestoreWith] 
		public CallApiResponseMessage(Guid correlationId) : base(correlationId)
		{
			
		}

		public IDynamicObject Result { get; set; }
	}


	public class CallHttpApiSaga : SagaBaseDispatched<Guid, CallApiRequestMessage, CallApiResponseMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallHttpApiSaga);

		public CallHttpApiSaga() : base(ukey)
		{
		}

		private readonly HttpClient _httpClient = new HttpClient();


		protected override async Task Handle(IHandleContext context, CallApiRequestMessage message)
		{
			var method = message.Method?.Trim()?.ToLowerInvariant();
			if (String.IsNullOrEmpty(method))
				method = "get";
			switch (method)
			{
				case "get":
					CorrelationId.Value = await ExecuteGet(context, message);
					break;
				case "post":
					CorrelationId.Value = await ExecutePost(context, message);
					break;
				default:
					throw new ArgumentOutOfRangeException($"invalid method");
			}
		}

		async Task<Guid> ExecuteGet(IHandleContext context, CallApiRequestMessage message)
		{
			using (var response = await _httpClient.GetAsync(message.Url))
			{
				if (response.IsSuccessStatusCode)
				{
					var headers = response.Content.Headers;
					var contentType = headers.ContentType.MediaType;
					//var charset = headers.ContentType.CharSet;

					var json = await response.Content.ReadAsStringAsync();
					IDynamicObject result;
					if (contentType == "application/json")
					{
						result = DynamicObjectConverters.FromJson(json);
					} 
					else
					{
						throw new NotSupportedException($"'{contentType}' yet not supported");
					}

					var responseMessage = new CallApiResponseMessage(message.CorrelationId.Value) {
						Result = result
					};
					context.SendMessage(responseMessage);
				}
			}
			return message.CorrelationId.Value;
		}

		Task<Guid> ExecutePost(IHandleContext context, CallApiRequestMessage message)
		{
			throw new NotImplementedException(nameof(ExecutePost));
		}

		protected override Task Handle(IHandleContext context, CallApiResponseMessage message)
		{
			var continueMessage = new ContinueActivityMessage(message.CorrelationId.Value, "", message.Result);
			context.SendMessage(continueMessage);
			IsComplete = true;
			return Task.CompletedTask;
		}
	}
}
