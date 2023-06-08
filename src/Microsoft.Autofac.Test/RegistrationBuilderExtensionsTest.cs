using Autofac;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft.Autofac;

namespace Microsoft.Autofac.Test;

public class RegistrationBuilderExtensionsTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTests() { }

	#endregion

	#region Data

	class MyClass
	{
		public ILogger Logger { get; }

		public MyClass(ILogger logger)
		{
			this.Logger = logger;
		}
	}

	#endregion

	#region Tests

	[Test]
	public void UsingNamedLogger()
	{
		// Arrange
		var loggerName = "MyLogger";
		var logger = _fixture.Create<ILogger>();
		var namedLogger = _fixture.Create<ILogger>();
		var builder = new ContainerBuilder();
		builder.RegisterInstance(logger).As<ILogger>();
		builder.RegisterInstance(namedLogger).As<ILogger>().Named<ILogger>(loggerName);
		builder.RegisterType<MyClass>().WithLogger(loggerName).AsSelf();
		var container = builder.Build();

		// Act
		var service = container.Resolve<MyClass>();

		// Assert
		Assert.AreEqual(namedLogger, service.Logger);
	}

	[Test]
	public void UsingModifiedLogger()
	{
		// Arrange
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(logger).Setup(mock => mock.BeginScope(It.IsAny<object>())).Verifiable();
		var builder = new ContainerBuilder();
		builder.RegisterInstance(logger).As<ILogger>();
		builder.RegisterType<MyClass>().WithLogger(l => l.BeginScope(new object())).AsSelf();
		var container = builder.Build();

		// Act
		container.Resolve<MyClass>();

		// Assert
		Mock.Get(logger).Verify(mock => mock.BeginScope(It.IsAny<object>()), Times.Once);
	}

	[Test]
	public void UsingNamedAndModifiedLogger()
	{
		// Arrange
		var loggerName = "MyLogger";
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(logger).Setup(mock => mock.BeginScope(It.IsAny<object>())).Verifiable();
		var builder = new ContainerBuilder();
		builder.RegisterInstance(logger).As<ILogger>().Named<ILogger>(loggerName);
		builder.RegisterType<MyClass>().WithLogger(loggerName, l => l.BeginScope(new object())).AsSelf();
		var container = builder.Build();

		// Act
		var service = container.Resolve<MyClass>();

		// Assert
		Assert.AreEqual(logger, service.Logger);
		Mock.Get(logger).Verify(mock => mock.BeginScope(It.IsAny<object>()), Times.Once);
	}

	[Test]
	public void UsingSwappedLogger()
	{
		// Arrange
		var logger = _fixture.Create<ILogger>();
		var swappedLogger = _fixture.Create<ILogger>();
		var builder = new ContainerBuilder();
		builder.RegisterInstance(logger).As<ILogger>();
		builder.RegisterType<MyClass>().WithLogger(_ => swappedLogger).AsSelf();
		var container = builder.Build();

		// Act
		var service = container.Resolve<MyClass>();

		// Assert
		Assert.AreEqual(swappedLogger, service.Logger);
	}

	#endregion
}