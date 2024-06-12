namespace uax29;

internal delegate int Read<T>(T[] buffer, int offset, int count) where T : struct;

internal ref struct Buffer<T> where T : struct
{
    readonly T[] storage;
    Read<T> Read;
    int end = 0;

    internal Buffer(Read<T> read, int size)
    {
        this.Read = read;
        storage = new T[size];
    }

    internal Buffer(Read<T> read, T[] storage)
    {
        this.Read = read;
        this.storage = storage;
    }

    internal ReadOnlySpan<T> Contents
    {
        get
        {
            var read = Read(storage, end, storage.Length - end);
            end += read;

            return storage.AsSpan(0, end);
        }
    }

    internal void Consume(int consumed)
    {
        // Move the remaining unconsumed data to the start of the buffer
        end -= consumed;
        Array.Copy(storage, consumed, storage, 0, end);
    }

    internal void SetRead(Read<T> read)
    {
        this.Read = read;
        end = 0;
    }
}