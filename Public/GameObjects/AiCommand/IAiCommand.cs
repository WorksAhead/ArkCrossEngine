﻿namespace ArkCrossEngine
{
    public interface IAiCommand
    {
        bool Execute(long deltaTime);//返回true表明命令结束，可以从命令队列里移除了
        void Recycle();
    }
}
