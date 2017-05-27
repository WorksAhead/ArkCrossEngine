using System;
using System.Collections.Generic;
using System.Text;

namespace DashFire
{
  internal sealed class SceneProfiler
  {
    internal long DelayActionProcessorTime = 0;
    internal long MovementSystemTime = 0;
    internal long SpatialSystemTime = 0;
    internal long AiSystemTime = 0;
    internal long SceneLogicSystemTime = 0;
    internal long StorySystemTime = 0;
    internal long TickSkillTime = 0;
    internal long TickUsersTime = 0;
    internal long TickNpcsTime = 0;
    internal long TickLevelupTime = 0;
    internal long TickAttrRecoverTime = 0;
    internal long TickDebugSpaceInfoTime = 0;
    internal long SightTickTime = 0;

    internal string GenerateLogString(int sceneId,long elapsedTime)
    {
      StringBuilder builder = new StringBuilder();

      builder.Append("=>SceneResourceId:").Append(sceneId).Append(",ElapsedTime:").Append(elapsedTime).AppendLine();

      builder.Append("=>DelayActionProcessorTime:").Append(DelayActionProcessorTime).AppendLine();
      
      builder.Append("=>MovementSystemTime:").Append(MovementSystemTime).AppendLine();
      
      builder.Append("=>SpatialSystemTime:").Append(SpatialSystemTime).AppendLine();

      builder.Append("=>AiSystemTime:").Append(AiSystemTime).AppendLine();

      builder.Append("=>SceneLogicSystemTime:").Append(SceneLogicSystemTime).AppendLine();

      builder.Append("=>StorySystemTime:").Append(StorySystemTime).AppendLine();
      
      builder.Append("=>TickSkillTime:").Append(TickSkillTime).AppendLine();
      
      builder.Append("=>TickUsersTime:").Append(TickUsersTime).AppendLine();
      
      builder.Append("=>TickNpcsTime:").Append(TickNpcsTime).AppendLine();
      
      builder.Append("=>TickLevelupTime:").Append(TickLevelupTime).AppendLine();
      
      builder.Append("=>TickAttrRecoverTime:").Append(TickAttrRecoverTime).AppendLine();

      builder.Append("=>TickDebugSpaceInfoTime:").Append(TickDebugSpaceInfoTime).AppendLine();

      builder.Append("=>SightTickTime:").Append(SightTickTime).AppendLine();

      return builder.ToString();
    }
  }
}
