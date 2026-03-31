using System;

namespace MachineControlBase
{
    /// <summary>
    /// 아날로그 컨버터
    /// 아날로그를 받아 기준값을 설정하고 비율로 계산한다.
    /// </summary>
    public class AnalogConverter
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="eChannel"></param>
        /// <param name="dZeroVolt"></param>
        /// <param name="dConvertValue"></param>
        public AnalogConverter(eAnalogInput eChannel, double dZeroVolt, double dConvertValue)
        {
            this.eChannel = eChannel;
            this.dZeroVolt = dZeroVolt;
            this.dConvertValue = dConvertValue;
        }

        /// <summary>
        /// 아날로그 입력 채널
        /// </summary>
        private eAnalogInput eChannel = 0;

        /// <summary>
        /// 제로 기준 측정 값
        /// </summary>
        private double dZeroVolt = 0.0;

        /// <summary>
        /// 변환 기준 값
        /// </summary>
        private double dConvertValue = 1.0;

        /// <summary>
        /// Zero Volt 값을 설정
        /// </summary>
        /// <param name="dZeroVolt"></param>
        public void SetZeroVolt(double dZeroVolt)
        {
            this.dZeroVolt = dZeroVolt;
        }

        /// <summary>
        /// Convert Value 값을 설정한다.
        /// </summary>
        /// <param name="dConvertValue"></param>
        public void SetConvertValue(double dConvertValue)
        {
            this.dConvertValue = dConvertValue;
        }

        /// <summary>
        /// 아날로그 Volt 변환 값을 리턴(아날로그 한번 읽음)
        /// </summary>
        /// <returns></returns>
        public double GetOneVoltToConvert()
        {
            if (Define.SIMULATION == true) return 0;
            double dResult = AjinADDACtrlLib.Ins.GetDirectADValue((int)eChannel);
            return Math.Round((dResult - dZeroVolt) * dConvertValue, 3);
        }

        /// <summary>
        /// 아날로그 Volt 변환 값을 리턴(아날로그 100번 읽어 평균 값 읽음)
        /// </summary>
        /// <returns></returns>
        public double GetRepeatVoltToConvert()
        {
            if (Define.SIMULATION == true) return 0;
            double dResult = AjinADDACtrlLib.Ins.GetADValue((int)eChannel);
            return Math.Round((dResult - dZeroVolt) * dConvertValue, 3);
        }
    }
}