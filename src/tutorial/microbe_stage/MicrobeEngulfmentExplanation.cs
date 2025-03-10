﻿namespace Tutorial;

using System;
using Godot;
using Newtonsoft.Json;

public class MicrobeEngulfmentExplanation : TutorialPhase
{
    [JsonProperty]
    private Vector3? chunkPosition;

    public MicrobeEngulfmentExplanation()
    {
        UsesPlayerPositionGuidance = true;
    }

    public override string ClosedByName => "MicrobeEngulfmentExplanation";

    public override void ApplyGUIState(MicrobeTutorialGUI gui)
    {
        gui.EngulfmentExplanationVisible = ShownCurrently;
    }

    public override bool CheckEvent(TutorialState overallState, TutorialEventType eventType, EventArgs args,
        object sender)
    {
        switch (eventType)
        {
            case TutorialEventType.MicrobeChunksNearPlayer:
            {
                var data = (EntityPositionEventArgs)args;

                if (!HasBeenShown && data.EntityPosition.HasValue && CanTrigger && !overallState.TutorialActive())
                {
                    Show();
                }

                if (ShownCurrently)
                {
                    chunkPosition = data.EntityPosition;
                    return true;
                }

                break;
            }

            case TutorialEventType.MicrobePlayerEngulfing:
            {
                if (!ShownCurrently)
                    break;

                // Tutorial is now complete
                Hide();
                return true;
            }
        }

        return false;
    }

    public override Vector3? GetPositionGuidance()
    {
        return chunkPosition;
    }
}
