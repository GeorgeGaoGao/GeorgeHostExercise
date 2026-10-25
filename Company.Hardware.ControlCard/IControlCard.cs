using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.ControlCard
{
    public  interface IControlCard
    {
       
        bool IsInitialized { get; }
        bool IsReady { get; }
        bool IsAxisHomed {  get; }
        bool Initialize(IControlCardConfigProvider controlCardConfigProvider);
        void Close();
        void SetSpeedMode(SpeedMode speedMode);//设置运动轴速度模式
        bool GoHome(out string message);//运动轴回零
        bool Move(AxisType axisType,double distance,out string message);
        bool Move(AxisType axisType, MoveDirection moveDirection);
        void Stop(AxisType? axisType);
    }
}
