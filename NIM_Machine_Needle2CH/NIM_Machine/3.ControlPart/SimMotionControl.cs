using System;
using System.Diagnostics;
using System.Timers;

namespace MachineControlBase
{
    /// <summary>
    /// 시물레이션 이동 종류
    /// </summary>
    public enum SimMoveType : int
    {
        /// <summary>
        /// 절대위치 이동
        /// </summary>
        ABS_Move = 1,

        /// <summary>
        /// 상태위치 이동
        /// </summary>
        REL_Move = 2,
    }

    /// <summary>
    /// 모터 시뮬레이션 이동 클래스
    /// </summary>
    public class SimMotionControl
    {
        /// <summary>
        /// 이동할 총 거리 (단위 mm) (시뮬레이션)
        /// </summary>
        private double simMoveDistance = 0.0;

        /// <summary>
        /// 모터 시작 포지션 (시뮬레이션)
        /// </summary>
        private double simStartPosition = 0.0;

        /// <summary>
        /// 모터 도착 포지션 (시뮬레이션)
        /// </summary>
        private double simFinishPosition = 0.0;

        /// <summary>
        /// 계산된 이동 시간 (시뮬레이션)
        /// </summary>
        private long simMoveTime = 0;

        /// <summary>
        /// 이동시간 중 증가 또는 감소시킬 단위 값 (0.1초 단위로 타이머에 의해 update 된다) (시뮬레이션)
        /// </summary>
        private double simIncValue = 0;

        /// <summary>
        /// 엔코더값을 증가 시켜야 하는지, 감소시켜야 하는지 여부 (시뮬레이션)
        /// </summary>
        private bool simIsIncRealCounter = true;

        /// <summary>
        /// 축 파라미터 정보
        /// </summary>
        private AxisParam axisParam;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="axisParam"></param>
        public SimMotionControl(AxisParam axisParam)
        {
            this.axisParam = axisParam;
        }

        /// <summary>
        /// 시뮬레이션 Home 진행
        /// </summary>
        /// <returns></returns>
        public bool SimHome()
        {
            axisParam.dSimCmdPos = 0;
            return true;
        }

        /// <summary>
        /// 시뮬레이션 모터 이동 함수
        /// </summary>
        /// <param name="simMoveType"></param>
        /// <param name="dPos"></param>
        /// <param name="uiSpeed"></param>
        /// <returns></returns>
        public bool SimMotorMove(SimMoveType simMoveType, double dPos, uint uiSpeed)
        {
            // 먼저 현재 위치가 모터 시작 포지션이 된다.
            simStartPosition = axisParam.dSimCmdPos;

            // 절대값 및 상대값에 따라 이동거리 계산
            if (simMoveType == SimMoveType.ABS_Move)
            {
                // 목적지 - 현재위치 = 절대값 이동해야될 거리
                simMoveDistance = dPos - simStartPosition;
                // 목적지 위치를 변수에 담는다.
                simFinishPosition = dPos;
            }
            else
            {
                // 현재위치 + 목적지 = 상대값 이동해야될 거리
                simMoveDistance = dPos;
                // 목적지 위치를 변수에 담는다.
                simFinishPosition = simStartPosition + dPos;
            }

            // 이동 시간 계산
            double simSpeedValue = (double)((double)axisParam.uiVel * (double)CMainLib.Ins.cSysOne.iAllAutoRatio / 100 * (double)uiSpeed / 100);
            double simMoveValue = (simMoveDistance / simSpeedValue) * 100;
            simMoveTime = (long)Math.Round(simMoveValue, 3);
            // 이동 시간 기준으로 증가값 설정
            simIncValue = simMoveDistance / (double)(simMoveTime) * 10;

            // 엔코더 값 증가 및 감소 설정
            // true면 증가, flase면 감소
            // 시작값 보다 도착값이 클경우 증가
            // 시작값 보다 도착값이 작을경우 감소
            if (simStartPosition < simMoveDistance)
            {
                simIsIncRealCounter = true;
            }
            else
            {
                // 도착값이 음수일 경우 감소
                if (simMoveDistance >= 0)
                    simIsIncRealCounter = true;
                else
                    simIsIncRealCounter = false;
            }

            // 모터 이동 엔코더 타이머 함수
            SimEncoderTimer();

            return true;
        }

        /// <summary>
        /// 0.1초 단위의 Interval Timer를 생성한다. (시뮬레이션)
        /// </summary>
        private Timer simMoveTimer = null;

        /// <summary>
        /// 모터 시뮬레이션 이동을 확인하기 위한 타이머 (시뮬레이션)
        /// </summary>
        private Stopwatch SimMotionTimer = new Stopwatch();

        /// <summary>
        /// 모터 엔코더 이동 타이머를 시작한다. (0.1초마다 이벤트 함수를 실행시킨다) (시뮬레이션)
        /// </summary>
        private void SimEncoderTimer()
        {
            // 10ms 마다 엔코더 증가하는 이벤트를 실행
            if (simMoveTimer == null)
            {
                simMoveTimer = new Timer(100.0);
                simMoveTimer.Elapsed += new ElapsedEventHandler(SimMoveTimer_Elapsed);
            }

            axisParam.bSimMoveDone = false;
            SimMotionTimer.Restart();
            simMoveTimer.Enabled = true;
        }

        /// <summary>
        /// 모터 엔코더 이동 타이머 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimMoveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 엔코더 값 증가 및 감소 부분
            if (simIsIncRealCounter == true)
                simStartPosition += simIncValue;
            else
                simStartPosition -= simIncValue;

            // 증가된 값을 실시간으로 담는다.
            axisParam.dSimCmdPos = Math.Round(simStartPosition, 3);

            // 모션이동 타임아웃 에러 방지를 위해, 타임아웃 리미트 값에 100을 뺀다.
            int iMotorTimeCheck = CMainLib.Ins.cSysOne.iMotorTimeOut - 100;

            // 타이머 시간이 이동 시간을 초과할 경우 모터 도착 완료.
            if (SimMotionTimer.ElapsedMilliseconds > Math.Abs(simMoveTime) ||
                SimMotionTimer.ElapsedMilliseconds > iMotorTimeCheck)
            {
                axisParam.dSimCmdPos = simFinishPosition;
                SimMotionTimer.Stop();
                simMoveTimer.Enabled = false;
                axisParam.bSimMoveDone = true;
            }
        }

        /// <summary>
        /// 시뮬레이션 모터 도착 확인 함수
        /// </summary>
        /// <returns></returns>
        public bool SimMoveDone()
        {
            return axisParam.bSimMoveDone;
        }
    }
}