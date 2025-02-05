using System.Numerics;

namespace Spz.NET.Structs;

/// <summary>
/// Organizes access to a chunk of data representing a gaussian from a PLY file.
/// </summary>
/// <param name="chunk">Data chunk.</param>
/// <param name="positionIdx">Index of the position components.</param>
/// <param name="scaleIdx">Index of the scale components.</param>
/// <param name="rotIdx">Index of the rotation components.</param>
/// <param name="alphaIdx">Index of the alpha components.</param>
/// <param name="colorIdx">Index of the color components.</param>
/// <param name="shIdx">Index of the harmonics components.</param>
/// <param name="shDim">The dimensions of this gaussian.</param>
internal readonly ref struct PlyChunker(
    Span<float> chunk,
    Span<int> positionIdx,
    Span<int> scaleIdx,
    Span<int> rotIdx,
    int alphaIdx,
    Span<int> colorIdx,
    Span<int> shIdx,
    int shDim)
{
    readonly Span<float> chunk = chunk;
    readonly Span<int> positionIdx = positionIdx;
    readonly Span<int> scaleIdx = scaleIdx;
    readonly Span<int> rotIdx = rotIdx;
    readonly Span<int> colorIdx = colorIdx;
    readonly Span<int> shIdx = shIdx;


    public readonly Vector3 Position
    {
        get => new(chunk[positionIdx[0]], chunk[positionIdx[1]], chunk[positionIdx[2]]);
        set
        {
            chunk[positionIdx[0]] = value.X;
            chunk[positionIdx[1]] = value.Y;
            chunk[positionIdx[2]] = value.Z;
        }
    }
    public readonly Vector3 Scale
    {
        get => new(chunk[scaleIdx[0]], chunk[scaleIdx[1]], chunk[scaleIdx[2]]);
        set
        {
            chunk[scaleIdx[0]] = value.X;
            chunk[scaleIdx[1]] = value.Y;
            chunk[scaleIdx[2]] = value.Z;
        }
    }

    public readonly Quaternion Rotation
    {
        // The quaternion is read as WXYZ, so shove W at the end to encode it as normal.
        get => new(chunk[rotIdx[1]], chunk[rotIdx[2]], chunk[rotIdx[3]], chunk[rotIdx[0]]);
        set
        {
            chunk[rotIdx[0]] = value.W;
            chunk[rotIdx[1]] = value.X;
            chunk[rotIdx[2]] = value.Y;
            chunk[rotIdx[3]] = value.Z;
        }
    }

    public readonly Vector3 Color
    {
        get => new(chunk[colorIdx[0]], chunk[colorIdx[1]], chunk[colorIdx[2]]);
        set
        {
            chunk[colorIdx[0]] = value.X;
            chunk[colorIdx[1]] = value.Y;
            chunk[colorIdx[2]] = value.Z;
        }
    }

    public readonly float Alpha
    {
        get => chunk[alphaIdx];
        set => chunk[alphaIdx] = value;
    }

    public readonly GaussianHarmonics Sh
    {
        get
        {
            GaussianHarmonics sh = new();

            for (int i = 0; i < shDim * 3; i++)
            {
                sh[i] = chunk[shIdx[i]];
            }

            // Transpose the harmonics such that they become vertically-rowed. E.g:
            // [RGB]
            // [RGB]
            // [RGB]

            // It is a mystery as to what [N,S,C] or [N,C,S] ordering is for the harmonics as far as I know.
            return sh.ToNSC();
        }

        set
        {
            // Transpose the harmonics such that they become horizontally-rowed. E.g:
            // [RRRR]
            // [GGGG]
            // [BBBB]

            GaussianHarmonics sh = value.ToNCS();
            for (int i = 0; i < shDim * 3; i++)
            {
                chunk[shIdx[i]] = sh[i];
            }
        }
    }


    public readonly Gaussian Gaussian
    {
        get => new(
            Position,
            Scale,
            Rotation,
            Alpha,
            Color,
            Sh
        );

        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            Alpha = value.Alpha;
            Color = value.Color;
            Sh = value.Sh;
        }
    }
}
