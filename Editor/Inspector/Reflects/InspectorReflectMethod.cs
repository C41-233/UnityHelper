using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inspector
{

    internal class InspectorReflectMethodParameter
    {

        public InspectorReflectType Type { get; }
        
        public string Name { get; }
        
        public InspectorReflectMethodParameter(ParameterInfo parameterInfo)
        {
            Type = InspectorReflectType.Get(parameterInfo.ParameterType);
            Name = parameterInfo.Name;
        }
    }
    
    internal class InspectorReflectMethod
    {

        public string Name { get; }
        
        public string Descriptor { get; }

        public IReadOnlyList<InspectorReflectMethodParameter> Parameters => parameters;
        
        private readonly List<InspectorReflectMethodParameter> parameters;

        public InspectorReflectType ReturnType { get; }
        
        private readonly Func<object, object[], object> call;
        
        private MethodBase RuntimeMethod { get; }
        
        public InspectorReflectMethod(MethodInfo method)
        {
            Name = InspectorReflect.NormalizeName(method.Name);
            call = method.Invoke;
            RuntimeMethod = method;

            var runtimeParmaeters = method.GetParameters();
            parameters = new List<InspectorReflectMethodParameter>(runtimeParmaeters.Length);
            foreach (var p in runtimeParmaeters)
            {
                parameters.Add(new InspectorReflectMethodParameter(p));
            }
            
            ReturnType = InspectorReflectType.Get(method.ReturnType);
            
            var sb = new StringBuilder();
            sb.Append(ReturnType.TypeName);
            sb.Append(" ");
            sb.Append(Name);
            sb.Append('(');
            sb.Append(string.Join(", ", parameters.Select(p => p.Type.TypeName + " " + p.Name)));
            sb.Append(")");
            Descriptor = sb.ToString();
        }

        public object Call(object self, object[] args)
        {
            return call(self, args);
        }
        
    }
}