// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.WebApi.Host
{
	public class WeatherForecast
	{
		public DateTime Date { get; set; }

		public Int32 TemperatureC { get; set; }

		public Int32 TemperatureF => 32 + (Int32)(TemperatureC / 0.5556);

		public String Summary { get; set; }
	}
}
