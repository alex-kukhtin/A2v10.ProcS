// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IExecuteContext
	{
		IInstance Instance { get; }
		Task SaveInstance();

		void SendMessage(IMessage message);
	}
}
