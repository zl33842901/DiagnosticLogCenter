using AspectCore.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    internal class TracingDiagnosticMethod
    {
        private readonly ITracingDiagnosticProcessor _tracingDiagnosticProcessor;
        private readonly string _diagnosticName;
        private readonly IParameterResolver[] _parameterResolvers;
        private readonly MethodReflector _reflector;

        public TracingDiagnosticMethod(ITracingDiagnosticProcessor tracingDiagnosticProcessor, MethodInfo method,
            string diagnosticName)
        {
            _tracingDiagnosticProcessor = tracingDiagnosticProcessor;
            _reflector = method.GetReflector();
            _diagnosticName = diagnosticName;
            _parameterResolvers = GetParameterResolvers(method).ToArray();
        }

        public void Invoke(string diagnosticName, object value)
        {
            if (_diagnosticName != diagnosticName)
            {
                return;
            }

            var args = new object[_parameterResolvers.Length];
            for (var i = 0; i < _parameterResolvers.Length; i++)
            {
                args[i] = _parameterResolvers[i].Resolve(value);
            }

            _reflector.Invoke(_tracingDiagnosticProcessor, args);
        }

        private static IEnumerable<IParameterResolver> GetParameterResolvers(MethodInfo methodInfo)
        {
            foreach (var parameter in methodInfo.GetParameters())
            {
                var binder = parameter.GetCustomAttribute<ParameterBinder>();
                if (binder != null)
                {
                    if (binder is ObjectAttribute objectBinder)
                    {
                        if (objectBinder.TargetType == null)
                        {
                            objectBinder.TargetType = parameter.ParameterType;
                        }
                    }
                    if (binder is PropertyAttribute propertyBinder)
                    {
                        if (propertyBinder.Name == null)
                        {
                            propertyBinder.Name = parameter.Name;
                        }
                    }
                    yield return binder;
                }
                else
                {
                    yield return new NullParameterResolver();
                }
            }
        }
    }
}
