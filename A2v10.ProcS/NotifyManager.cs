// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class NotifyManager : INotifyManager
	{
		private readonly ConcurrentDictionary<Guid, Promise<String>> _dict = new ConcurrentDictionary<Guid, Promise<String>>();

		public void Register(Guid id, Promise<String> promise)
		{
			_dict.AddOrUpdate(id, promise, (k, v) => promise);
		}

		public Promise<String> GetAndRemove(Guid id)
		{
			if (_dict.TryRemove(id, out Promise<String> promise))
				return promise;
			return null;
		}

	}
}
