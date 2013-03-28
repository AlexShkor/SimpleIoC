using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleIoC.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CanCreateContainer()
        {
            Assert.DoesNotThrow(()=>
            {
                var container = new Container();
            });
        }

        [Test]
        public void CanRegisterGenericLabmba()
        {
            var container = new Container();
            container.Register<ITest>(()=> new TestImp());
            var impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
        }

        [Test]
        public void CanRebindLabmba()
        {
            var container = new Container();
            container.Register<ITest>(()=> new TestImp());
            container.Register<ITest>(()=> new TestDependent(null));
            var impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
            Assert.AreEqual(typeof(TestDependent), impl.GetType());
        }

        [Test]
        public void CanRegisterGeneric()
        {
            var container = new Container();
            container.Register<ITest,TestImp>();
            var impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
        }

        [Test]
        public void CanResolveParametred()
        {
            var container = new Container();
            container.Register<ITestDependency,TestDependency>();
            container.Register<ITest, TestDependent>();
            var impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
        }

        [Test]
        public void CanRrebindGeneric()
        {
            var container = new Container();
            container.Register<ITestDependency,TestDependency>();
            container.Register<ITest, TestImp>();
            container.Register<ITest, TestDependent>();
            var impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
            Assert.AreEqual(typeof(TestDependent),impl.GetType());
        }

        [Test]
        public void CanResolveParametredTwice()
        {
            var container = new Container();
            container.Register<ITestDependency,TestDependency>();
            container.Register<ITest, TestDependent>();
            var impl = container.Resolve<ITest>();
            impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
        }

        [Test]
        public void CanResolveWithoutRegistration()
        {
            var container = new Container();
            var impl = container.Resolve<TestImp>();
            Assert.NotNull(impl);
        }

        [Test]
        public void CanResolveParametredWithoutRegistration()
        {
            var container = new Container();
            container.Register<ITestDependency, TestDependency>();
            var impl = container.Resolve<TestDependent>();
            Assert.NotNull(impl);
        }

        [Test]
        public void CanResolveSingleton()
        {
            var container = new Container();
            container.RegisterSingleton<ITest>(new TestImp());
            var impl = container.Resolve<ITest>();
            Assert.NotNull(impl);
        }

        //just lucky :)
        [Test]
        public void AsyncTest()
        {
            var container = new Container();
            container.Register<ITestDependency, TestDependency>();
            container.Register<ITest, TestDependent>();
            var bag = new ConcurrentBag<ITest>();
            for (int i = 0; i < 1000000; i++)
            {
                Task.Factory.StartNew(() => bag.Add(container.Resolve<ITest>()));
            }
            var set = new HashSet<int>();
            foreach (var item in bag)
            {
                set.Add(item.GetHashCode());
            }
            Assert.AreEqual(bag.Count, set.Count);
        }

        [Test]
        public void PerformanceTest()
        {
            var container = new Container();
            container.Register<ITest, TestImp>();
            var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < 1000000; i++)
                {
                    container.Resolve<ITest>();
                }
            stopwatch.Stop();
            Assert.Less(stopwatch.Elapsed.TotalMilliseconds,1000, "Elapsed" + Math.Round(stopwatch.Elapsed.TotalMilliseconds));
            Assert.Pass("Elapsed " + Math.Round(stopwatch.Elapsed.TotalMilliseconds));
        }
    }

    public class TestImp:ITest
    {
    }

    public interface ITest
    {
        
    }

    public class TestDependent : ITest
    {
        private readonly ITestDependency _dependency;

        public TestDependent(ITestDependency dependency)
        {
            _dependency = dependency;
        }
    }

    public interface ITestDependency
    {
    }

    public class TestDependency : ITestDependency
    {
    }
}
