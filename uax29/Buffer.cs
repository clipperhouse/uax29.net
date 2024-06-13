namespace uax29;

internal delegate int Read<T>(T[] buffer, int offset, int count) where T : struct;

internal ref struct Buffer<T> where T : struct
{
    /// <summary>
    /// Allows the active span of the array to move with reduced copying.
    /// </summary>
    Read<T> Read;

    internal const int factor = 2;
    internal readonly T[] storage;
    internal int start = 0;
    internal int end = 0;

    internal Buffer(Read<T> read, int maxTokenSize)
    {
        this.Read = read;
        storage = new T[maxTokenSize * factor];
    }

    internal ReadOnlySpan<T> Contents
    {
        get
        {
            if (end < storage.Length)
            {
                var read = Read(storage, end, storage.Length - end);
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

        // Optimization: move the array less often
        if (start >= storage.Length / factor)
        {
            // Move the remaining unconsumed data to the start of the buffer
            Array.Copy(storage, start, storage, 0, end - start);
            end -= start;
            start = 0;
        }
    }

    internal void SetRead(Read<T> read)
    {
        this.Read = read;
        start = 0;
        end = 0;
    }
}