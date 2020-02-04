// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{

	public class CallApiRequest : MessageBase<String>
	{
		public CallApiRequest() : base(null)
		{

		}

		public Guid Id { get; set; }
		public String Method { get; set; }
		public String Url { get; set; }
	}

	public class CallApiResponse : MessageBase<String>
	{
		public CallApiResponse(String correlationId) : base(correlationId)
		{
			
		}

		public String Result { get; set; }
	}


	public class CallHttpApiSaga : SagaBase<String>
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		// serializable
		private Guid _id;

		#region dispatch
		public override async Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case CallApiRequest apiRequest:
					await HandleRequest(context, apiRequest);
					break;
				case CallApiResponse apiResponse:;
					await HandleResponse(context, apiResponse);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}
		#endregion

		public async Task HandleRequest(IHandleContext context, CallApiRequest message)
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

		async Task<String> ExecuteGet(IHandleContext context, CallApiRequest message)
		{
			_id = message.Id;
			String correlationId = Guid.NewGuid().ToString();
			using (var response = await _httpClient.GetAsync(message.Url))
			{
				if (response.IsSuccessStatusCode)
				{
					var headers = response.Content.Headers;
					var contentType = headers.ContentType.MediaType;
					var charset = headers.ContentType.CharSet;

					var json = await response.Content.ReadAsStringAsync();

					var responseMessage = new CallApiResponse(correlationId) {
						Result = json
					};
					context.SendMessage(responseMessage);
				}
			}
			return correlationId;
		}

		public Task<String> HandleResponse(IHandleContext context, CallApiResponse message)
		{
			var resumeProcess = new ResumeProcess(_id, message.Result);
			context.SendMessage(resumeProcess);
			IsComplete = true;
			return Task.FromResult<String>(null);
		}

		Task<String> ExecutePost(IHandleContext context, CallApiRequest message)
		{
			throw new NotImplementedException(nameof(ExecutePost));
		}

		public static void Register()
		{
			InMemorySagaKeeper.RegisterMessageType<CallApiRequest, CallHttpApiSaga>();
			InMemorySagaKeeper.RegisterMessageType<CallApiResponse, CallHttpApiSaga>();
		}
	}
}
