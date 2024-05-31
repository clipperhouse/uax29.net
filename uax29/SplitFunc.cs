namespace uax29;

public delegate (int advance, byte[] token) SplitFunc(byte[] data, bool atEOF);
