using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace MailKitSample.Services
{
    public class PdfFontResolver : IFontResolver
    {
        public static readonly PdfFontResolver Instance = new();

        public string DefaultFontName => "Arial";

        public byte[] GetFont(string faceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // 実際に含まれているリソース名一覧を確認
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var name in resourceNames)
                Console.WriteLine("リソース名: " + name);

            // Arialのみ対応
            if (faceName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            {
                //var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("MailKitSample.Fonts.arial.ttf") ?? throw new InvalidOperationException("Arialフォントリソースが見つかりません。");
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
            throw new NotSupportedException($"フォント '{faceName}' はサポートされていません。");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            {
                // 必要に応じてボールドやイタリックも分岐
                return new FontResolverInfo("Arial");
            }
            // デフォルトでArialにフォールバック
            return new FontResolverInfo("Arial");
        }
    }
}
