using System.Collections;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class LogScopeManagerTest
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

	/// <summary> Test implementation of IExecutionContextAwareLogScope </summary>
	private class TestExecutionContextAwareScope : IExecutionContextAwareLogScope
	{
		IDictionary<string, object?> _underlyingData;

		public string Name { get; }

		public TestExecutionContextAwareScope(string name)
		{
			_underlyingData = new Dictionary<string, object?>();
			_underlyingData["name"] = name;
			this.Name = name;
		}

		public override bool Equals(object? obj)
		{
			if (obj is TestExecutionContextAwareScope other) return this.Name == other.Name;
			return false;
		}

		public override int GetHashCode() => this.Name.GetHashCode();

		/// <inheritdoc />
		public override string ToString() => this.Name;

		#region IDictionary<string, object?> Implementation

		public object? this[string key]
		{
			get => _underlyingData[key];
			set => _underlyingData[key] = value;
		}

		public ICollection<string> Keys => _underlyingData.Keys;

		public ICollection<object?> Values => _underlyingData.Values;

		public int Count => _underlyingData.Count;

		public bool IsReadOnly => _underlyingData.IsReadOnly;

		public void Add(string key, object? value) => _underlyingData.Add(key, value);

		public void Add(KeyValuePair<string, object?> item) => _underlyingData.Add(item);

		public void Clear() => _underlyingData.Clear();

		public bool Contains(KeyValuePair<string, object?> item) => _underlyingData.Contains(item);

		public bool ContainsKey(string key) => _underlyingData.ContainsKey(key);

		public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => _underlyingData.CopyTo(array, arrayIndex);

		public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _underlyingData.GetEnumerator();

		public bool Remove(string key) => _underlyingData.Remove(key);

		public bool Remove(KeyValuePair<string, object?> item) => _underlyingData.Remove(item);

		public bool TryGetValue(string key, out object? value) => _underlyingData.TryGetValue(key, out value);
		
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		#endregion
	}

	#endregion

	#region Tests

	#region Adding/removing scopes

	/// <summary>
	/// Verifies that adding duplicate scopes to <see cref="LogScopeManager"/> is properly handled by ignoring the duplicate.
	/// </summary>
	[Test]
    public void DuplicatesAreIgnored()
    {
        // Assert
        Assert.Multiple
        (
            () =>
            {
                this.DuplicatesAreIgnored(ushort.MaxValue);
                this.DuplicatesAreIgnored(Guid.NewGuid());
                this.DuplicatesAreIgnored(Guid.NewGuid().ToString());
                this.DuplicatesAreIgnored(new TestExecutionContextAwareScope(_fixture.Create<String>()));
            }
        );
    }

    private void DuplicatesAreIgnored<TState>(TState state)
        where TState : notnull
    {
        // Arrange
        var scopes = new LogScopeManager();

        // Act
        scopes.AddScope(state);
        scopes.AddScope(state);

		// Assert
		var scopeValues = scopes.GetScopeValues().ToArray();
        Assert.That(scopeValues, Has.Length.EqualTo(1));
    }

	/// <summary>
	/// Verifies that disposing a duplicate scope removes the original scope from the collection.
	/// </summary>
	/// <remarks> When the same scope is added twice, both references point to the same underlying scope. Disposing either reference should remove the scope entirely from the collection. </remarks>
	[Test]
    public void RemovingDuplicateRemovesOriginal()
    {
        // Arrange
        var state = Guid.NewGuid().ToString();
        var scopes = new LogScopeManager();
        scopes.AddScope(state);
        var disposable = scopes.AddScope(state);

        // Act
        disposable.Dispose();

		// Assert
		var scopeValues = scopes.GetScopeValues().ToArray();
		Assert.That(scopeValues, Has.Length.EqualTo(0));
    }

	/// <summary>
	/// Verifies that removing a scope from the middle of the collection doesn't affect other scopes.
	/// </summary>
	[Test]
    public void RemovingIsIndependentOfOrder()
    {
        // Arrange
        var state = Guid.NewGuid().ToString();
        var scopes = new LogScopeManager();
        scopes.AddScope(Guid.NewGuid().ToString());
        var disposable = scopes.AddScope(state);
        scopes.AddScope(Guid.NewGuid().ToString());
        scopes.AddScope(Guid.NewGuid().ToString());

        // Act
        disposable.Dispose();

		// Assert
		var scopeValues = scopes.GetScopeValues().ToArray();
		Assert.That(scopeValues, Does.Not.Contain(state));
        Assert.That(scopeValues, Is.Not.Empty);
    }

	/// <summary>
	/// Verifies that calling Dispose multiple times on the same scope disposable does not throw an exception.
	/// </summary>
	[Test]
    public void RemovingMultipleTimesDoesNotThrowException()
    {
        // Arrange
        var state = Guid.NewGuid().ToString();
        var scopes = new LogScopeManager();
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
		var scopeValues = scopes.GetScopeValues().ToArray();
		Assert.That(scopeValues, Is.Empty);
	}

	#endregion
	
	#region Execution Context Aware Scopes

	/// <summary>
	/// Verifies that execution context-aware scopes are properly isolated between different tasks and don't leak across execution contexts.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Tests the core functionality of <see cref="AsyncLocal{T}"/> in <see cref="LogScopeManager"/> by ensuring:
	/// </para>
	/// <list type="bullet">
	/// <item><description> Regular scopes are visible to all execution contexts (main thread and tasks). </description></item>
	/// <item><description> Execution context-aware scopes are only visible within their own execution context. </description></item>
	/// <item><description> Task 1 cannot see Task 2's execution context-aware scope and vice versa. </description></item>
	/// <item><description> The main thread cannot see any task's execution context-aware scopes. </description></item>
	/// </list>
	/// <para>
	/// Uses <see cref="SemaphoreSlim"/> as a synchronization barrier to ensure both tasks execute in parallel and reach the same point before collecting results, providing deterministic test behavior.
	/// </para>
	/// </remarks>
	[Test]
	public async Task ExecutionContextAwareScopesAreRespected()
	{
		// Arrange		
		var scopes = new LogScopeManager();
		
		// Create a simple test scope that implements IExecutionContextAwareLogScope.
		var regularScope = "RegularScope";
		var executionContextAwareScope1 = new TestExecutionContextAwareScope("Task1-Scope");
		var executionContextAwareScope2 = new TestExecutionContextAwareScope("Task2-Scope");

		// Results collectors.
		var mainThreadResult = new List<object>();
		var task1Result = new List<object>();
		var task2Result = new List<object>();
		
		// Synchronization to ensure both tasks reach the same point before collecting results.
		var barrier = new SemaphoreSlim(0, 2);
		
		// Act
		// Add a regular scope in the main context.
		scopes.AddScope(regularScope);

		// Task 1: Add execution context-aware scope and collect results.
		var task1 = Task.Run
		(
			async () =>
			{
				scopes.AddScope(executionContextAwareScope1);

				// Signal that this task is ready.
				barrier.Release();
				
				// Wait for the other task to also be ready.
				await barrier.WaitAsync();
								
				// Extract the actual scope objects
				foreach (var scope in scopes.GetScopeValues())
				{
					task1Result.Add(scope);
				}
			}
		);

		// Task 2: Add a different execution context-aware scope and collect results.
		var task2 = Task.Run
		(
			async () =>
			{
				scopes.AddScope(executionContextAwareScope2);
			
				// Signal that this task is ready.
				barrier.Release();
				
				// Wait for the other task to also be ready.
				await barrier.WaitAsync();

				// Extract the actual scope objects
				foreach (var scope in scopes.GetScopeValues())
				{
					task2Result.Add(scope);
				}
			}
		);

		// Wait for both tasks to complete.
		await Task.WhenAll(task1, task2);

		// Collect results from main thread.
		foreach (var scope in scopes.GetScopeValues())
		{
			mainThreadResult.Add(scope);
		}

		// Assert
		Assert.Multiple
		(
			() =>
			{
				// Main thread should only see the regular scope (no execution context-aware scopes from tasks).
				Assert.That(mainThreadResult, Does.Contain(regularScope), "Main thread should see the regular scope");
				Assert.That(mainThreadResult, Does.Not.Contain(executionContextAwareScope1), "Main thread should NOT see Task 1's execution context-aware scope");
				Assert.That(mainThreadResult, Does.Not.Contain(executionContextAwareScope2), "Main thread should NOT see Task 2's execution context-aware scope");
			
				// Task 1 should only see its own execution context-aware scope + regular scope.
				Assert.That(task1Result, Does.Contain(executionContextAwareScope1), "Task 1 should see its own execution context-aware scope");
				Assert.That(task1Result, Does.Contain(regularScope), "Task 1 should see the regular scope");
				Assert.That(task1Result, Does.Not.Contain(executionContextAwareScope2), "Task 1 should NOT see Task 2's execution context-aware scope");

				// Task 2 should only see its own execution context-aware scope + regular scope.
				Assert.That(task2Result, Does.Contain(executionContextAwareScope2), "Task 2 should see its own execution context-aware scope");
				Assert.That(task2Result, Does.Contain(regularScope), "Task 2 should see the regular scope");
				Assert.That(task2Result, Does.Not.Contain(executionContextAwareScope1), "Task 2 should NOT see Task 1's execution context-aware scope");
			}
		);
	}

	/// <summary>
	/// Verifies that execution context-aware scopes flow correctly from parent tasks to child tasks.
	/// </summary>
	[Test]
	public async Task ExecutionContextAwareScopesFlowToChildTasks()
	{
		// Arrange
		var scopes = new LogScopeManager();
		var parentScope = new TestExecutionContextAwareScope("Parent-Scope");
		var childScope = new TestExecutionContextAwareScope("Child-Scope");
		
		var parentResult = new List<object>();
		var childResult = new List<object>();
		
		// Act
		var parentTask = Task.Run
		(
			async () =>
			{
				scopes.AddScope(parentScope);
				parentResult.AddRange(scopes.GetScopeValues());
				
				// Child task should inherit parent's execution context.
				await Task.Run
				(
					() =>
					{
						scopes.AddScope(childScope);
						childResult.AddRange(scopes.GetScopeValues());
					}
				);
			}
		);
		
		await parentTask;
		
		// Assert
		Assert.Multiple
		(
			() =>
			{
				Assert.That(parentResult, Does.Contain(parentScope), "Parent should see its scope");
				Assert.That(childResult, Does.Contain(parentScope), "Child should see parent's scope");
				Assert.That(childResult, Does.Contain(childScope), "Child should see its own scope");
			}
		);
	}

	/// <summary>
	/// Verifies that the insertion order of execution context-aware scopes is preserved when retrieved.
	/// </summary>
	[Test]
	public async Task ExecutionContextAwareScopeOrderIsPreserved()
	{
		// Arrange
		var scopes = new LogScopeManager();
		var scope1 = new TestExecutionContextAwareScope("First");
		var scope2 = new TestExecutionContextAwareScope("Second");
		var scope3 = new TestExecutionContextAwareScope("Third");

		var result = new List<object>();

		// Act
		await Task.Run
		(
			() =>
			{
				scopes.AddScope(scope1);
				scopes.AddScope(scope2);
				scopes.AddScope(scope3);

				result.AddRange(scopes.GetScopeValues());
			}
		);

		// Assert
		var executionContextScopes = result.OfType<TestExecutionContextAwareScope>().ToList();
		Assert.Multiple
		(
			() =>
			{
				Assert.That(executionContextScopes[0], Is.EqualTo(scope1), "First scope should be in first position");
				Assert.That(executionContextScopes[1], Is.EqualTo(scope2), "Second scope should be in second position");
				Assert.That(executionContextScopes[2], Is.EqualTo(scope3), "Third scope should be in third position");
			}
		);
	}

	/// <summary>
	/// Verifies that regular scopes and execution context-aware scopes can be mixed and that scopes are still returned in the order they where added.
	/// </summary>
	[Test]
	public async Task MixingRegularAndExecutionContextAwareScopes()
	{
		// Arrange
		var scopes = new LogScopeManager();
		var regularScope1 = "Regular-1";
		var regularScope2 = "Regular-2";
		var execScope = new TestExecutionContextAwareScope("Exec-Scope");

		var result = new List<object>();

		// Act
		scopes.AddScope(regularScope1);

		await Task.Run
		(
			() =>
			{
				scopes.AddScope(execScope);
				scopes.AddScope(regularScope2);

				result.AddRange(scopes.GetScopeValues());
			}
		);

		// Assert
		Assert.Multiple
		(
			() =>
			{
				// Execution context scopes should be returned first.
				Assert.That(result, Has.Count.EqualTo(3), "Scope should contain three elements.");
				Assert.That(result[0], Is.EqualTo(regularScope1), "Regular scope 1 must be the first element in the scope.");
				Assert.That(result[1], Is.EqualTo(execScope), "Execution context scope must be the second element in the scope.");
				Assert.That(result[2], Is.EqualTo(regularScope2), "Regular scope 2 must be the third and last element in the scope.");
			}
		);
	}

	
	[Test]
	public async Task MultipleLoggersShareScopeWhenInSameExecutionContext()
	{
		
	}

	/// <summary>
	/// Verifies that multiple execution context-aware scopes can coexist within the same execution context.
	/// </summary>
	/// <remarks>
	/// Tests that the internal <see cref="AsyncLocal{T}"/> collection in <see cref="LogScopeManager"/> can hold multiple execution context-aware scopes simultaneously without conflicts.
	/// This is important for scenarios where multiple layers of an application add their own execution context-aware logging scopes (e.g., request ID, user ID, correlation ID).
	/// </remarks>
	[Test]
	public async Task MultipleExecutionContextAwareScopesInSameTask()
	{
		// Arrange
		var scopes = new LogScopeManager();
		var scope1 = new TestExecutionContextAwareScope("Scope-1");
		var scope2 = new TestExecutionContextAwareScope("Scope-2");
		var scope3 = new TestExecutionContextAwareScope("Scope-3");
		
		var result = new List<object>();
		
		// Act
		await Task.Run
		(
			() =>
			{
				scopes.AddScope(scope1);
				scopes.AddScope(scope2);
				scopes.AddScope(scope3);
				
				result.AddRange(scopes.GetScopeValues());
			}
		);
		
		// Assert
		Assert.Multiple
		(
			() =>
			{
				Assert.That(result, Has.Count.EqualTo(3), "Should have all 3 scopes.");
				Assert.That(result, Does.Contain(scope1), "Should contain scope 1.");
				Assert.That(result, Does.Contain(scope2), "Should contain scope 2.");
				Assert.That(result, Does.Contain(scope3), "Should contain scope 3.");
			}
		);
	}

	///// <summary>
	///// Verifies that <see cref="LogScopeManager.CreateLogEventPropertyValues"/> respects execution context boundaries.
	///// </summary>
	///// <remarks>
	///// <para>
	///// This is a critical test that directly validates the <see cref="LogScopeManager.CreateLogEventPropertyValues"/> method that is used by serilog to actually get the scope via the <see cref="FrameworkLoggerEnricher"/>.
	///// It ensures:
	///// </para>
	///// <list type="bullet">
	///// <item><description> Each task processes only its own execution context-aware scope. </description></item>
	///// <item><description> Both tasks process the shared regular scope. </description></item>
	///// <item><description> Property creation is tracked via a thread-safe <see cref="System.Collections.Concurrent.ConcurrentBag{T}"/>. </description></item>
	///// </list>
	///// <para>
	///// Uses <see cref="SemaphoreSlim"/> to synchronize both tasks and ensure they call <see cref="LogScopeManager.CreateLogEventPropertyValues"/> in parallel.
	///// </para>
	///// </remarks>
	//[Test]
	//public async Task CreateLogEventPropertyValuesRespectsExecutionContext()
	//{
	//	// Arrange
	//	var scopes = new LogScopeManager();
		
	//	var mockPropertyFactory = new Mock<ILogEventPropertyFactory>();
	//	var propertiesCreated = new System.Collections.Concurrent.ConcurrentBag<string>();
		
	//	mockPropertyFactory
	//		.Setup(mock => mock.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
	//		.Returns
	//		(
	//			(string name, object value, bool destructure) =>
	//			{
	//				propertiesCreated.Add(value?.ToString() ?? "null");
	//				return new LogEventProperty(name, new ScalarValue(value));
	//			}
	//		);
		
	//	var logEvent = new LogEvent
	//	(
	//		DateTimeOffset.UtcNow,
	//		LogEventLevel.Information,
	//		null,
	//		new MessageTemplate("Test", new List<MessageTemplateToken>()),
	//		new List<LogEventProperty>()
	//	);
		
	//	var regularScope = "RegularScope";
	//	var taskScope1 = new TestExecutionContextAwareScope("Task1-Scope");
	//	var taskScope2 = new TestExecutionContextAwareScope("Task2-Scope");
		
	//	scopes.AddScope(regularScope);
		
	//	var barrier = new SemaphoreSlim(0, 2);
		
	//	// Act
	//	var task1 = Task.Run
	//	(
	//		async () =>
	//		{
	//			scopes.AddScope(taskScope1);
	//			barrier.Release();
	//			await barrier.WaitAsync();
				
	//			var properties = scopes.CreateLogEventPropertyValues(logEvent, mockPropertyFactory.Object).ToList();
	//			return properties.Count;
	//		}
	//	);
		
	//	var task2 = Task.Run
	//	(
	//		async () =>
	//		{
	//			scopes.AddScope(taskScope2);
	//			barrier.Release();
	//			await barrier.WaitAsync();
				
	//			var properties = scopes.CreateLogEventPropertyValues(logEvent, mockPropertyFactory.Object).ToList();
	//			return properties.Count;
	//		}
	//	);
		
	//	await Task.WhenAll(task1, task2);
		
	//	// Assert
	//	Assert.Multiple
	//	(
	//		() =>
	//		{
	//			// Both tasks should have created properties for their scope + regular scope.
	//			Assert.That(propertiesCreated, Does.Contain("Task1-Scope"), "Should have processed Task1 scope.");
	//			Assert.That(propertiesCreated, Does.Contain("Task2-Scope"), "Should have processed Task2 scope.");
	//			Assert.That(propertiesCreated.Count(p => p == "RegularScope"), Is.EqualTo(2), "Regular scope should be processed twice (once per task).");
	//		}
	//	);
	//}

	/// <summary>
	/// Verifies that execution context-aware scopes remain properly isolated under high concurrency scenarios.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Stress tests the <see cref="AsyncLocal{T}"/> implementation by spawning 100 concurrent tasks, each with its own unique execution context-aware scope.
	/// </para>
	/// <para>
	/// This test validates:
	/// </para>
	/// <list type="bullet">
	/// <item><description> Thread safety of the <see cref="LogScopeManager"/> implementation. </description></item>
	/// <item><description> Each task can only access its own execution context-aware scope. </description></item>
	/// <item><description> No scope leakage occurs between tasks even under heavy concurrent load. </description></item>
	/// <item><description> The implementation is suitable for real-world high-concurrency scenarios. </description></item>
	/// </list>
	/// </remarks>
	[Test]
	public async Task ExecutionContextAwareScopesUnderHighConcurrency()
	{
		// Arrange
		var scopes = new LogScopeManager();
		const int taskCount = 100;
		var tasks = new List<Task<bool>>();
		
		// Act
		for (var i = 0; i < taskCount; i++)
		{
			var taskId = i;
			tasks.Add
			(
				Task.Run
				(
					() =>
					{
						var scope = new TestExecutionContextAwareScope($"Task-{taskId}");
						scopes.AddScope(scope);
						
						var scopeValues = scopes.GetScopeValues().ToList();
						
						// Verify this task only sees its own execution context scope.
						var hasOwnScope = scopeValues.Contains(scope);
						var hasOtherTaskScope = scopeValues
							.OfType<TestExecutionContextAwareScope>()
							.Any(scope => scope.Name != $"Task-{taskId}")
							;

						// Return true if only own scope is visible.
						return hasOwnScope && !hasOtherTaskScope;
					}
				)
			);
		}
		
		var results = await Task.WhenAll(tasks);
		
		// Assert
		Assert.That(results, Has.All.True, "All tasks should only see their own execution context scope");
	}

	#endregion

	#endregion
}