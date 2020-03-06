// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public class ProcessException : Exception
	{
		public ProcessException()
		{
		}

		public ProcessException(String message) : base(message)
		{
		}

		public ProcessException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}

}
