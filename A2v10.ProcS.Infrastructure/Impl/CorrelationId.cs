// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public class CorrelationId<T> : ICorrelationId, IEquatable<CorrelationId<T>> where T : IEquatable<T>
	{
		public T Value { get; set; }

		/*public CorrelationId()
		{
			Value = default;
		}*/

		public CorrelationId(T value)
		{
			Value = value;
		}

		public Boolean Equals(ICorrelationId other)
		{
			if (other is CorrelationId<T> tt)
				return Equals(tt);
			return false;
		}

		public override Int32 GetHashCode()
		{
			return Value?.GetHashCode() ?? 0;
		}

		public Boolean Equals(CorrelationId<T> other)
		{
			if (Value == null) return other.Value == null;
			return Value.Equals(other.Value);
		}
	}

}
