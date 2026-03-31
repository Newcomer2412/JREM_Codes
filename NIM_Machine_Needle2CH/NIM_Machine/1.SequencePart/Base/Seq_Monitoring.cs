using System;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 기타 모니터링
    /// </summary>
    public class Seq_Monitoring
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 시퀀스 스레드 동작 관리 클래스
        /// </summary>
        public CSeqProc Proc { get; set; }

        /// <summary>
        /// 생성자
        /// </summary>
        public Seq_Monitoring()
        {
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            Proc = new CSeqProc();
            Proc.SeqName = "ReadMonitoring";
        }

        /// <summary>
        /// 프로그램 종료
        /// </summary>
        private bool bClose = false;

        /// <summary>
        /// 시퀀스 소멸
        /// </summary>
        public void Free()
        {
            Proc.Free();
        }

        /// <summary>
        /// 스래드 시작
        /// </summary>
        public void Start()
        {
            Proc.Start(Do, ThreadPriority.Highest);
        }

        /// <summary>
        /// 스래드 정지
        /// </summary>
        public void Stop()
        {
            bClose = true;
            Proc.Stop();
        }

        /// <summary>
        /// 메인 Sequence 구동 스레드
        /// </summary>
        public bool Do()
        {
            if (bClose == true) return true;
            if (Get_System_Utility_IO_Check() == false) return false;
            if (Define.SIMULATION == false) SystemSwitch();
            Thread.Sleep(10);
            return false;
        }

        /// <summary>
        /// 물리 버튼이 중복 눌리는 것을 방지 플래그
        /// </summary>
        private bool m_bCheckStartSwitch = false;

        /// <summary>
        /// System 버튼 클릭
        /// </summary>
        public void SystemSwitch()
        {
            if (m_bCheckStartSwitch == false)
            {
                m_bCheckStartSwitch = true;
                //if (ml.Seq.SeqIO.GetInput((int)eIO_I.START_SW) == true)
                //{
                //    ml.McStart();        // 자동 운전 시작
                //    Thread.Sleep(700);
                //}
                //else if (ml.Seq.SeqIO.GetInput((int)eIO_I.STOP_SW) == true)
                //{
                //    ml.McStop();     // 자동 운전 정지
                //    Thread.Sleep(700);
                //}
                //else if (ml.Seq.SeqIO.GetInput((int)eIO_I.RESET_SW) == true)
                //{
                //    ml.McReset();   // 장비 초기화
                //    Thread.Sleep(700);
                //}
            }
        }

        /// <summary>
        /// 반복적으로 IO 등 체크하는 함수
        /// </summary>
        /// <returns></returns>
        public bool Get_System_Utility_IO_Check()
        {
            if (ml.cVar.bSystemUtilityIOCheck == false) return false;

            // MC 전원 Check
            if (Get_MCPower() == false)
            {
                if (ml.McState != eMachineState.ERROR)
                {
                    // Alarm : MC Power Off
                    ml.AddError(eErrorCode.MC_POWER_OFF);
                }
                ml.cVar.bMCPowerFlag = false;
                ml.cVar.bInitializeComplete = false;
                return false;
            }
            else
            {
                if (ml.cVar.bMCPowerFlag == false)
                    ml.cVar.bMCPowerFlag = true;
            }

            if (Define.SIMULATION == false)
            {
                bool bInput1 = false, bInput2 = false, bInput3 = false;
                ml.Seq.SeqIO.GetDirectInput((int)eIO_I.EMERGENCY_SW_1_OFF_FRONT_RIGHT, ref bInput1);
                ml.Seq.SeqIO.GetDirectInput((int)eIO_I.EMERGENCY_SW_2_OFF_REAR, ref bInput2);
                ml.Seq.SeqIO.GetDirectInput((int)eIO_I.EMERGENCY_SW_3_OFF_FRONT_LEFT, ref bInput3);
                // 비상정지 Check
                if (bInput1 == true ||
                    bInput2 == true ||
                    bInput3 == true)
                {
                    if (ml.McState != eMachineState.ERROR)
                    {
                        // IO 비상정지
                        Set_IO_System_Close();
                        // 전축 모터 비상정지
                        Set_AllMotor_EmgStop();
                        // 전 시퀀스 비상정지
                        ml.Seq.EMGStop();
                        // Alarm : 비상정지
                        ml.AddError(eErrorCode.EMG_BUTTON_INPUT);
                    }
                    ml.cVar.bInitializeMotor = false;
                    ml.cVar.bInitializeComplete = false;
                    return false;
                }
            }

            // Motor Alarm Check
            if (Get_Motor_Alarm_Check() == false) return false;

            // 공압 Check
            if (Define.SIMULATION == false &&
                ml.Seq.SeqIO.GetInput((int)eIO_I.MAIN_AIR) == false)
            {
                if (ml.McState != eMachineState.ERROR)
                {
                    // Alarm : 공압 저하
                    ml.AddError(eErrorCode.MC_AIR_OFF);
                }
                ml.cVar.bMainAirFlag = false;
                return false;
            }
            else
            {
                if (ml.cVar.bMainAirFlag == false)
                    ml.cVar.bMainAirFlag = true;
            }

            return true;
        }

        /// <summary>
        /// Power Check
        /// </summary>
        /// <returns></returns>
        public bool Get_MCPower()
        {
            if (ml.Seq.SeqIO.GetInput((int)eIO_I.MOTOR_POWER_MC1_ON) == true &&
                ml.Seq.SeqIO.GetInput((int)eIO_I.CONTROL_POWER_MC2_ON) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 시스템 종료 시 IO 상태
        /// </summary>
        public void Set_IO_System_Close()
        {
            // 부져 Off
            ml.Seq.SeqIO.SetOutput((int)eIO_O.TOWER_LAMP_BUZZER, false);
        }

        /// <summary>
        /// EMG 정지
        /// </summary>
        public void Set_AllMotor_EmgStop()
        {
            for (int iAxis = 0; iAxis < ml.Axis.Count; iAxis++)
            {
                ml.Axis[(eMotor)iAxis].EmergencyStop();
            }
        }

        /// <summary>
        /// Motor Alarm 체크
        /// </summary>
        /// <returns></returns>
        public bool Get_Motor_Alarm_Check()
        {
            if (ml.McState == eMachineState.ERROR) return true;

            int iAlarmMotorAxisNumber = 0;
            bool bStatus = false;

            for (int i = 0; i < ml.Axis.Count; i++)
            {
                if (bStatus == false &&
                   (ml.Axis[(eMotor)i].GetServoState() == false ||
                    ml.Axis[(eMotor)i].GetServoReady() == false ||
                    ml.Axis[(eMotor)i].GetAlarm() == true ||
                    ml.Axis[(eMotor)i].GetControllerFault() == true))
                {
                    iAlarmMotorAxisNumber = i;
                    bStatus = true;
                }
            }

            if (bStatus == true &&
                ml.McState != eMachineState.ERROR &&
                ml.cVar.bInitializeMotor == true)
            {
                // 모터 다시 초기화하도록 플래그 변경
                ml.cVar.bInitializeMotor = false;
                //ml.cVar.bInitializeComplete = false;  //이게 false 라면 Error Reset이 안됨
                // 모터 알람 발생
                ml.AddError(eErrorCode.MOTOR0_ERROR + iAlarmMotorAxisNumber);
                // 전축 모터 비상정지
                Set_AllMotor_EmgStop();
                return false;
            }

            return true;
        }
    }
}