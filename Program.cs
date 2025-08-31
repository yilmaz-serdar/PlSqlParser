// See https://aka.ms/new-console-template for more information
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System.Collections;

Console.WriteLine("Hello, World!");

string sql = @"
            SELECT e.employee_id, e.first_name
            FROM hr.employees e
            JOIN sales.orders o ON o.emp_id = e.employee_id
            WHERE EXISTS (SELECT 1 FROM dept@dblink d WHERE d.id = e.department_id);

            INSERT INTO app.audit_log (id) VALUES (1);
        ";

var objects = OracleSqlObjects.ExtractObjectNames(sql);

Console.WriteLine("SQL içinde bulunan objeler:");
foreach (var obj in objects)
{
    Console.WriteLine(obj);
}

public class CaseChangingCharStream : ICharStream
{
    private readonly ICharStream _innerStream;
    private readonly bool _toUpper;

    public CaseChangingCharStream(ICharStream innerStream, bool toUpper = true)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        _toUpper = toUpper;
    }

    public int Index => _innerStream.Index;
    public int Size => _innerStream.Size;
    public string SourceName => _innerStream.SourceName;

    public void Consume() => _innerStream.Consume();

    public int LA(int i)
    {
        int c = _innerStream.LA(i);
        if (c <= 0) return c; // EOF
        char ch = (char)c;
        // Tırnak içinde değilse upper-case uygula
        return _toUpper ? char.ToUpperInvariant(ch) : ch;
    }

    public int Mark() => _innerStream.Mark();
    public void Release(int marker) => _innerStream.Release(marker);
    public void Seek(int index) => _innerStream.Seek(index);
    public string GetText(Interval interval) => _innerStream.GetText(interval);
}

public static class OracleSqlObjects
{
    public static IReadOnlyCollection<string> ExtractObjectNames(string sql)
    {
        // 1) Input'u case-insensitive parse için upper-case'e çeviriyoruz
        var input = new AntlrInputStream(sql);
        var upper = new CaseChangingCharStream(input, true);

        // 2) Lexer/Parser
        var lexer = new PlSqlLexer(upper);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PlSqlParser(tokens)
        {
            // Hata oluşursa parse durur
            ErrorHandler = new BailErrorStrategy()
        };

        // 3) Giriş noktası (script birden çok statement içerebilir)
        var tree = parser.sql_script();

        // 4) Visitor ile tabloları, viewleri vs. çıkar
        var visitor = new ObjectNameVisitor(tokens);
        visitor.Visit(tree);

        return visitor.Results;
    }

    private sealed class ObjectNameVisitor : PlSqlParserBaseVisitor<object>
    {
        private readonly CommonTokenStream _tokens;
        private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

        public ObjectNameVisitor(CommonTokenStream tokens) => _tokens = tokens;

        public IReadOnlyCollection<string> Results => _names;

        // Table/View/MaterializedView/Synonym kurallarını ziyaret ediyoruz
        public override object? VisitTableview_name(PlSqlParser.Tableview_nameContext context)
        {
            AddQualifiedName(context);
            return base.VisitTableview_name(context);
        }

        public override object? VisitTable_name(PlSqlParser.Table_nameContext context)
        {
            AddQualifiedName(context);
            return base.VisitTable_name(context);
        }

        private void AddQualifiedName(ParserRuleContext ctx)
        {
            // Tokenları al ve nokta ile birleştir
            var parts = new List<string>();
            for (int i = 0; i < ctx.ChildCount; i++)
            {
                var text = ctx.GetChild(i).GetText();
                if (text == ".") continue;

                // Çift tırnaklı isimleri temizle
                if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
                    text = text.Substring(1, text.Length - 2);

                if (!string.IsNullOrWhiteSpace(text))
                    parts.Add(text);
            }

            if (parts.Count > 0)
            {
                string name = string.Join(".", parts);
                // DB link varsa at (optional)
                int atIndex = name.IndexOf('@');
                if (atIndex >= 0) name = name.Substring(0, atIndex);

                _names.Add(name);
            }
        }
    }
}