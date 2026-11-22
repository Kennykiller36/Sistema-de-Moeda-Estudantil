using UglyToad.PdfPig;
var path = args[0];
using var doc = PdfDocument.Open(path);
Console.WriteLine($"PAGES {doc.NumberOfPages}");
for (int i = 1; i <= doc.NumberOfPages; i++) {
    var page = doc.GetPage(i);
    Console.WriteLine($"\n===== PAGE {i} =====");
    Console.WriteLine(page.Text);
}
