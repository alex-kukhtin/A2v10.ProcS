// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS.WebApi.Host.Classes
{
	public class FakeLogger : ILogger
	{
		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return false;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			
		}
	}
}
