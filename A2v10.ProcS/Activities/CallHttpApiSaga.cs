// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class CallApiRequestMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallApiRequestMessage);

		[RestoreWith]
		public CallApiRequestMessage() : base(null)
		{

		}

		public Guid Id { get; set; }
		public String Method { get; set; }
		public String Url { get; set; }

	}

	[ResourceKey(ukey)]
	public class CallApiResponseMessage : MessageBase<String>
	{
		public const string ukey = ProcS.ResName + ":" + nameof(CallApiResponseMessage);

		[RestoreWith] 
		public CallApiResponseMessage(String correlationId) : base(correlationId)
		{
			
		}

		public IDynamicObject Result { get; set; }
	}


	public class CallHttpApiSaga : SagaBaseDispatched<String, CallApiRequestMessage, CallApiResponseMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallHttpApiSaga);

		public CallHttpApiSaga() : base(ukey)
		{
		}

		private readonly HttpClient _httpClient = new HttpClient();

		// serializable
		private Guid _id;

		public override IDynamicObject Store()
		{
			var d = new DynamicObject();
			d.Set("Id", _id);
			return d;
		}

		public override void Restore(IDynamicObject store)
		{
			_id = store.Get<Guid>("Id");
		}

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

		async Task<String> ExecuteGet(IHandleContext context, CallApiRequestMessage message)
		{
			_id = message.Id;
			String correlationId = Guid.NewGuid().ToString();

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

					var responseMessage = new CallApiResponseMessage(correlationId) {
						Result = result
					};
					context.SendMessage(responseMessage);
				}
			}
			return correlationId;
		}

		Task<String> ExecutePost(IHandleContext context, CallApiRequestMessage message)
		{
			throw new NotImplementedException(nameof(ExecutePost));
		}

		protected override Task Handle(IHandleContext context, CallApiResponseMessage message)
		{
			var continueMessage = new ContinueActivityMessage(_id, "", message.Result);
			context.SendMessage(continueMessage);
			IsComplete = true;
			return Task.CompletedTask;
		}
	}
}
