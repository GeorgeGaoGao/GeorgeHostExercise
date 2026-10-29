using ADT_CARD_632XE;
using Company.Logger;
using DEMO632XE;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.ControlCard.AdTech
{
    public class ControlCardAdTech : ControlCardBase
    {
        private int _cardNo;//控制卡卡号
        private readonly ControlCardBit 急停=new ControlCardBit(2,1);
        public override bool IsAxisHomed => throw new NotImplementedException();

        public override bool GoHome(out string message)
        {
            throw new NotImplementedException();
        }

        protected override void DoClose()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 配置控制卡
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected override void DoConfigure()
        {
            int result = 0;
            int board = 0;
            int logic = 0;

            
            //参  数：  cardno      --  可用控制卡卡号
            //          axis        --  轴号, [1,32], 上限以当前系统实际连接总线伺服数为准
            //          mode        --  信号类型, [0,4], 0:正限位, 1:负限位, 2:原点, 3:保留, 4:急停(此时axis无实际意义)
            //          enable      --  使能标识, [0,1], 0:关闭, 1:使能
            //          logic       --  有效电平, [0,1], 0:低电平有效, 1:高电平有效
            //          admode      --  停止模式, [0,1], 0:减速停止, 1:立即停止
            //备  注：	1. 默认正负限位低电平有效, 原点/紧急停止信号无效
            //          2. 正负限位和紧急停止信号仅支持立即停止模式

            //设置与获取原点/限位/急停信号输入端口映射
            result += adt_card_632xe.adt_set_axis_io_map(_cardNo, 0, 4, 急停.Board,急停.Port);
            //设置与获取原点/限位/急停信号模式与有效电平
            result += adt_card_632xe.adt_set_hardlimit_mode(_cardNo, 0, 4, 1, 1, 1);

            foreach (var axisType in AxisTypes)
            {
                int axis = (int)axisType;

                //正限位
                result += adt_card_632xe.adt_set_axis_io_map(_cardNo, axis, 0, board, 0);
                result += adt_card_632xe.adt_set_hardlimit_mode(_cardNo, axis, 0, 1, logic, 1);
                //负限位
                result += adt_card_632xe.adt_set_axis_io_map(_cardNo, axis, 1, board, 1);
                result += adt_card_632xe.adt_set_hardlimit_mode(_cardNo, axis, 1, 1, logic, 1);
                //原点信号
                result += adt_card_632xe.adt_set_axis_io_map(_cardNo, axis, 2, board, 2);
                result += adt_card_632xe.adt_set_hardlimit_mode(_cardNo, axis, 2, 1, logic, 0);
                //设置脉冲当量编程模式
                result += adt_card_632xe.adt_set_unit_mode(_cardNo, axis, 0);
                result += adt_card_632xe.adt_set_pulse_equiv(_cardNo, axis, 1000);
                //
            }
        }

        protected override bool DoGetAxisEnable(AxisType axisType)
        {
            throw new NotImplementedException();
        }

        protected override bool DoGetAxisStopped(AxisType axisType)
        {
            throw new NotImplementedException();
        }

        protected override bool DoInitial()
        {
            try
            {
                int cardCount;

                int result = adt_card_632xe.adt_initial(out cardCount, 0);

                if (result != 0)
                {
                    Logs.LogError($"adt_card_632xe 控制卡初始化失败。{adt_global.adt_decode_error_code(result)}");
                    return false;
                }
                if (cardCount != 1)
                {
                    Logs.LogError($"adt_card_632xe 控制卡数量不等于1。{adt_global.adt_decode_error_code(result)}");
                    return false;
                }

                int[] cardIndexes = new int[10];//设置卡号的索引
                adt_card_632xe.adt_get_card_index(out cardCount, cardIndexes);
                _cardNo = cardIndexes[0];

                if (StartControlCard())//启动控制卡
                {
                    return true;
                }

            }
            catch (Exception ex)
            {

                Logs.LogError(ex);
                return false;
            }
            return false;
        }
        /// <summary>
        /// 启动控制卡
        /// </summary>
        /// <returns></returns>
        private bool StartControlCard()
        {
            adt_card_632xe.adt_ecat_stop(_cardNo);//停止总线通讯
            int result= adt_card_632xe.adt_ecat_load_flash_cfg(_cardNo);//加载总线参数
            if (result!=0)
            {
                Logs.LogError($"adt_card_632xe 控制卡加载总线参数失败，{adt_global.adt_decode_error_code(result)}");
                return false;
            }
            result = adt_card_632xe.adt_ecat_start(_cardNo);
            if (result!=0)
            {
                Logs.LogError($"adt_card_632xe 控制卡启动总线通讯失败，{adt_global.adt_decode_error_code(result)}");
            }

            return true;
        }

        protected override bool DoMoveAxis(AxisType axisType, double distance)
        {
            throw new NotImplementedException();
        }

        protected override void DoStop(AxisType? axisType, AxisStopMode 减速停止)
        {
            throw new NotImplementedException();
        }
    }
}
