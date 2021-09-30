using NLedger.Tests;
using Python.Runtime;
using System;
using Xunit;

namespace NLedger.Extensibility.Python.Tests
{
    public class PythonFunctorTests : TestFixture
    {
        [PythonFact]
        public void PythonFunctor_Constructor_PopulatesProperties()
        {
            using (var session = new PythonSession())
            {
                using (session.GIL())
                {
                    var converter = new PythonValueConverter(session);
                    var obj = PyObject.FromManagedObject("some-string");
                    var name = "some-name";

                    var functor = new PythonFunctor(name, obj, converter);

                    Assert.Equal(name, functor.Name);
                    Assert.Equal(obj.ToString(), functor.Obj.ToString());
                    Assert.Equal(converter, functor.PythonValueConverter);
                }
            }
                
        }

        public PythonFunctorTests() 
            : base()
        {
            Assert.True(PythonHostConnector.Current.IsInitialized);
        }

        public override void Dispose()
        {
            base.Dispose();
            PythonHostConnector.Reconnect(disposeCurrentConnection: true);
        }
    }
}
