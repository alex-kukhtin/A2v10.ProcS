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
		public String Method { get; set; }
		public String Url { get; set; }
	}

	public class CallHttpApiSaga : SagaBase
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		public CallHttpApiSaga(Guid id, IServiceBus serviceBus, IInstanceStorage instanceStorage)
			: base(id, serviceBus, instanceStorage)
		{
		}

		#region dispatch
		public override Task Start(IMessage message)
		{
			switch (message)
			{
				case CallApiRequest apiRequest:
					return Start(apiRequest);
			}
			throw new ArgumentOutOfRangeException(message.GetType().FullName);
		}
		#endregion

		public Task Start(CallApiRequest message)
		{
			var method = message.Method?.Trim()?.ToLowerInvariant();
			if (String.IsNullOrEmpty(method))
				method = "get";
			switch (method)
			{
				case "get":
					return ExecuteGet(message);
				case "post":
					return ExecutePost(message);
			}
			throw new ArgumentOutOfRangeException($"invalid method");
		}

		async Task ExecuteGet(CallApiRequest message)
		{
			using (var response = await _httpClient.GetAsync(message.Url))
			{
				if (response.IsSuccessStatusCode)
				{
					var headers = response.Content.Headers;
					var contentType = headers.ContentType.MediaType;
					var charset = headers.ContentType.CharSet;

					var json = await response.Content.ReadAsStringAsync();
					IsComplete = true;

					var resumeProcess = new ResumeProcess(Id, json);
					ServiceBus.Send(resumeProcess);
				}
			}
		}

		Task ExecutePost(CallApiRequest message)
		{
			throw new NotImplementedException(nameof(ExecutePost));
		}

		public static void Register()
		{
			ProcS.ServiceBus.RegisterSaga<CallApiRequest, CallHttpApiSaga>();
		}
	}
}
