using System;
using System.Text;

namespace Josupeit.Practices.Disposables.Mixins;

internal sealed class IndentingStringBuilder
{
    private const string INDENTATION = "    ";

    private static readonly ReadOnlyMemory<char> _newLine = Environment.NewLine.AsMemory();
    private static readonly int _newLineLen = _newLine.Length;

    private readonly StringBuilder _sb = new();

    private bool _prependIndentation;
    private int _level;

    public IndentingStringBuilder Append(ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
            return AppendIndentationIfRequired();

        var indexOfNewLine = chars.IndexOf(_newLine.Span);

        if (indexOfNewLine == -1)
            return AppendIndentationIfRequired().AppendCore(chars);

        var indexOfNewLineIncluded = indexOfNewLine + _newLineLen;
        var line = chars[..indexOfNewLineIncluded];
        var remainder = chars[indexOfNewLineIncluded..];

        return AppendIndentationIfRequired().AppendCore(line).AppendIndentation().Append(remainder);
    }

    public IndentingStringBuilder Append(string s)
    {
        return Append(s.AsSpan());
    }

    public IndentingStringBuilder AppendLine(ReadOnlySpan<char> chars)
    {
        Append(chars);

        _sb.AppendLine();
        _prependIndentation = true;

        return this;
    }
    
    public IndentingStringBuilder AppendLine(string s)
    {
        return AppendLine(s.AsSpan());
    }

    public UnindentAction Indent()
    {
        _level++;

        return new UnindentAction(this);
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    private IndentingStringBuilder AppendCore(ReadOnlySpan<char> chars)
    {
        _sb.Append(chars.ToArray());

        return this;
    }

    private IndentingStringBuilder AppendIndentationIfRequired()
    {
        var prependIndentation = _prependIndentation;
        _prependIndentation = false;

        return prependIndentation ? AppendIndentation() : this;
    }

    private IndentingStringBuilder AppendIndentation()
    {
        for (var i = 0; i < _level; i++)
            _sb.Append(INDENTATION);

        return this;
    }

    public readonly ref struct UnindentAction : IDisposable
    {
        private readonly IndentingStringBuilder? _builder;
        private readonly int _level;

        public UnindentAction(IndentingStringBuilder builder)
        {
            _builder = builder;
            _level = _builder._level;
        }

        public void Dispose()
        {
            if (_builder == null)
                return;

            if (_builder._level-- != _level)
                throw new InvalidOperationException();
        }
    }
}
