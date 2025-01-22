using System.Collections;
using System.Text;

namespace Spuzzy;

public readonly struct MatrixView<T> where T : unmanaged
{
	public readonly int Width => Transposed ? _height : _width;
	private readonly int _width;

    public readonly int Height => Transposed ? _width : _height;
	private readonly int _height;


	public readonly T this[int x, int y]
	{
        get => Access(x, y);
		set => Access(x, y) = value;
	}


	public readonly T this[int index]
	{
		get => Access(index / Width, index % Width);
		set => Access(index / Width, index % Width) = value;
	}
	

	public readonly int Length => _width * _height;

	public readonly Span<T> Span => data.Span;

	
	internal readonly Memory<T> data;
	private readonly bool Transposed { get; init; }

	
	


    public MatrixView(int width, int height)
    {
        data = new T[width * height];
        _width = width;
        _height = height;
    }

	public MatrixView(Memory<T> array, int width, int height)
    {
		if (array.Length < width * height)
			throw new ArgumentOutOfRangeException($"Provided array was too small for the desired width and height!");
		
        data = array;
        _width = width;
        _height = height;
    }

    public readonly MatrixSegment<T> GetRow(int row) => new(this, row);
	public readonly Span<T> GetRowSpan(int row) => Span.Slice(row * _width, _width);
	
	
	public readonly MatrixView<T> Transpose() => this with { Transposed = !Transposed };



	private readonly ref T Access(int x, int y) => ref Transposed ? ref data.Span[y * _width + x] : ref data.Span[x * _width + y];



	
	public override string ToString()
	{
		StringBuilder builder = new();
		

		for (int i = 0; i < Height; i++)
		{
			builder.Append('[');
			builder.Append($"{string.Join(", ", GetRow(i).Select(row => $"{row:d3}"))}");
			builder.Append(']');
			builder.AppendLine();
		}

		return builder.ToString();
	}
}


public readonly struct MatrixSegment<T> : IEnumerable<T>
	where T : unmanaged
{
	private readonly MatrixView<T> _view;
	public readonly int Length;
	private readonly int _row;


	public readonly T this[int index]
	{
		get => _view[_row, index];
		set => _view[_row, index] = value;
	}

	internal MatrixSegment(MatrixView<T> view, int row)
	{
		Length = view.Width;
		_view = view;
		_row = row;
	}


    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new MatrixEnumerator(this);
	IEnumerator IEnumerable.GetEnumerator() => new MatrixEnumerator(this);



    public struct MatrixEnumerator(MatrixSegment<T> segment) : IEnumerator<T>
	{
		private readonly MatrixSegment<T> _segment = segment;
		private int _curIndex = -1;

		readonly T IEnumerator<T>.Current
		{
			get
			{
				try
				{
					return _segment[_curIndex];
				}
				catch (IndexOutOfRangeException)
				{
					return default;
				}
			}
		}

		readonly object? IEnumerator.Current
		{
			get
			{
				try
				{
					return _segment[_curIndex];
				}
				catch (IndexOutOfRangeException)
				{
					return default;
				}
			}
		}

		public bool MoveNext() => ++_curIndex < _segment.Length;


		public void Reset() => _curIndex = -1;


		public readonly void Dispose() { }
	}
}