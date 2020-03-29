// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

/*
 */

namespace A2v10.ProcS
{

	public abstract class WorkflowStorageBase : IWorkflowStorage
	{
		private readonly IResourceWrapper rw;

		public WorkflowStorageBase(IResourceWrapper rw)
		{
			this.rw = rw;
		}

		public abstract Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity);

		protected IWorkflowDefinition WorkflowFromJson(String json)
		{
			var sett = new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto,
				ContractResolver = new WorkflowContractResolver()
			};
			sett.Converters.Add(new ActivityConverter(rw));
			return JsonConvert.DeserializeObject<StateMachine>(json, sett);
		}

		private readonly Lazy<Regex> rr = new Lazy<Regex>(() => new Regex("[\\r\\n\\t ]", RegexOptions.Compiled));

		protected Guid GetJsonHash(String json)
		{
			var s = rr.Value.Replace(json, String.Empty);
			using (System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create())
			{
				var h = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(s));
				return new Guid(h);
			}
		}
	}

	public class WorkflowContractResolver : DefaultContractResolver
	{
		
	}

	public class ActivityConverter : JsonConverter<IActivity>
	{
		private readonly IResourceWrapper rw;
		private readonly JsonConverter ecv;

		public ActivityConverter(IResourceWrapper rw)
		{
			this.rw = rw;
			ecv = new ExpandoObjectConverter();
		}

		public override IActivity ReadJson(JsonReader reader, Type objectType, IActivity existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var jo = JObject.Load(reader, new JsonLoadSettings());
			var k = jo.ContainsKey("$res") ? jo.Value<String>("$res") : (ProcS.ResName + ":" + nameof(CodeActivity));

			var act = rw.Create<IActivity>(k, new DynamicObject());

			serializer.Populate(new JTokenReader(jo), act);
			return act;
		}

		public override bool CanWrite => false;
		public override void WriteJson(JsonWriter writer, IActivity value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
