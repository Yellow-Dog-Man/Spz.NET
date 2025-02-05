# Spz.NET: A C# Implementation of SPZ (de)serialization.

This library provides a completely native implementation of Niantic Labs' SPZ format, of which the original can be found [here](https://github.com/nianticlabs/spz).


Let's get straight to the point with an example of how to use this library.

In this example, a compressed .spz file is loaded and serialized to a .ply file.
```csharp

// Load a .spz file
GaussianCloud splat = SplatSerializer.FromSpz("/path/to/splat.spz");

// Serialize to .ply
splat.ToPly("/path/to/splat.ply");
```

And here's how to compress a .ply to a .spz:
```csharp

// Load a .ply file.
GaussianCloud splat = SplatSerializer.FromPly("/path/to/splat.ply");

// Serialize to .spz
splat.ToSpz("/path/to/splat.spz");

```

Gaussian clouds implement `IReadOnlyList<Gaussian>`, so they can be indexed and enumerated through as a normal collection.


Also included in this repo is a demo library consisting of a simple console app to demonstrate how to use Spz.NET.


Unit tests and a proper nuget release will be coming soon.