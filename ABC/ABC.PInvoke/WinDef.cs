using System.Runtime.InteropServices;


namespace ABC.PInvoke
{
	/// <summary>
	///   Basic Windows type definitions, originally located in windef.h.
	/// </summary>
	public class WinDef
	{
		/// <summary>
		///   The Rectangle structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
		/// </summary>
		/// <remarks>
		///   By convention, the right and bottom edges of the rectangle are normally considered exclusive.
		///   In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the rectangle.
		///   For example, when Rectangle is passed to the FillRect function, the rectangle is filled up to,
		///   but not including, the right column and bottom row of pixels.
		/// </remarks>
		[StructLayout( LayoutKind.Sequential )]
		public struct Rectangle
		{
			/// <summary>
			///   The x-coordinate of the upper-left corner of the rectangle.
			/// </summary>
			public int Left;

			/// <summary>
			///   The y-coordinate of the upper-left corner of the rectangle.
			/// </summary>
			public int Top;

			/// <summary>
			///   The x-coordinate of the lower-right corner of the rectangle.
			/// </summary>
			public int Right;

			/// <summary>
			///   The y-coordinate of the lower-right corner of the rectangle.
			/// </summary>
			public int Bottom;
		}
	}
}