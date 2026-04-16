//#region LICENSE NOTICE
////! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
//#endregion

//namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

///// <summary>
///// Contains extension methods for <see cref="IDisposable"/>.
///// </summary>
//public static class DisposableExtensions
//{
//	/// <summary>
//	/// Combines the <paramref name="disposables"/> in a single <see cref="IDisposable"/> instance.
//	/// </summary>
//	/// <param name="disposables"> The <see cref="IDisposable"/>s to combine. </param>
//	/// <returns> A new <see cref="IDisposable"/> that disposes each item from <paramref name="disposables"/> if it gets disposed. </returns>
//	public static IDisposable Combine(this IEnumerable<IDisposable> disposables)
//		=> new DisposableCollection(disposables);

//	internal sealed class DisposableCollection : IDisposable
//	{
//		#region Delegates / Events

//		#endregion

//		#region Constants

//		#endregion

//		#region Fields

//		private readonly HashSet<IDisposable> _disposables;

//		#endregion

//		#region Properties

//		#endregion

//		#region (De)Constructors

//		internal DisposableCollection(IEnumerable<IDisposable> disposables)
//		{
//			_disposables = [.. disposables];
//		}

//		#endregion

//		#region Methods

//		#region IDisposable

//		/// <inheritdoc />
//		public void Dispose()
//		{
//			foreach (var disposable in _disposables)
//			{
//				disposable.Dispose();
//			}
//		}

//		#endregion

//		#endregion
//	}
//}