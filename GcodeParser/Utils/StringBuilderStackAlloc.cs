namespace GcodeParser.Utils;

public ref struct StringBuilderStackAlloc
{
    private Span<char> _buffer;
    private int _currentIndex = -1;

    public StringBuilderStackAlloc(Span<char> buffer)
    {
        _buffer = buffer;
    }

    public void Append(ReadOnlySpan<char> str)
    {
        foreach (var charater in str)
        {
            _currentIndex++;
            _buffer[_currentIndex] = charater;
        }
    }

    public ReadOnlySpan<char> GetReadOnlySpan()
    {
        return _buffer.Slice(0, _currentIndex+1);
    }
}