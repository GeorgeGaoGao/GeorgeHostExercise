using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Company.Hardware.ControlCard
{
    public abstract class ControlCardBase : IControlCard
    {
        public bool IsInitialized{ get; private set; }

        public bool IsReady => IsInitialized && IsInitialized;

        public abstract bool IsAxisHomed { get; }
        public SpeedMode SpeedMode { get; private set; }
        public ControlCardConfig ControlCardConfig { get; set; }

        protected IEnumerable<AxisType> AxisTypes { get; private set; } = Enum.GetValues<AxisType>().Cast<AxisType>();
        public void Close()
        {
            if (IsInitialized)
            {
                DoStop(null, AxisStopMode.减速停止);
                DoClose();
                IsInitialized = false;
            }
        }

       
        
        public IControlCardConfigProvider ConfigProvider { get; private set; }
        public bool Initialize(IControlCardConfigProvider controlCardConfigProvider)
        {
            ConfigProvider=controlCardConfigProvider;
            ControlCardConfig=ConfigProvider.ControlCardConfig;

            if (IsInitialized)
            {
                throw new Exception($"重复初始化控制卡。");
            }
            if (DoInitial())
            {
                IsInitialized = true;
                DoConfigure();
                return true;
            }
            return false;
        }

        

        public bool Move(AxisType axisType, double distance, out string message)
        {
            if (!IsReady)
            {
                message = $"运动轴未准备好。";
                return false;
            }
            bool result=MoveAxis(axisType, distance, out string v);
            message = v;
            return result;
        }

        private bool MoveAxis(AxisType axisType, double distance,out string v)
        {
            //判断当前轴是否开启使能
            if (!DoGetAxisEnable(axisType))
            {
                v = "当前轴未开启使能";
                return false;
            }
            //判断当前轴是否正在运动
            if (!DoGetAxisStopped(axisType))
            {
                v = "当前轴正在运动中";
                return false;

            }
            bool result=DoMoveAxis(axisType, distance);
            while (true)
            {
                if (DoGetAxisStopped(axisType))
                {
                    break;
                }
                Thread.Sleep(5);
            }
            v = "当前轴运动完成";
            return result;
        }

        
        public bool Move(AxisType axisType, MoveDirection moveDirection)
        {
            throw new NotImplementedException();
        }

        public void SetSpeedMode(SpeedMode speedMode)
        {
            SpeedMode = speedMode;
        }

        public void Stop(AxisType? axisType)
        {
            DoStop(axisType, AxisStopMode.减速停止);
        }
        protected abstract bool DoInitial();
        protected abstract void DoConfigure();

        protected abstract void DoClose();
        protected abstract void DoStop(AxisType? axisType, AxisStopMode 减速停止);
        public abstract bool GoHome(out string message);
        protected abstract bool DoMoveAxis(AxisType axisType, double distance);
        protected abstract bool DoGetAxisStopped(AxisType axisType);
        protected abstract bool DoGetAxisEnable(AxisType axisType);


    }
}
