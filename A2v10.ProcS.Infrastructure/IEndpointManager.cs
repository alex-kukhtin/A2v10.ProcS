using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IEndpointHandler
	{
		Task<(String body, String type)> HandleAsync(String body, String path);
	}

	public interface IEndpointHandlerFactory
	{
		IEndpointHandler CreateHandler();
	}

	public interface IEndpointManager
	{
		void RegisterEndpoint(String key, IEndpointHandler handler);
		void RegisterEndpoint(String key, IEndpointHandlerFactory factory);
	}

	public interface IEndpointResolver
	{
		IEndpointHandler GetHandler(String key);
	}
}
