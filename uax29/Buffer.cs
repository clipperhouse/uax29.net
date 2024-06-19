namespace uax29;

internal delegate int Read<T>(T[] buffer, int offset, int count) where T : struct;

internal ref struct Buffer<T> where T : struct
{
    /// <summary>
    /// Allows the active span of the array to move with reduced copying.
    /// </summary>
    Read<T> read;
    internal int minItems = 0;
    internal readonly T[] storage;

    internal int start = 0;
    internal int end = 0;

    internal Buffer(Read<T> read, int minItems, T[]? storage = null)
    {
        this.read = read;
        this.minItems = minItems;
        if (storage != null && storage.Length < minItems)
        {
            throw new ArgumentException($"Storage ({typeof(T)}[{storage.Length}]) must be at least as large as minItems ({minItems}).");
        }
        storage ??= new T[minItems];
        this.storage = storage;
    }

    internal ReadOnlySpan<T> Contents
    {
        get
        {
            var len = end - start;
            if (len < minItems)
            {
                // Move the remaining unconsumed data to the start of the buffer
                Array.Copy(storage, start, storage, 0, end - start);
                end -= start;
                start = 0;

                var read = this.read(storage, end, storage.Length - end);
                end += read;
            }
            return storage.AsSpan(start, end - start);
        }
    }

    internal void Consume(int consumed)
    {
        var remaining = end - start;
        if (consumed > remaining)
        {
            consumed = remaining;
        }

        start += consumed;
    }

    internal void SetRead(Read<T> read)
    {
        this.read = read;
        start = 0;
        end = 0;
    }
}