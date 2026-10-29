using Prism.Modularity;
using ReactiveUI.Primitives.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Company.Hardware.ControlCard.None
{
    public class ControlCardNone : ControlCardBase
    {
        private bool _isAxisHomed;

        public override bool IsAxisHomed => _isAxisHomed;

        public override bool GoHome(out string message)
        {
            message = "回零成功";
            _isAxisHomed = true;
            return true;
        }

        protected override void DoClose()
        {
           
        }

        protected override void DoConfigure()
        {
           
        }

        protected override bool DoGetAxisEnable(AxisType axisType)
        {
            return true;
        }

        protected override bool DoGetAxisStopped(AxisType axisType)
        {
            return true;
        }

        protected override bool DoInitial()
        {
            ConfigProvider.ConfigChanged += () =>
            {
                //todo 控制卡参数发生变化时，在这里重新设置控制卡参数。
            };
            return true;
        }

        protected override bool DoMoveAxis(AxisType axisType, double distance)
        {
            Thread.Sleep((int)distance * 1);//1微米 1毫秒 用耗时模拟操作
            return true;
        }

        protected override void DoStop(AxisType? axisType, AxisStopMode 减速停止)
        {
            if (axisType.HasValue)
            {
                StopAxis(axisType.Value);
            }
            else
            {
                foreach (var axis in AxisTypes)
                {
                    StopAxis(axis);
                }
            }
        }

        private void StopAxis(AxisType value)
        {
            Thread.Sleep(500);//耗时模拟
        }
    }
}
