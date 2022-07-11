#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Contains extension methods for <see cref="IDisposable"/>.
/// </summary>
public static class DisposableExtensions
{
	/// <summary>
	/// Combines the <paramref name="disposables"/> in a single <see cref="IDisposable"/> instance.
	/// </summary>
	/// <param name="disposables"> The <see cref="IDisposable"/>s to combine. </param>
	/// <returns> A new <see cref="IDisposable"/> that disposes each item from <paramref name="disposables"/> if it gets disposed. </returns>
	public static IDisposable Combine(this IEnumerable<IDisposable> disposables)
		=> new DisposableCollection(disposables);

	internal sealed class DisposableCollection : IDisposable
	{
		#region Delegates / Events

		#endregion

		#region Constants

		#endregion

		#region Fields

		private readonly HashSet<IDisposable> _disposables;

		#endregion

		#region Properties

		#endregion

		#region (De)Constructors

		internal DisposableCollection(IEnumerable<IDisposable> disposables)
		{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		_disposables = new HashSet<IDisposable>(disposables);
#else
			_disposables = disposables.ToHashSet();
#endif
		}

		#endregion

		#region Methods

		#region IDisposable

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var disposable in _disposables)
			{
				disposable.Dispose();
			}
		}

		#endregion

		#endregion
	}
}