using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RimAI.Core.Source.Modules.Tooling.Execution
{
	internal static class ExecutorDiscovery
	{
		public static List<IToolExecutor> DiscoverExecutors()
		{
			var list = new List<IToolExecutor>();
			var asm = Assembly.GetExecutingAssembly();
			foreach (var t in asm.GetTypes())
			{
				if (!typeof(IToolExecutor).IsAssignableFrom(t) || t.IsAbstract || t.IsInterface) continue;
				try
				{
					if (Activator.CreateInstance(t) is IToolExecutor ex)
					{
						list.Add(ex);
					}
				}
				catch (global::System.Exception ex) { RimAI.Core.Source.Infrastructure.Diagnostics.ErrorPolicy.Ignore(ex, "General", "empty-catch-fallback"); }
			}
			return list.OrderBy(e => e.Name).ToList();
		}
	}
}
