﻿

/****    影袭影子攻击二段    ****/

skill(125102)
{
  section(1)//初始化
  {
    movechild(0, "3_Cike_w_01", "ef_righthand");//初始化主武器
    movechild(0, "3_Cike_w_02", "ef_lefthand");//初始化主武器
    movecontrol(true);
    //
    //设定方向为施法者方向
    settransform(0," ",vector3(0,0,0),eular(0,0,0),"RelativeOwner",false);
		//findmovetarget(0, vector3(0, 0, 1), 2.5, 60, 0.1, 0.9, 0, -0.8);
  };

  section(200)//起手
  {
    animation("Cike_Skill06_shadow02_01")
    {
      speed(1);
    };
    //
    //角色移动
    startcurvemove(10, true, 0.18, 0, 0, 37, 0, 0, -20);
    //
    //特效
    //charactereffect("Hero_FX/3_Cike/3_Hero_CiKe_ShadowXian_02", 2000, "Bone_Root", 0);
    sceneeffect("Hero_FX/3_Cike/3_Hero_CiKe_ShadowXian_02", 3000, vector3(0, 0, 0), 0, eular(0, 0, 0), vector3(1, 1, 1), true);
  };

  section(250)//第一段
  {
    animation("Cike_Skill06_shadow02_02")
    {
      speed(1);
    };
    //
    //伤害判定
    areadamage(10, 0, 1.5, 1.5, 2.4, true)
		{
			stateimpact("kDefault", 12060201);
			stateimpact("kLauncher", 12060203);
			stateimpact("kKnockDown", 12060203);
      //showtip(200, 0, 0, 1);
		};
    //
    //特效
    sceneeffect("Hero_FX/3_Cike/3_Hero_CiKe_shadow01_02", 3000, vector3(0, 0, 0), 0, eular(0, 0, 0), vector3(1, 1, 1), true);
    sceneeffect("Hero_FX/3_Cike/3_Hero_CiKe_CiJiShouJi_HeiYing_01_002", 3000, vector3(0, 0, 0), 240, eular(0, 0, 0), vector3(1, 1, 1), true);
    //
    //音效
    playsound(10, "Hit", "Sound/Cike/CikeSkillSound01", 1000, "Sound/Cike/Cike_Skill06_YingXi_01", false);
    playsound(10, "Hit02", "Sound/Cike/CikeSkillSound01", 1000, "Sound/Cike/guaiwu_shouji_01", true)
    {
			audiogroup("Sound/Cike/guaiwu_shouji_02", "Sound/Cike/guaiwu_shouji_03", "Sound/Cike/guaiwu_shouji_04");
    };
  };

  oninterrupt() //技能在被打断时会运行该段逻辑
  {
    //模型消失
    setenable(0, "Visible", true);
    destroyself(1);
  };

  onstop() //技能正常结束时会运行该段逻辑
  {
    //模型消失
    setenable(0, "Visible", true);
    destroyself(1);
  };
};
