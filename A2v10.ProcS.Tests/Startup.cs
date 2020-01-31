using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	public class Startup
	{
		public static StateMachine Load(String fileName)
		{
			String json = File.ReadAllText($"..//..//..//Workflows//{fileName}");
			return JsonConvert.DeserializeObject<StateMachine>(json, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto
			});
		}
	}
}
