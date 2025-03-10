﻿namespace Components;

/// <summary>
///   Entity is a member of a species and has species related data applied to it. Note that for most things
///   <see cref="CellProperties"/> should be used instead as that works for multicellular things as well.
/// </summary>
[ComponentIsReadByDefault]
[JSONDynamicTypeAllowed]
public struct MicrobeSpeciesMember
{
    public MicrobeSpecies Species;
}
