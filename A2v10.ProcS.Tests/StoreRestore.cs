// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using A2v10.ProcS.Infrastructure;
using System.Linq;

using DynamicObject = A2v10.ProcS.Infrastructure.DynamicObject;
using Newtonsoft.Json.Linq;

namespace A2v10.ProcS.Tests
{
	public class FakeResourceManager : IResourceManager
	{
		public Dictionary<String, KeyValuePair<String, Type>[]> TheList { get; } = new Dictionary<String, KeyValuePair<String, Type>[]>();

		private readonly KeyValuePair<String, Type>[] empty = new KeyValuePair<String, Type>[0];

		public static String GetKey(Type type)
		{
			var att = type.GetCustomAttribute<ResourceKeyAttribute>();
			if (att == null) throw new Exception("Resource must have ResourceKeyAttribute");
			return att.Key;
		}

		public void RegisterResource(Type type)
		{
			TheList.Add(GetKey(type), GetParams(new TypeResourceFactory(type)));
		}

		public void RegisterResource<T>(String key) where T : new()
		{
			TheList.Add(key, GetParams(new TypeResourceFactory(typeof(T))));
		}

		public void RegisterResource<T>() where T : new()
		{
			RegisterResource<T>(GetKey(typeof(T)));
		}

		public void RegisterResources(IEnumerable<Type> types)
		{
			foreach (var t in types)
			{
				RegisterResource(t);
			}
		}

		public void RegisterResources(params Type[] types)
		{
			RegisterResources(types as IEnumerable<Type>);
		}

		public void RegisterResources<T1, T2>()
			where T1 : new()
			where T2 : new()
		{
			RegisterResource<T1>();
			RegisterResource<T2>();
		}

		public void RegisterResources<T1, T2, T3>()
			where T1 : new()
			where T2 : new()
			where T3 : new()
		{
			RegisterResource<T1>();
			RegisterResource<T2>();
			RegisterResource<T3>();
		}

		public void RegisterResources<T1, T2, T3, T4>()
			where T1 : new()
			where T2 : new()
			where T3 : new()
			where T4 : new()
		{
			RegisterResource<T1>();
			RegisterResource<T2>();
			RegisterResource<T3>();
			RegisterResource<T4>();
		}

		public static KeyValuePair<String, Type>[] GetParams(TypeResourceFactory factory)
		{
			var fld = factory.GetType().GetField("ct", BindingFlags.NonPublic | BindingFlags.Instance);
			var ddd = ((ConstructorInfo c, ParameterInfo[] prms))fld.GetValue(factory);
			return ddd.prms.Select(p => new KeyValuePair<String, Type>(p.Name, p.ParameterType)).ToArray();
		}

		public void RegisterResourceFactory(String key, IResourceFactory factory)
		{
			if (factory is TypeResourceFactory trsf)
			{
				TheList.Add(key, GetParams(trsf));
			}
			else
			{
				TheList.Add(key, empty);
			}
		}
	}


	[TestClass]
	public class StoreRestore
	{

		public static Object BullshitGenerator(Type type)
		{
			if (type.IsAssignableFrom(typeof(String))) return "Bullshit String";
			if (type.IsAssignableFrom(typeof(Int32))) return 777;
			if (type.IsAssignableFrom(typeof(Guid))) return Guid.NewGuid();
			if (type.IsAssignableFrom(typeof(Boolean))) return true;
			return null;
		}

