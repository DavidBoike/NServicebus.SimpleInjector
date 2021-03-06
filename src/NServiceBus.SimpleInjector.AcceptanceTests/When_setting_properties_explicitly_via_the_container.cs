﻿namespace NServiceBus.SimpleInjector.AcceptanceTests
{
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_setting_properties_explicitly_via_the_container : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_use_provide_values()
        {
            var simpleValue = "SomeValue";

            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>(b =>
                {
                    b.CustomConfig(c => c.RegisterComponents(r => r.ConfigureComponent(builder =>
                    {
                        return new Endpoint.MyMessageHandler(builder.Build<Context>())
                        {
                            MySimpleDependency = simpleValue
                        };
                    }, DependencyLifecycle.InstancePerCall)));
                    b.When((bus, c) => bus.SendLocal(new MyMessage()));
                })
                .Done(c => c.WasCalled)
                .Run();

            Assert.AreEqual(simpleValue, context.PropertyValue);
        }

        public class Context : ScenarioContext
        {
            public bool WasCalled { get; set; }
            public string PropertyValue { get; set; }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.SendFailedMessagesTo("error");
                });
            }

            public class MyMessageHandler : IHandleMessages<MyMessage>
            {
                public MyMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public string MySimpleDependency { get; set; }

                public Task Handle(MyMessage message, IMessageHandlerContext context)
                {
                    testContext.PropertyValue = MySimpleDependency;
                    testContext.WasCalled = true;
                    return Task.FromResult(0);
                }

                Context testContext;
            }

            public class MyPropDependency
            {
            }
        }

        public class MyMessage : ICommand
        {
        }
    }
}