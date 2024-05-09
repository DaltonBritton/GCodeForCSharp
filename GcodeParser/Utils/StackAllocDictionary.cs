using System.Diagnostics.CodeAnalysis;

namespace GcodeParser.Utils;

/// <summary>
/// This is a Dictionary allocated on the stack to reduce the number of heap allocations when parsing commands.
/// StackAllocDictionary should contain few elements because all methods have a linear growth rate.
/// </summary>
public ref struct StackAllocDictionary<TKey, TValue>
{
    private Span<TKey> _keys;
    private Span<TValue> _values;

    /// <summary>
    /// Gets the number of elements in the Dictionary
    /// </summary>
    public int Count { get; private set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="values"></param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="keys"/> and <paramref name="values"/> do not have the same length.</exception>
    public StackAllocDictionary(Span<TKey> keys, Span<TValue> values)
    {
        _keys = keys;
        _values = values;

        if (_keys.Length != _values.Length)
            throw new ArgumentException("keys and value spans must have the same length");
    }

    /// <summary>
    /// Gets or sets values within the dictionary
    /// </summary>
    /// <param name="key">The key to get or set the value of</param>
    public TValue this[TKey key]
    {
        get
        {
            if (TryGet(key, out TValue? result))
                return result;

            throw new ArgumentException("Key does not exist");
        }

        set
        {
            if(!TryAdd(key, value))
                throw new OutOfMemoryException("Unable to add element to Dictionary");
        }
    }

    /// <summary>
    /// Tries to add element to the dictionary
    /// </summary>
    /// <returns>True if element was successfully added to the dictionary, False if dictionary ran out of memory</returns>
    public bool TryAdd(TKey key, TValue value)
    {
        for (int i = 0; i < Count; i++)
        {
            TKey possibleKey = _keys[i];
            if (possibleKey == null || !possibleKey.Equals(key))
                continue;
            
            _values[i] = value;
            return true;
        }

        if (Count >= _keys.Length)
            return false;

        _keys[Count] = key;
        _values[Count] = value;
        Count++;

        return true;
    }

    /// <summary>
    /// Tries to get element in the dictionary
    /// </summary>
    /// <returns>True if element was found, False if otherwise</returns>
    public bool TryGet(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        for (int i = 0; i < Count; i++)
        {
            TKey possibleKey = _keys[i];
            if (possibleKey == null || !possibleKey.Equals(key))
                continue;
            
            value = _values[i];
            return value != null;
        }

        value = default;
        return false;
    }
    
    
}