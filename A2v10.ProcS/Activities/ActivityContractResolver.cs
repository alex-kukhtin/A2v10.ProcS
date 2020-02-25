// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using Newtonsoft.Json.Serialization;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class ActivityContractResolver : DefaultContractResolver
	{
		public override JsonContract ResolveContract(Type type)
		{
			if (type == typeof(IActivity))
			{
				return base.ResolveContract(typeof(CodeActivity));
			}
			return base.ResolveContract(type);
		}
	}
}