		public static void FillPropertiesBullshit(Object item, IResourceWrapper rw)
		{
			var tt = item.GetType();
			var ppts = tt.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var ppt in ppts)
			{
				var sm = ppt.GetSetMethod(true);
				if (sm == null) continue;
				Object val = BullshitGenerator(ppt.PropertyType);
				if (val == null)
				{
					if (ppt.PropertyType.IsClass)
					{
						var rk = ppt.PropertyType.GetCustomAttribute<ResourceKeyAttribute>();
						if (rk != null)
						{
							val = rw.Create(rk.Key, new DynamicObject());
						}
						else if (ppt.PropertyType.IsGenericType && ppt.PropertyType.GetGenericTypeDefinition() == typeof(CorrelationId<>))
						{
							var crdt = ppt.PropertyType.GetGenericArguments()[0];
							val = Activator.CreateInstance(ppt.PropertyType, BullshitGenerator(crdt));
						}
						else
						{
							var fct = new TypeResourceFactory(ppt.PropertyType);
							var prms = FakeResourceManager.GetParams(fct);
							var data = new DynamicObject();
							foreach (var itm in prms)
							{
								data[itm.Key] = BullshitGenerator(itm.Value);
							}
							val = fct.Create(data);
						}
						FillPropertiesBullshit(val, rw);
					}
					else
					{
						continue;
					}
				}
				sm.Invoke(item, new Object[] { val });
			}
		}

		public static bool Check(object item)
		{
			Assert.IsTrue(item is IStorable);
			return true;
		}


		public static void StoreRestoreBullshit(IResourceWrapper rw, String key, IDynamicObject data)
		{
			var item = rw.Create(key, data);
			if (!Check(item)) return;
			var storable = item as IStorable;

			FillPropertiesBullshit(item, rw);

			var stored = storable.Store(rw);

			var rest = rw.Create(key, data);
			Assert.IsTrue(rest is IStorable);
			var restorable = rest as IStorable;

			restorable.Restore(stored, rw);

			var restored = restorable.Store(rw);

			Compare(key, stored, restored);
		}

		public static void StoreRestoreEmpty(IResourceWrapper rw, String key, IDynamicObject data)
		{
			var item = rw.Create(key, data);
			if (!Check(item)) return;
			var storable = item as IStorable;

			var stored = storable.Store(rw);

			var rest = rw.Create(key, data);
			Assert.IsTrue(rest is IStorable);
			var restorable = rest as IStorable;

			restorable.Restore(stored, rw);

			var restored = restorable.Store(rw);

			Compare(key, stored, restored);
		}

		public static void Compare(String key, IDynamicObject stored, IDynamicObject restored)
		{
			Assert.AreEqual(stored == null, restored == null);
			if (stored == null && restored == null) return;

			Assert.AreEqual(stored.IsEmpty, restored.IsEmpty);
			if (stored.IsEmpty && restored.IsEmpty) return;

			var storedJson = DynamicObjectConverters.ToJson(stored);
			var restoredJson = DynamicObjectConverters.ToJson(restored);

			Assert.IsTrue(JToken.DeepEquals(JToken.Parse(storedJson), JToken.Parse(restoredJson)));

			
		}

		[TestMethod]
		public void StoreRestore1()
		{
			var frm = new FakeResourceManager();
			var mgr2 = new SagaManager(null);

			var rm = new ResourceManager(null);
			var mgr = new SagaManager(null);
			//var pmr = new PluginManager(null);

			//String pluginPath = GetPluginPath();

			//var configuration = new ConfigurationBuilder().Build();

			ProcS.RegisterSagas(rm, mgr);
			//ProcS.RegisterActivities(rm);

			ProcS.RegisterSagas(frm, mgr2);
			//ProcS.RegisterActivities(frm);

			//pmr.LoadPlugins(pluginPath, configuration);
			//pmr.RegisterResources(rm, mgr);

			foreach (var obj in frm.TheList)
			{
				var data0 = new DynamicObject();
				var data = new DynamicObject();
				foreach (var fld in obj.Value)
				{
					data0[fld.Key] = fld.Value.IsValueType ? Activator.CreateInstance(fld.Value) : null;
					data[fld.Key] = BullshitGenerator(fld.Value);
				}
				StoreRestoreEmpty(rm, obj.Key, data0);
				StoreRestoreBullshit(rm, obj.Key, data);
			}
		}
	}
}
