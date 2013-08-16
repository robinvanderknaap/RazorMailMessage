using System;
using System.Linq;
using RazorEngine.Templating;

namespace RazorMailMessage.Activators
{
    public class Activator : IActivator
    {
        private readonly Func<Type, object> _resolver;

        public Activator(Func<Type, object> resolver)
        {
            _resolver = resolver;
        }

        public ITemplate CreateInstance(InstanceContext context)
        {
            var constructor = context.TemplateType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (constructor == null)
            {
                throw new InvalidOperationException("Base template does not contain constructor");
            }

            var arguments = constructor.GetParameters().Select(parameter => _resolver(parameter.ParameterType)).ToArray();

            return (ITemplate)constructor.Invoke(arguments);
        }
    }

    
}
