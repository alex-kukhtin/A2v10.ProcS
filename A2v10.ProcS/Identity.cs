
using System;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public struct Identity : IIdentity
	{
		public String ProcessId { get; }
		public Int32 Version { get; }

		public Identity(String processId, Int32 version = 0)
		{
			ProcessId = processId ?? throw new ArgumentNullException(nameof(processId));
			Version = version;
		}

		public override Boolean Equals(Object obj)
		{
			if (obj is Identity identity)
				return Equals(identity);
			return false;
		}

		public override Int32 GetHashCode()
		{
			return (ProcessId + Version.ToString()).GetHashCode();
		}

		public static Boolean operator ==(Identity left, Identity right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(Identity left, Identity right)
		{
			return !(left == right);
		}
	}
}
