// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{

	public class CallApiRequest : IMessage
	{
		public Guid Id { get; set; }
		public String CorrelationId { get; set; }
		public String Method { get; set; }
		public String Url { get; set; }
	}

	public class CallApiResponse : IMessage
	{
		public String CorrelationId { get; set; }
		public String Result { get; set; }
	}


	public class CallHttpApiSaga : ISaga
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		public Boolean IsComplete { get; set; }

		// serializable
		private Guid _id;

		#region dispatch
		public Task<String> Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case CallApiRequest apiRequest:
					return HandleRequest(context, apiRequest);
				case CallApiResponse apiResponse:;
					return HandleResponse(context, apiResponse);
			}
			throw new ArgumentOutOfRangeException(message.GetType().FullName);
		}
		#endregion

		public Task<String> HandleRequest(IHandleContext context, CallApiRequest message)
		{
			var method = message.Method?.Trim()?.ToLowerInvariant();
			if (String.IsNullOrEmpty(method))
				method = "get";
			switch (method)
			{
				case "get":
					return ExecuteGet(context, message);
				case "post":
					return ExecutePost(context, message);
			}
			throw new ArgumentOutOfRangeException($"invalid method");
		}

		async Task<String> ExecuteGet(IHandleContext context, CallApiRequest message)
		{
			_id = message.Id;
			using (var response = await _httpClient.GetAsync(message.Url))
			{
				if (response.IsSuccessStatusCode)
				{
					var headers = response.Content.Headers;
					var contentType = headers.ContentType.MediaType;
					var charset = headers.ContentType.CharSet;

					var json = await response.Content.ReadAsStringAsync();

					var responseMessage = new CallApiResponse() {
						CorrelationId = "CorrelationId",
						Result = json
					};
					context.SendMessage(responseMessage);
				}
			}
			return "CorrelationId";
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
			ServiceBus.RegisterSaga<CallApiRequest, CallHttpApiSaga>();
			ServiceBus.RegisterSaga<CallApiResponse, CallHttpApiSaga>();
		}
	}
}
