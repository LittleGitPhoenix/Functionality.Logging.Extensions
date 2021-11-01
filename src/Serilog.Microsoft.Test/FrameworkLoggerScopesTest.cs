using System;
using System.Threading;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

namespace Serilog.Microsoft.Test
{
	public class FrameworkLoggerScopesTest
	{
#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
		private IFixture _fixture;
#pragma warning restore 8618

		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
		}

		[Test]
		public void Check_Duplicates_Are_Ignored()
		{
			// Assert
			Assert.Multiple
			(
				() =>
				{
					this.Check_Duplicates_Are_Ignored(ushort.MaxValue);
					this.Check_Duplicates_Are_Ignored(Guid.NewGuid());
					this.Check_Duplicates_Are_Ignored(Guid.NewGuid().ToString());
				}
			);
		}

		private void Check_Duplicates_Are_Ignored<TState>(TState state)
			where TState : notnull
		{
			// Arrange
			var scopes = new FrameworkLoggerScopes();

			// Act
			scopes.AddScope(state);
			scopes.AddScope(state);

			// Assert
			Assert.That(scopes, Has.Count.EqualTo(1));
		}

		[Test]
		public void Check_Removing_Duplicate_Removes_Original()
		{
			// Arrange
			var state = Guid.NewGuid().ToString();
			var scopes = new FrameworkLoggerScopes();
			scopes.AddScope(state);
			var disposable = scopes.AddScope(state);

			// Act
			disposable.Dispose();

			// Assert
			Assert.That(scopes, Has.Count.EqualTo(0));
		}

		[Test]
		public void Check_Removing_Is_Independent_Of_Order()
		{
			// Arrange
			var state = Guid.NewGuid().ToString();
			var scopes = new FrameworkLoggerScopes();
			scopes.AddScope(Guid.NewGuid().ToString());
			var disposable = scopes.AddScope(state);
			scopes.AddScope(Guid.NewGuid().ToString());
			scopes.AddScope(Guid.NewGuid().ToString());

			// Act
			disposable.Dispose();

			// Assert
			Assert.That(scopes, Does.Not.Contain(state));
			Assert.That(scopes, Is.Not.Empty);
		}
		
		[Test]
		public void Check_Removing_Multiple_Times_Does_Not_Throw_Exception()
		{
			// Arrange
			var state = Guid.NewGuid().ToString();
			var scopes = new FrameworkLoggerScopes();
			var disposable = scopes.AddScope(state);

			// Act + Assert
			Assert.DoesNotThrow
			(
				() =>
				{
					disposable.Dispose();
					disposable.Dispose();
					disposable.Dispose();
				}
			);
			Assert.That(scopes, Is.Empty);
		}
	}
}