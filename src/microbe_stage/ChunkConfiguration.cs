﻿using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

/// <summary>
///   See FloatingChunk for what many of the fields here do
/// </summary>
public struct ChunkConfiguration : IEquatable<ChunkConfiguration>
{
    public string Name;

    /// <summary>
    ///   Possible models / scenes to use for this chunk
    /// </summary>
    public List<ChunkScene> Meshes;

    /// <summary>
    ///   This is the spawn density of the chunk
    /// </summary>
    public float Density;

    public bool Dissolves;
    public float Radius;
    public float ChunkScale;
    public float PhysicsDensity;
    public float Size;

    /// <summary>
    ///   Amount of compound vented per second
    /// </summary>
    public float VentAmount;

    /// <summary>
    ///   If > 0 the amount of damage to deal on touch
    /// </summary>
    public float Damages;

    public bool DeleteOnTouch;

    /// <summary>
    ///   The name of the kind of damage type this chunk inflicts.
    /// </summary>
    public string DamageType;

    public Dictionary<Compound, ChunkCompound>? Compounds;

    /// <summary>
    ///   Whether this chunk type is an Easter egg.
    /// </summary>
    public bool EasterEgg;

    /// <summary>
    ///   The type of enzyme needed to break down this chunk.
    /// </summary>
    public string DissolverEnzyme;

    public static bool operator ==(ChunkConfiguration left, ChunkConfiguration right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChunkConfiguration left, ChunkConfiguration right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is ChunkConfiguration other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public bool Equals(ChunkConfiguration other)
    {
        return Name == other.Name &&
            Density == other.Density &&
            Dissolves == other.Dissolves &&
            Radius == other.Radius &&
            ChunkScale == other.ChunkScale &&
            PhysicsDensity == other.PhysicsDensity &&
            Size == other.Size &&
            VentAmount == other.VentAmount &&
            Damages == other.Damages &&
            DeleteOnTouch == other.DeleteOnTouch &&
            Meshes.Equals(other.Meshes) &&
            EasterEgg == other.EasterEgg &&
            DamageType == other.DamageType &&
            DissolverEnzyme == other.DissolverEnzyme &&
            Equals(Compounds, other.Compounds);
    }

    public struct ChunkCompound : IEquatable<ChunkCompound>
    {
        public float Amount;

        public static bool operator ==(ChunkCompound left, ChunkCompound right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkCompound left, ChunkCompound right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is ChunkCompound other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)Amount;
        }

        public bool Equals(ChunkCompound other)
        {
            return Amount == other.Amount;
        }
    }

    /// <summary>
    ///   Don't modify instances of this class
    /// </summary>
    public class ChunkScene
    {
        /// <summary>
        ///   Scene to use for this chunk. Note that this and the following 2 variables reflect
        ///   <see cref="SceneWithModelInfo"/> but this isn't converted to use that for save compatibility (and this
        ///   would be a slight variant with the scene path not being a loaded scene).
        /// </summary>
        public string ScenePath;

        /// <summary>
        ///   Path to the MeshInstance inside the ScenePath scene, null if it is the root
        /// </summary>
        public NodePath? SceneModelPath;

        /// <summary>
        ///   Path to the AnimationPlayer inside the ScenePath scene, null if no animation
        /// </summary>
        public NodePath? SceneAnimationPath;

        /// <summary>
        ///   Path to the convex collision shape of this chunk's graphical mesh (if any).
        /// </summary>
        public string? ConvexShapePath;

        /// <summary>
        ///   Need to be set to true on particle type visuals as those need special handling
        /// </summary>
        public bool IsParticles;

        /// <summary>
        ///   If true, animations won't be stopped on this scene when this is spawned as a chunk
        /// </summary>
        public bool PlayAnimation;

        /// <summary>
        ///   If true, then the default shader (material retrieve) is not done, and it is assumed that normal shader
        ///   operations like dissolving are unavailable
        /// </summary>
        public bool MissingDefaultShaderSupport;

        [JsonConstructor]
        public ChunkScene(string scenePath)
        {
            ScenePath = scenePath;

            SceneModelPath = null;
            SceneAnimationPath = null;
            ConvexShapePath = null;
            IsParticles = false;
            PlayAnimation = false;
            MissingDefaultShaderSupport = false;
        }

        public ChunkScene(LoadedSceneWithModelInfo fromModelInfo)
        {
            // TODO: investigate if it would make sense to switch ScenePath to be also a loaded scene (that would be
            // saved and loaded from JSON)
            ScenePath = fromModelInfo.LoadedScene.ResourcePath;

            SceneModelPath = fromModelInfo.ModelPath;

            SceneAnimationPath = fromModelInfo.AnimationPlayerPath;

            // Default init for non-copied things
            ConvexShapePath = null;
            IsParticles = false;
            PlayAnimation = false;
            MissingDefaultShaderSupport = false;
        }
    }
}
