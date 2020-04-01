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
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS.Tests.StoreRestore
{
	public class FakeResourceManager : IResourceManager
	{
		public Dictionary<String, KeyValuePair<String, Type>[]> TheList { get; } = new Dictionary<String, KeyValuePair<String, Type>[]>();

		private readonly KeyValuePair<String, Type>[] empty = Array.Empty<KeyValuePair<String, Type>>();

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
			var (c, prms) = ((ConstructorInfo c, ParameterInfo[] prms))fld.GetValue(factory);
			return prms.Select(p => new KeyValuePair<String, Type>(p.Name, p.ParameterType)).ToArray();
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

		public static Object PlainBullshitGenerator(Type type)
		{
			if (type.IsAssignableFrom(typeof(String))) return "Bullshit String";
			if (type.IsAssignableFrom(typeof(Int32))) return 777;
			if (type.IsAssignableFrom(typeof(Int64))) return 999;
			if (type.IsAssignableFrom(typeof(Single))) return (Single)333.33;
			if (type.IsAssignableFrom(typeof(Double))) return (Double)55555.5555;
			if (type.IsAssignableFrom(typeof(Guid))) return Guid.NewGuid();
			if (type.IsAssignableFrom(typeof(Boolean))) return true;
			if (type == typeof(Uri)) return new Uri("http://bullshit.io/");
			if (type.IsAssignableFrom(typeof(DateTime))) return new DateTime(2020, 1, 1);
			if (type.IsAssignableFrom(typeof(TimeSpan))) return new TimeSpan(0, 15, 0);
			if (type.IsEnum)
			{
				var vals = type.GetEnumNames();
				var i = vals.Length == 1 ? 0 : vals.Length -1;
				return Enum.Parse(type, vals[i]);
			}
			return null;
		}

		public static Object BullshitGenerator(Type type, IResourceWrapper rw, IDictionary<Type, Type> impl)
		{
			Object val = PlainBullshitGenerator(type);
			if (val == null)
			{
				var optype = impl.ContainsKey(type) ? impl[type] : type;
				if (optype != null)
				{
					if (!type.IsAssignableFrom(optype)) throw new Exception("Bad implementation");
					Boolean fill;
					var rk = optype.GetCustomAttribute<ResourceKeyAttribute>();
					if (rk != null)
					{
						val = rw.Create(rk.Key, new DynamicObject());
						fill = true;
					}
					else if (optype.IsGenericType && optype.GetGenericTypeDefinition() == typeof(CorrelationId<>))
					{
						var crdt = optype.GetGenericArguments()[0];
						val = Activator.CreateInstance(optype, PlainBullshitGenerator(crdt));
						fill = false;
					}
					else if (optype.IsArray)
					{
						var et = optype.GetElementType();
						var arr = Array.CreateInstance(et, 3);
						arr.SetValue(BullshitGenerator(et, rw, impl), 0);
						arr.SetValue(BullshitGenerator(et, rw, impl), 1);
						arr.SetValue(BullshitGenerator(et, rw, impl), 2);
						val = arr;
						fill = false;
					}
					else if (optype.IsGenericType && (optype.GetGenericTypeDefinition() == typeof(IEnumerable<>) || optype.GetInterface("IEnumerable`1") != null))
					{
						var gtd = optype.GetGenericTypeDefinition();
						if (gtd == typeof(IEnumerable<>))
						{
							var et = optype.GetGenericArguments()[0];
							var arr = Array.CreateInstance(et, 3);
							arr.SetValue(BullshitGenerator(et, rw, impl), 0);
							arr.SetValue(BullshitGenerator(et, rw, impl), 1);
							arr.SetValue(BullshitGenerator(et, rw, impl), 2);
							val = arr;
							fill = false;
						}
						else if (gtd == typeof(List<>) || gtd == typeof(IList<>))
						{
							var lt = optype.GetGenericArguments()[0];
							var t = typeof(List<>).MakeGenericType(lt);
							var l = Activator.CreateInstance(t) as System.Collections.IList;
							for (var i = 0; i < 3; i++)
							{
								l.Add(BullshitGenerator(lt, rw, impl));
							}
							val = l;
							fill = false;
						}
						else if (gtd == typeof(Dictionary<,>) || gtd == typeof(IDictionary<,>))
						{
							throw new Exception("Can't handle Dictionaries");
						}
						else
						{
							throw new Exception("Can't handle this Enumerable");
						}
					}
					else
					{
						var fct = new TypeResourceFactory(optype);
						var prms = FakeResourceManager.GetParams(fct);
						var data = new DynamicObject();
						foreach (var itm in prms)
						{
							data[itm.Key] = PlainBullshitGenerator(itm.Value);
						}
						val = fct.Create(data);
						fill = true;
					}
					if (fill) FillPropertiesBullshit(val, rw, impl);
				}
				else
				{
					//throw new Exception("Can't implement");
				}
			}
			return val;
		}

		public static void FillPropertiesBullshit(Object item, IResourceWrapper rw, IDictionary<Type, Type> impl)
		{
			var tt = item.GetType();
			var ppts = tt.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var ppt in ppts)
			{
				if (ppt.GetIndexParameters().Length > 0) continue;
				var sm = ppt.GetSetMethod(true);
				if (sm == null) continue;
				var val = BullshitGenerator(ppt.PropertyType, rw, impl);
				sm.Invoke(item, new Object[] { val });
			}
			var flds = tt.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var fld in flds)
			{
				if (fld.IsInitOnly) continue;
				var val = BullshitGenerator(fld.FieldType, rw, impl);
				fld.SetValue(item, val);
			}
		}

		public static Boolean Check(Object item)
		{
			if (item is IMessage)
				Assert.IsTrue(item is IStorable);
			if (item is ISaga)
				Assert.IsTrue(item is IStorable);
			return item is IStorable;
		}


		public static void StoreRestoreBullshit(IResourceWrapper rw, String key, IDynamicObject data, IDictionary<Type, Type> impl)
		{
			var item = rw.Create(key, data);
			if (!Check(item)) return;
			var storable = item as IStorable;

			FillPropertiesBullshit(item, rw, impl);

			var stored = storable.Store(rw);

			foreach (var d in data)
			{
				stored.Set(d.Key, d.Value);
			}

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

			foreach (var d in data)
			{
				stored.Set(d.Key, d.Value);
			}

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

			Assert.IsTrue(JToken.DeepEquals(JToken.Parse(storedJson), JToken.Parse(restoredJson)), $"Stored and Restoread are different for {key}");
		}

		class Dno : DynamicObject, IDynamicObject
		{
			public Guid DynamicGuid
			{
				get
				{
					return Get<Guid>("guid");
				}
				set
				{
					Set("guid", value);
				}
			}

			public String DynamicString
			{
				get
				{
					return Get<String>("string");
				}
				set
				{
					Set("string", value);
				}
			}

			public Int32 DynamicInt
			{
				get
				{
					return Get<Int32>("number");
				}
				set
				{
					Set("number", value);
				}
			}
		}

		public static void TestRegistred(IResourceWrapper rw, IEnumerable<KeyValuePair<String, KeyValuePair<String, Type>[]>> list, IEnumerable<KeyValuePair<Type, Type>> im = null)
		{
			var impl = new Dictionary<Type, Type>
			{
				{ typeof(IDynamicObject), typeof(Dno) },
				{ typeof(IActivity), typeof(CodeActivity) },
				{ typeof(IMessage), typeof(CallbackMessage) },
				{ typeof(IResultMessage), typeof(ContinueActivityMessage) }
			};

			if (im != null) foreach (var ii in im) impl.Add(ii.Key, ii.Value);

			foreach (var obj in list)
			{
				var cdata = new DynamicObject();
				foreach (var fld in obj.Value)
				{
					cdata[fld.Key] = PlainBullshitGenerator(fld.Value);
				}
				StoreRestoreEmpty(rw, obj.Key, cdata);
				StoreRestoreBullshit(rw, obj.Key, cdata, impl);
			}
		}

		[TestMethod]
		public void StoreRestore1()
		{
			var frm = new FakeResourceManager();
			var mgr2 = new SagaManager(null);

			var rm = new ResourceManager(null);
			var mgr = new SagaManager(null);
			//var pmr = new PluginManager(null);

			var configuration = new ConfigurationBuilder().Build();

			var storage = new FakeStorage(rm);
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage, configuration);

			//String pluginPath = GetPluginPath();

			//var configuration = new ConfigurationBuilder().Build();

			ProcS.RegisterSagas(rm, mgr, scriptEngine, repository);
			//ProcS.RegisterActivities(rm);

			ProcS.RegisterSagas(frm, mgr2, scriptEngine, repository);
			//ProcS.RegisterActivities(frm);

			//pmr.LoadPlugins(pluginPath, configuration);
			//pmr.RegisterResources(rm, mgr);

			

			TestRegistred(rm, frm.TheList);
		}
	}
}
