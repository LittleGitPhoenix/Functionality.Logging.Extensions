using System;
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
			LoggerGroupManager.Cache.Clear();
		}

		#endregion

		[Test]
		public void Check_Loggers_Are_Added()
		{
			// Arrange
			var loggers = _fixture.CreateMany<ILogger>(count: 6).ToArray();
			var firstUnevenLogger = loggers[1];
			var evenGroupIdentifier = _fixture.Create<string>();
			var unevenGroupIdentifier = _fixture.Create<int>();

			// Act
			for (var index = 0; index < loggers.Length; index++)
			{
				var logger = loggers[index];
				if (index == 5)
				{
					//! Last logger will be in both groups.
					LoggerGroupManager.AddLoggerToGroup(logger, evenGroupIdentifier);
					logger.AddToGroup(unevenGroupIdentifier);
				}
				else if (index % 2 == 0)
				{
					//! Even loggers will be in even-group.
					LoggerGroupManager.AddLoggerToGroup(logger, evenGroupIdentifier);
				}
				else
				{
					//! Un-even loggers will be in uneven-group.
					logger.AddToGroup(unevenGroupIdentifier);
				}
			}

			// Assert
			Assert.That(LoggerGroupManager.GetAllGroups(), Has.Length.EqualTo(2));
			Assert.That(LoggerGroupManager.GetAllLoggers(evenGroupIdentifier), Has.Length.EqualTo(4));
			Assert.That(firstUnevenLogger.AsGroup(unevenGroupIdentifier), Has.Length.EqualTo(3));
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

		/// <summary>
		/// Checks that <see cref="ILogger"/>s with the same group identifier share the same group.
		/// </summary>
		[Test]
		public void Check_Loggers_Share_Group()
		{
			// Arrange
			var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
			var groupIdentifier = _fixture.Create<string>();
			
			// Act
			foreach (var logger in loggers)
			{
				logger.AddToGroup(groupIdentifier);
			}
			
			// Assert
			Assert.That(LoggerGroupManager.GetAllGroups(), Has.Length.EqualTo(1));
			Assert.That(LoggerGroupManager.GetAllLoggers(groupIdentifier), Has.Length.EqualTo(loggers.Length));
		}

		/// <summary>
		/// Checks that <see cref="LoggerGroupManager.GetAllLoggers"/> does not throw if no loggers where found and instead returns an empty <see cref="ILogger"/> collection.
		/// </summary>
		[Test]
		public void Check_Get_Loggers_From_Group_Identifier_Returns_Empty()
		{
			// Arrange
			var groupIdentifier = Guid.NewGuid();

			// Act
			var loggers = LoggerGroupManager.GetAllLoggers(groupIdentifier);
			
			// Assert
			Assert.IsEmpty(loggers);
		}
		
		/// <summary>
		/// Checks that <see cref="LoggerGroupManager.GetAllGroups"/> does not throw if no groups where found and instead returns an empty collection.
		/// </summary>
		[Test]
		public void Check_Get_All_Groups_Returns_Empty()
		{
			// Arrange
			
			// Act
			var groups = LoggerGroupManager.GetAllGroups();
			
			// Assert
			Assert.IsEmpty(groups);
		}

		/// <summary>
		/// Checks that <see cref="LoggerGroupManager.GetAllGroups(Microsoft.Extensions.Logging.ILogger)"/> does not throw if no groups where found and instead returns an empty collection.
		/// </summary>
		[Test]
		public void Check_Get_All_Groups_For_Logger_Returns_Empty()
		{
			// Arrange
			var logger = _fixture.Create<ILogger>();
			
			// Act
			var groups = LoggerGroupManager.GetAllGroups(logger);
			
			// Assert
			Assert.IsEmpty(groups);
		}

		#region GroupIdentifier

		[Test]
		public void Check_GroupIdentifier_Is_Same_For_Same_Value_Type()
		{
			// Arrange
			var groupIdentifier = Guid.NewGuid();

			// Act
			var identifier1 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier);
			var identifier2 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier);
			
			// Assert
			Assert.AreEqual(identifier1, identifier2);
			Assert.AreNotSame(identifier1, identifier2);
		}

		[Test]
		public void Check_GroupIdentifier_Is_Same_For_Equal_Value_Type()
		{
			// Arrange
			var groupIdentifier1 = 10;
			var groupIdentifier2 = 10;

			// Act
			var identifier1 = new LoggerGroupManager.GroupIdentifier<int>(groupIdentifier1);
			var identifier2 = new LoggerGroupManager.GroupIdentifier<int>(groupIdentifier2);
			
			// Assert
			Assert.AreEqual(identifier1, identifier2);
			Assert.AreNotSame(identifier1, identifier2);
		}

		[Test]
		public void Check_GroupIdentifier_Is_Same_For_Same_Reference_Type()
		{
			// Arrange
			var groupIdentifier = new object();

			// Act
			var identifier1 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier);
			var identifier2 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier);
			
			// Assert
			Assert.AreEqual(identifier1, identifier2);
			Assert.AreNotSame(identifier1, identifier2);
		}

		[Test]
		public void Check_GroupIdentifier_Is_Same_For_Equal_Reference_Type()
		{
			// Arrange
			var groupIdentifier1 = "Equal";
			var groupIdentifier2 = "Equal";

			// Act
			var identifier1 = new LoggerGroupManager.GroupIdentifier<string>(groupIdentifier1);
			var identifier2 = new LoggerGroupManager.GroupIdentifier<string>(groupIdentifier2);
			
			// Assert
			Assert.AreEqual(identifier1, identifier2);
			Assert.AreNotSame(identifier1, identifier2);
		}

		[Test]
		public void Check_GroupIdentifier_Is_Not_Same_For_Value_Type()
		{
			// Arrange
			var groupIdentifier1 = Guid.NewGuid();
			var groupIdentifier2 = Guid.NewGuid();

			// Act
			var identifier1 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier1);
			var identifier2 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier2);

			// Assert
			Assert.AreNotEqual(identifier1, identifier2);
			Assert.AreNotSame(identifier1, identifier2);
		}

		[Test]
		public void Check_GroupIdentifier_Is_Not_Same_For_Reference_Type()
		{
			// Arrange
			var groupIdentifier1 = new object();
			var groupIdentifier2 = new object();

			// Act
			var identifier1 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier1);
			var identifier2 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier2);

			// Assert
			Assert.AreNotEqual(identifier1, identifier2);
			Assert.AreNotSame(identifier1, identifier2);
		}

		#endregion
	}
}