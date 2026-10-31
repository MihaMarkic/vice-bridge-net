using System.Globalization;

namespace Righthand.ViceMonitor.Bridge.Test.TestExtensions;

public static class StringExtension
{
	extension(string textHexBytes)
	{
		/// <summary>
		/// Parses hex bytes single space delimited text representation to byte array.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		/// <example>
		/// A valid input value would be <code>fa 10 2b</code>
		/// </example>
		public ReadOnlySpan<byte> ToHexBytes()
		{
			if (string.IsNullOrWhiteSpace(textHexBytes))
			{
				return [];
			}
			
			int byteCount = textHexBytes.Count(c => c == ' ') + 1;
			var builder = ImmutableArray.CreateBuilder<byte>(byteCount);
			foreach (var part in textHexBytes.AsSpan().Split(' '))
			{
				if (!byte.TryParse(textHexBytes[part], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out byte b))
				{
					throw new Exception($"Part {textHexBytes[part]} is not a valid hex byte");
				}
				builder.Add(b);
			}
			return builder.ToImmutable().AsSpan();
		}
	}
}