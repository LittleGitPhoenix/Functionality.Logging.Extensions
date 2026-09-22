using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class NoDisposableTest
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
	public void AfterAllTest() { }

	#endregion

	#region Data
	#endregion

	#region Tests

	[Test]
	public void InstanceIsNotNull()
	{
		// Arrange + Act
		var instance = NoDisposable.Instance;

		// Assert
		Assert.That(instance, Is.Not.Null);
	}

	[Test]
	public void InstanceReturnsSameSingletonInstance()
	{
		// Arrange + Act
		var first = NoDisposable.Instance;
		var second = NoDisposable.Instance;

		// Assert
		Assert.That(first, Is.SameAs(second));
	}

	[Test]
	public void DisposeDoesNotThrow()
	{
		// Arrange
		var instance = NoDisposable.Instance;

		// Act + Assert
		Assert.That(() => instance.Dispose(), Throws.Nothing);
	}

	[Test]
	public void InstanceImplementsIDisposable()
	{
		// Arrange + Act
		var instance = NoDisposable.Instance;

		// Assert
		Assert.That(instance, Is.InstanceOf<IDisposable>());
	}

	[Test]
	public void InstanceIsThreadSafe()
	{
		// Arrange
		var instances = new NoDisposable[100];

		// Act
		Parallel.For(0, instances.Length, i => instances[i] = NoDisposable.Instance);

		// Assert
		Assert.That(instances, Is.All.SameAs(instances[0]));
	}

	[Test]
	public void DisposeIsThreadSafe()
	{
		// Arrange
		var instance = NoDisposable.Instance;
		var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

		// Act
		Parallel.For(0, 100, i =>
		{
			try
			{
				instance.Dispose();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}
		});

		// Assert
		Assert.That(exceptions, Is.Empty);
	}

	#endregion
}