using NLedger.Extensibility.Python.Platform;
using NLedger.Tests;
using Python.Runtime;
using System;
using Xunit;

namespace NLedger.Extensibility.Python.Tests
{
    public class PythonFunctorTests : TestFixture
    {
        public PythonFunctorTests()
        {
            Assert.True(PythonConnector.Current.IsAvailable);
            PythonConnectionContext = PythonConnector.Current.Connect();
            PythonConnector.Current.KeepAlive = false;
        }

        public PythonConnectionContext PythonConnectionContext { get; }

        public override void Dispose()
        {
            PythonConnectionContext.Dispose();
            base.Dispose();
        }

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

        [PythonFact]
        public void PythonFunctor_ExprFunc_ReturnsValue()
        {
            using (var session = new PythonSession())
            {
                using (session.GIL())
                {
                    var converter = new PythonValueConverter(session);
                    var obj = PyObject.FromManagedObject("some-string");
                    var functor = new PythonFunctor("some-name", obj, converter);

                    var val = functor.ExprFunctor(session);

                    Assert.Equal("some-string", val.AsString);
                }
            }
        }

    }
}
