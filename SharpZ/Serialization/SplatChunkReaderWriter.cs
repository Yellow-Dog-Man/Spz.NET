using System.Numerics;

namespace SharPZ;

internal readonly ref struct SplatChunkReaderWriter(
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

    public readonly GaussianHarmonics<float> Sh
    {
        get
        {
            GaussianHarmonics<float> sh = new();

            for (int i = 0; i < shDim * 3; i++)
            {
                sh[i] = chunk[shIdx[i]];
            }

            return sh.Transpose();
        }

        set
        {
            GaussianHarmonics<float> sh = value.Transpose();
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
