using System;
using System.Collections.Generic;
using ArkCrossEngine;

namespace LobbyRobot
{
  internal class RobotThread : MyClientThread
  {
    internal void AddRobot(string url, string user, string pwd, string gmTxt, int sceneId)
    {
      Robot robot = new Robot();
      m_Robots.Add(robot);
      robot.Init(this);
      robot.Load(gmTxt);
      robot.Start(url, user, pwd, sceneId);
    }
    protected override void OnStart()
    {
      TickSleepTime = 10;
    }

    protected override void OnTick()
    {
      foreach (Robot robot in m_Robots) {
        robot.Tick();
      }
    }

    protected override void OnQuit()
    {
    }

    private List<Robot> m_Robots = new List<Robot>();
  }
}
