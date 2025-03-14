﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Saving.Serializers;

/// <summary>
///   Represents a macroscopic species that is 3D and composed of placed tissues
/// </summary>
[JsonObject(IsReference = true)]
[TypeConverter($"Saving.Serializers.{nameof(ThriveTypeConverter)}")]
[JSONDynamicTypeAllowed]
[UseThriveConverter]
[UseThriveSerializer]
public class MacroscopicSpecies : Species
{
    public MacroscopicSpecies(uint id, string genus, string epithet) : base(id, genus, epithet)
    {
    }

    [JsonProperty]
    public MacroscopicMetaballLayout BodyLayout { get; private set; } = new();

    [JsonProperty]
    public List<CellType> CellTypes { get; private set; } = new();

    /// <summary>
    ///   The scale in meters of the species
    /// </summary>
    public float Scale { get; set; } = 1.0f;

    [JsonProperty]
    public float BrainPower { get; private set; }

    [JsonProperty]
    public float MuscularPower { get; private set; }

    /// <summary>
    ///   Where this species reproduces, used to control also where individuals of this species spawn and where the
    ///   player spawns
    /// </summary>
    [JsonProperty]
    public ReproductionLocation ReproductionLocation { get; set; }

    [JsonProperty]
    public MacroscopicSpeciesType MacroscopicType { get; private set; }

    /// <summary>
    ///   All organelles in all the species' placed metaballs (there can be a lot of duplicates in this list)
    /// </summary>
    [JsonIgnore]
    public IEnumerable<OrganelleTemplate> Organelles =>
        BodyLayout.Select(m => m.CellType).Distinct().SelectMany(c => c.Organelles);

    [JsonIgnore]
    public override string StringCode => ThriveJsonConverter.Instance.SerializeObject(this);

    public static MacroscopicSpeciesType CalculateMacroscopicTypeFromLayout(MetaballLayout<MacroscopicMetaball> layout,
        float scale)
    {
        var brainPower = CalculateBrainPowerFromLayout(layout, scale);

        if (brainPower >= Constants.BRAIN_POWER_REQUIRED_FOR_AWAKENING)
        {
            return MacroscopicSpeciesType.Awakened;
        }

        if (brainPower >= Constants.BRAIN_POWER_REQUIRED_FOR_AWARE)
        {
            return MacroscopicSpeciesType.Aware;
        }

        return MacroscopicSpeciesType.Macroscopic;
    }

    public override void OnEdited()
    {
        base.OnEdited();

        RepositionToOrigin();
        UpdateInitialCompounds();
        CalculateBrainPower();
        CalculateMuscularPower();

        // Note that a few stage transitions are explicit for the player so the editor will override this
        SetTypeFromBrainPower();
    }

    public override bool RepositionToOrigin()
    {
        return BodyLayout.RepositionToGround();
    }

    public override void UpdateInitialCompounds()
    {
        // TODO: change this to be dynamic similar to microbe stage

        var simulation = SimulationParameters.Instance;

        var rusticyanin = simulation.GetOrganelleType("rusticyanin");
        var chemo = simulation.GetOrganelleType("chemoplast");
        var chemoProtein = simulation.GetOrganelleType("chemoSynthesizingProteins");

        if (Organelles.Any(o => o.Definition == rusticyanin))
        {
            SetInitialCompoundsForIron();
        }
        else if (Organelles.Any(o => o.Definition == chemo ||
                     o.Definition == chemoProtein))
        {
            SetInitialCompoundsForChemo();
        }
        else
        {
            SetInitialCompoundsForDefault();
        }
    }

    public override void HandleNightSpawnCompounds(CompoundBag targetStorage, ISpawnEnvironmentInfo spawnEnvironment)
    {
        // TODO: implement something here if required (probably needed for plants at least if they use this class)
    }

