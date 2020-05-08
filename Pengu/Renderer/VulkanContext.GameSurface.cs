using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;

namespace Pengu.Renderer
{
	partial class VulkanContext
	{
		class GameSurface
		{
			VulkanContext context;
			FontString hexEditorFontString;

			public GameSurface(VulkanContext context)
			{
				this.context = context;

				const int editorBytes = 0x10;
				const int addressSizeBytes = 2;
				const int linesCount = 32;

				byte mem = 0;

				hexEditorFontString = context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -0.9f), 0.05f);
				var header = new string(' ', addressSizeBytes * 2) + " | " + string.Join(' ', Enumerable.Range(0, editorBytes).Select(idx => $"{idx:X2}"));
				hexEditorFontString.Value =
					header + "\n" +
					new string('-', header.Length) + "\n" +
					string.Join('\n', Enumerable.Range(0, linesCount).Select(lineIdx =>
						$"{lineIdx * editorBytes:X4} | {string.Join(' ', Enumerable.Range(0, editorBytes).Select(idx => $"{mem++:X2}"))}"));
			}
		}
	}
}
