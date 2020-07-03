using AspectCore.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class PropertyAttribute : ParameterBinder
    {
        public string Name { get; set; }

        public override object Resolve(object value)
        {
            if (value == null || Name == null)
            {
                return null;
            }

            var property = value.GetType().GetProperty(Name);

            return property?.GetReflector()?.GetValue(value);
        }
    }
}
