// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class Instance : IInstance
	{
		#region IInstance
		public Guid Id { get; set; }
		public Guid ParentInstanceId { get; set; }

		public Boolean IsComplete { get; set; }

		public IWorkflowDefinition Workflow { get; set; }

		public String CurrentState { get; set; }

		public void SetState(String state)
		{
			CurrentState = state;
		}

		public void SetParameters(IDynamicObject data)
		{
			if (data != null)
				Parameters = data;
		}

		public IDynamicObject GetParameters()
		{
			return Parameters;
		}

		public void SetEnvironment(IDynamicObject env)
		{
			if (env != null)
				Environment = env;
		}

		public IDynamicObject GetData()
		{
			return Data;
		}

		public IDynamicObject GetResult()
		{
			return Result;
		}

		public IDynamicObject GetEnvironment()
		{
			return Environment;
		}

		public IDynamicObject GetSelf()
		{
			var d = new DynamicObject
			{
				{ "Id", Id },
				{ "ParentId", ParentInstanceId },
				{ "CurrentState", CurrentState }
			};
			return d;
		}

		#endregion

		#region IStorable
		public IDynamicObject Store(IResourceWrapper wrapper)
		{
			var d = new DynamicObject();
			d.Set(nameof(Data), Data);
			d.Set(nameof(Parameters), Parameters);
			d.Set(nameof(Result), Result);
			d.Set(nameof(CurrentState), CurrentState);
			d.Set(nameof(IsComplete), IsComplete);
			return d;
		}

		public void Restore(IDynamicObject store, IResourceWrapper wrapper)
		{
			IsComplete = store.GetOrDefault<Boolean>(nameof(IsComplete));
			CurrentState = store.GetOrDefault<String>(nameof(CurrentState));
			Result.AssignFrom(nameof(Result), store);
			Parameters.AssignFrom(nameof(Parameters), store);
			Data.AssignFrom(nameof(Data), store);

		}
		#endregion


		public Instance()
		{
			Data = new DynamicObject();
			Parameters = new DynamicObject();
			Environment = new DynamicObject();
			Result = new DynamicObject();
		}

		public IDynamicObject Data { get; set; }
		public IDynamicObject Parameters { get; set; }
		public IDynamicObject Environment { get; set; }
		public IDynamicObject Result { get; set; }
	}
}
