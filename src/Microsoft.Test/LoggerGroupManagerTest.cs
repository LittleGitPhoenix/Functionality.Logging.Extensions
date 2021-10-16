using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test
{
	public class LoggerGroupManagerTest
	{
		#region Data

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
		private IFixture _fixture;
#pragma warning restore 8618
		
		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
		}

		#endregion
		
		[Test]
		public void Check_Loggers_Are_Added()
		{
			// Arrange
			var loggers = _fixture.CreateMany<ILogger>(count: 6).ToArray();
			var firstUnevenLogger = loggers[1];
			var group0 = _fixture.Create<string>();
			var group1 = _fixture.Create<string>();

			// Act
			for (var index = 0; index < loggers.Length; index++)
			{
				var logger = loggers[index];
				if (index == 5)
				{
					//! Last logger will be in both groups.
					LoggerGroupManager.AddLoggerToGroup(logger, group0);
					logger.AddToGroup(group1);
				}
				else if (index % 2 == 0)
				{
					//! Even loggers will be in group 0.
					LoggerGroupManager.AddLoggerToGroup(logger, group0);
				}
				else
				{
					//! Un-Even loggers will be in group 1.
					logger.AddToGroup(group1);
				}
			}

			// Assert
			Assert.That(LoggerGroupManager.GetAllGroups(), Has.Length.EqualTo(2));
			Assert.That(LoggerGroupManager.GetAllLoggers(group0), Has.Length.EqualTo(4));
			Assert.That(firstUnevenLogger.AsGroup(group1), Has.Length.EqualTo(3));
			for (var index = 0; index < loggers.Length; index++)
			{
				var logger = loggers[index];
				if (index == 5)
				{
					Assert.That(LoggerGroupManager.GetAllGroups(logger), Has.Length.EqualTo(2));
				}
				else
				{
					Assert.That(logger.GetGroups(), Has.Length.EqualTo(1));
				}
			}
		}
	}
}