    public override void ApplyMutation(Species mutation)
    {
        base.ApplyMutation(mutation);

        var casted = (MacroscopicSpecies)mutation;

        CellTypes.Clear();

        foreach (var cellType in casted.CellTypes)
        {
            CellTypes.Add((CellType)cellType.Clone());
        }

        BodyLayout.Clear();

        var metaballMapping = new Dictionary<Metaball, MacroscopicMetaball>();

        // Make sure we process things with parents first
        // TODO: if the tree depth calculation is too expensive here, we'll need to cache the values in the metaball
        // objects
        foreach (var metaball in casted.BodyLayout.OrderBy(m => m.CalculateTreeDepth()))
        {
            BodyLayout.Add(metaball.Clone(metaballMapping));
        }
    }

    public override float GetPredationTargetSizeFactor()
    {
        throw new NotImplementedException("Size factor for auto-evo not implemented for macroscopic species");
    }

    /// <summary>
    ///   Explicitly moves the player to awakened status, this is like this to make sure the player wouldn't get stuck
    ///   underwater if they accidentally increased their brain power
    /// </summary>
    public void MovePlayerToAwakenedStatus()
    {
        MacroscopicType = MacroscopicSpeciesType.Awakened;
    }

    public void KeepPlayerInAwareStage()
    {
        if (MacroscopicType == MacroscopicSpeciesType.Awakened)
            MacroscopicType = MacroscopicSpeciesType.Aware;
    }

    public override object Clone()
    {
        var result = new MacroscopicSpecies(ID, Genus, Epithet);

        ClonePropertiesTo(result);

        foreach (var cellType in CellTypes)
        {
            result.CellTypes.Add((CellType)cellType.Clone());
        }

        var metaballMapping = new Dictionary<Metaball, MacroscopicMetaball>();

        foreach (var metaball in BodyLayout)
        {
            result.BodyLayout.Add(metaball.Clone(metaballMapping));
        }

        return result;
    }

    private static float CalculateBrainPowerFromLayout(MetaballLayout<MacroscopicMetaball> layout, float scale)
    {
        float result = 0;

        foreach (var metaball in layout)
        {
            if (metaball.CellType.IsBrainTissueType())
            {
                // TODO: check that volume scaling in physically sensible way (using GetVolume) is what we want here
                // Maybe we would actually just want to multiply by the scale number to buff small species' brain?
                result += metaball.GetVolume(scale);
            }
        }

        return result;
    }

    private static float CalculateMuscularPowerFromLayout(MetaballLayout<MacroscopicMetaball> layout, float scale)
    {
        float result = 0;

        foreach (var metaball in layout)
        {
            if (metaball.CellType.IsMuscularTissueType())
            {
                // TODO: check that volume scaling in physically sensible way (using GetVolume) is what we want here
                result += metaball.GetVolume(scale);
            }
        }

        return result;
    }

    private void SetTypeFromBrainPower()
    {
        MacroscopicType = CalculateMacroscopicTypeFromLayout(BodyLayout, Scale);
    }

    private void SetInitialCompoundsForDefault()
    {
        InitialCompounds.Clear();

        // TODO: modify these numbers based on the scale and metaball count or something more accurate
        InitialCompounds.Add(Compound.ATP, 180);
        InitialCompounds.Add(Compound.Glucose, 90);
    }

    private void SetInitialCompoundsForIron()
    {
        SetInitialCompoundsForDefault();
        InitialCompounds.Add(Compound.Iron, 90);
    }

    private void SetInitialCompoundsForChemo()
    {
        SetInitialCompoundsForDefault();
        InitialCompounds.Add(Compound.Hydrogensulfide, 90);
    }

    private void CalculateBrainPower()
    {
        BrainPower = CalculateBrainPowerFromLayout(BodyLayout, Scale);
    }

    private void CalculateMuscularPower()
    {
        MuscularPower = CalculateMuscularPowerFromLayout(BodyLayout, Scale);
    }
}
