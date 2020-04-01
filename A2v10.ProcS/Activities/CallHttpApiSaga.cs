// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class CallApiRequestMessage : MessageBase<Guid>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallApiRequestMessage);

		[RestoreWith]
		public CallApiRequestMessage(Guid correlationId) : base(correlationId)
		{
		}

		public String Method { get; set; }
		public String Url { get; set; }
		public ErrorMode HandleError { get; set; }
		public String Body { get; set; }

		public override void Store(IDynamicObject storage, IResourceWrapper _)
		{
			storage.Set(nameof(Method), Method);
			storage.Set(nameof(Url), Url);
			storage.Set(nameof(HandleError), HandleError);
			storage.Set(nameof(Body), Body);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			Method = store.Get<String>(nameof(Method));
			Url = store.Get<String>(nameof(Url));
			HandleError = store.Get<ErrorMode>(nameof(HandleError));
			Body = store.Get<String>(nameof(Body));
		}
	}

	[ResourceKey(ukey)]
	public class CallApiResponseMessage : MessageBase<Guid>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallApiResponseMessage);

		[RestoreWith] 
		public CallApiResponseMessage(Guid correlationId) : base(correlationId)
		{
			
		}

		public IDynamicObject Result { get; set; }

		public override void Store(IDynamicObject storage, IResourceWrapper _)
		{
			storage.Set("Result", Result);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			Result = store.GetDynamicObject("Result");
		}
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

		async Task ProcessResponse(IHandleContext context, Guid correlationId, HttpResponseMessage response)
		{
			var headers = response.Content.Headers;
			var contentType = headers.ContentType.MediaType;
			//var charset = headers.ContentType.CharSet;

			var json = await response.Content.ReadAsStringAsync();
			IDynamicObject result;
			if (contentType == "application/json")
				result = DynamicObjectConverters.FromJson(json);
			else
				throw new NotSupportedException($"'{contentType}' yet not supported");

			var responseMessage = new CallApiResponseMessage(correlationId)
			{
				Result = result
			};
			context.SendMessage(responseMessage);
		}

		async Task<Guid> ExecuteGet(IHandleContext context, CallApiRequestMessage message)
		{
			using (var response = await _httpClient.GetAsync(message.Url))
			{
				if (response.IsSuccessStatusCode)
					await ProcessResponse(context, message.CorrelationId.Value, response);
				else
				{
					if (message.HandleError == ErrorMode.Ignore)
						return message.CorrelationId.Value;
					// FAIL?
				}
			}
			return message.CorrelationId.Value;
		}

		async Task<Guid> ExecutePost(IHandleContext context, CallApiRequestMessage message)
		{
			var msg = new HttpRequestMessage()
			{
				Method = HttpMethod.Post,
				RequestUri = new Uri(message.Url)
			};
			
			if (!String.IsNullOrEmpty(message.Body))
				msg.Content = new StringContent(message.Body, Encoding.UTF8, "application/json");

			using (var response = await _httpClient.SendAsync(msg))
			{
				if (response.IsSuccessStatusCode)
					await ProcessResponse(context, message.CorrelationId.Value, response);
				else
				{
					if (message.HandleError == ErrorMode.Ignore)
						return message.CorrelationId.Value;
					// FAIL?
				}
			}
			return message.CorrelationId.Value;
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
