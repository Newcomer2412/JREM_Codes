using System;
using System.Diagnostics;
using System.Threading;

namespace MachineControlBase
{
    public class Seq_Initilize
    {
        /// <summary>
        /// Main Library
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 초기화 스레드
        /// </summary>
        private Thread cThread;

        /// <summary>
        /// 스레드 반복 플래그
        /// </summary>
        private bool bThread;

        /// <summary>
        /// UI에 보낼 이벤트
        /// </summary>
        public delegate void InitializingEndedHandler();

        public event InitializingEndedHandler InitializingEnded;

        /// <summary>
        /// Main Step No
        /// </summary>
        public int iStep;

        /// <summary>
        /// UI에 보여줄 Msg
        /// </summary>
        public string strMsgState;

        public Seq_Initilize()
        {
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// 다음 이동 시퀀스 설정
        /// </summary>
        /// <param name="iStep"></param>
        public void Next(int iStep)
        {
            this.iStep = iStep;
        }

        /// <summary>
        /// 초기화 시작
        /// </summary>
        public void Start()
        {
            bThread = true;
            iStep = 0;
            cThread = new Thread(new ThreadStart(DoInitialize));
            cThread.IsBackground = true;
            cThread.Start();
        }

        /// <summary>
        /// 초기화 정지
        /// </summary>
        public void InitStop()
        {
            bThread = false;
            strMsgState = "Initailize Stop";
        }

        /// <summary>
        /// 타임아웃 스톱워치
        /// </summary>
        private Stopwatch cTimeOut = new Stopwatch();

        /// <summary>
        /// 스레드 함수
        /// </summary>
        public void DoInitialize()
        {
            while (bThread == true)
            {
                Thread.Sleep(10);
                switch (iStep)
                {
                    case 0:
                        {
                            if (Define.SIMULATION == false &&
                                ml.Seq.SeqIO.GetInput((int)eIO_I.MAIN_AIR) == false)    //실린더 작동을 위한 공압 확인
                            {
                                Error("initialize cancel because air is not supply.");
                            }

                            strMsgState = "Motion IO Module Connect Success";
                            Next(10);
                        }
                        break;

                    case 10:
                        {
                            strMsgState = "IO Initilize Success";
                            Next(11);
                        }
                        break;

                    //서보 알람 확인
                    case 11:
                        {
                            strMsgState = "Servo Alarm Check";
                            for (int i = 0; i < ml.Axis.Count; i++)
                            {
                                if (ml.Axis[(eMotor)i].GetAlarm() == true ||
                                    ml.Axis[(eMotor)i].GetControllerFault() == true)
                                {
                                    ml.Axis[(eMotor)i].Reset();
                                }
                            }
                            cTimeOut.Restart();
                            Next(12);
                        }
                        break;

                    // 서보 Ready 확인
                    case 12:
                        {
                            strMsgState = "Servo Ready Check";
                            bool bCheckOK = true;
                            for (int i = 0; i < ml.Axis.Count; i++)
                            {
                                if (ml.Axis[(eMotor)i].GetServoReady() == false)
                                {
                                    bCheckOK = false;
                                }
                            }
                            if (cTimeOut.ElapsedMilliseconds > 5000)
                            {
                                cTimeOut.Stop();
                                string strMessage = "Servo Ready Check Time Out";
                                Error(eErrorCode.CONTROLLER_ERROR, strMessage, "");
                                return;
                            }
                            if (bCheckOK == true)
                            {
                                cTimeOut.Stop();
                                Next(15);
                            }
                        }
                        break;

                    case 15:
                        {
                            Before_init();
                            strMsgState = "All Var, IO Start Setting Success";
                            Next(20);
                        }
                        break;

                    //서보 ON 확인
                    case 20:
                        {
                            strMsgState = "Checking All Axis Servo On";
                            for (int i = 0; i < ml.Axis.Count; i++)
                            {
                                if (ml.Axis[(eMotor)i].GetServoState() == false)
                                {
                                    string strMessage = "Servo On Fail";
                                    Error(eErrorCode.MOTOR0_ERROR + i, strMessage, "", (eMotor)i);
                                    return;
                                }
                            }
                            if (Define.SIMULATION == true) Next(50);
                            else Next(25);
                        }
                        break;

                    //서보 알람 확인
                    case 25:
                        {
                            strMsgState = "Checking All Axis Servo Error";
                            for (int i = 0; i < ml.Axis.Count; i++)
                            {
                                if (ml.Axis[(eMotor)i].GetAlarm() == true ||
                                    ml.Axis[(eMotor)i].GetControllerFault() == true)
                                {
                                    string strMessage = "Check Servo Error";
                                    Error(eErrorCode.MOTOR0_ERROR + i, strMessage, "", (eMotor)i);
                                    return;
                                }
                            }
                            Next(30);
                        }
                        break;

                    // 모든 Z축 Home
                    case 30:
                        {
                            strMsgState = "All Axis Z Moving Safe Position";
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                            ml.Axis[eMotor.DISPENSER_Z].Home();                 //모터위치를 저장할 수 있는 앱솔루트모터의 경우엔
                            ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute(0.0, 30);   //홈을 잡을 필요가 없기 때문에 MoveAbsolute 사용
                            ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute(0.0, 30);
                            ml.Axis[eMotor.NEEDLE_PnP_Z].MoveAbsolute(0.0, 30);
                            ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute(0.0, 30);
                            Next(31);
                        }
                        break;

                    //모든 Z축 Home 확인
                    case 31:
                        {
                            if (ml.Axis[eMotor.DISPENSER_Z].HomeDone() == true &&
                                ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true &&
                                ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true &&
                                ml.Axis[eMotor.NEEDLE_PnP_Z].IsMoveDone() == true &&
                                ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                            {
                                Next(40);
                            }
                        }
                        break;

                    //T축 Home
                    case 40:
                        {
                            strMsgState = "All Step Axis Homing";
                            ml.Axis[eMotor.PIPE_PnP_T].Home();
                            ml.Axis[eMotor.NEEDLE_PnP_T].Home();
                            ml.Axis[eMotor.PIPE_ROTATE].Home();
                            ml.Axis[eMotor.NEEDLE_ROTATE].Home();
                            Next(41);
                        }
                        break;

                    //T축 Home 확인
                    case 41:
                        {
                            if (ml.Axis[eMotor.PIPE_PnP_T].HomeDone() == true &&
                                ml.Axis[eMotor.NEEDLE_PnP_T].HomeDone() == true &&
                                ml.Axis[eMotor.PIPE_ROTATE].HomeDone() == true &&
                                ml.Axis[eMotor.NEEDLE_ROTATE].HomeDone() == true)
                            {
                                Next(42);
                            }
                        }
                        break;

                    case 42:
                        {
                            ml.Axis[eMotor.FLIP_Z].Home();
                            ml.Axis[eMotor.DISPENSER_X].Home();
                            Next(43);
                        }
                        break;

                    case 43:
                        {
                            if (ml.Axis[eMotor.FLIP_Z].HomeDone() == true &&
                                ml.Axis[eMotor.DISPENSER_X].HomeDone() == true)
                            {
                                Next(44);
                            }
                        }
                        break;

                    case 44:
                        {
                            ml.Axis[eMotor.FLIP_T].Home();
                            Next(45);
                        }
                        break;

                    case 45:
                        {
                            if (ml.Axis[eMotor.FLIP_T].HomeDone() == true)
                            {
                                Next(50);
                            }
                        }
                        break;

                    //MPC Left, Right Cylinder BWD
                    case 50:
                        {
                            strMsgState = "MPC Left, Right LM Cylinder Backward On";
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, true);
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, true);
                            cTimeOut.Restart();
                            Next(51);
                        }
                        break;

                    //MPC Left, Right Cylinder BWD 확인
                    case 51:
                        {
                            if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false &&
                                ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false)
                            {
                                cTimeOut.Stop();
                                Next(52);
                            }
                            else
                            {
                                if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == true)
                                    strMsgState = "MPC Left LM Rail Not Backward";
                                else if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true)
                                    strMsgState = "MPC Right LM Rail Not Backward";

                                if (cTimeOut.ElapsedMilliseconds > 1000) Next(50);
                            }
                        }
                        break;

                    //MPC Left, Right Front 위치로 이동
                    case 52:
                        {
                            if (ml.Axis[eMotor.MPC_LEFT_Y].MoveAbsolute((int)eAxisLeftMPC_Y.Front) &&
                                ml.Axis[eMotor.MPC_RIGHT_Y].MoveAbsolute((int)eAxisRightMPC_Y.Front) == true)
                            {
                                Next(53);
                            }
                        }
                        break;

                    //MPC Left, Right Front 위치로 이동 확인
                    case 53:
                        {
                            if (ml.Axis[eMotor.MPC_LEFT_Y].IsMoveDone() &&
                                ml.Axis[eMotor.MPC_RIGHT_Y].IsMoveDone() == true)
                            {
                                Next(60);
                            }
                        }
                        break;

                    //MPC Left, Right Cylinder FWD
                    case 60:
                        {
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, false);
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);
                            Next(61);
                        }
                        break;

                    //MPC Left, Right Cylinder FWD 확인
                    case 61:
                        {
                            if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false &&
                                ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false)
                            {
                                Next(62);
                            }
                            else
                            {
                                if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false)
                                    strMsgState = "MPC Left LM Rail Not Forward";
                                else if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false)
                                    strMsgState = "MPC Right LM Rail Not Forward";
                            }
                        }
                        break;

                    //UV 홀더 고정 실린더 Up
                    case 62:
                        {
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, false);
                            Next(63);
                            break;
                        }

                    //UV 홀더 고정 실린더 Up 확인
                    case 63:
                        {
                            if (ml.Seq.SeqIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON) == true)
                            {
                                Next(64);
                            }
                            else
                            {
                                strMsgState = "Rear MPC Holder Fix Cylinder Up Fail";
                            }
                        }
                        break;

                    //니들, 파이프 홀더 고정 실린더 Up
                    case 64:
                        {
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.FIXTURE_DOWN, false);
                            Next(65);
                        }
                        break;

                    //니들, 파이프 홀더 고정 실린더 Up 확인
                    case 65:
                        {
                            if (ml.Seq.SeqIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_UP_ON) &&
                                ml.Seq.SeqIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_UP_ON) == true)
                            {
                                Next(66);
                            }
                            else
                            {
                                strMsgState = "Front MPC Holder Fix Cylinder Up Fail";
                            }
                        }
                        break;

                    //  10mm 이동(Front 홈 잡는 방향 : +, Rear 홈 잡는 방향 : -)
                    case 66:
                        {
                            ml.Axis[eMotor.MPC_FRONT_X].MoveRelative(-10.0, 70);
                            ml.Axis[eMotor.MPC_REAR_X].MoveRelative(-10.0, 70);
                            Next(67);
                        }
                        break;

                    case 67:
                        {
                            if (ml.Axis[eMotor.MPC_FRONT_X].IsMoveDone() &&
                                ml.Axis[eMotor.MPC_REAR_X].IsMoveDone() == true)
                            {
                                Next(68);
                            }
                        }
                        break;

                    //Front, Rear X 홈 동작
                    case 68:
                        {
                            ml.Axis[eMotor.MPC_FRONT_X].Home();
                            ml.Axis[eMotor.MPC_REAR_X].Home();
                            Next(69);
                        }
                        break;

                    case 69:
                        {
                            if (ml.Axis[eMotor.MPC_FRONT_X].HomeDone() == true &&
                                ml.Axis[eMotor.MPC_REAR_X].HomeDone() == true)
                            {
                                Next(70);
                            }
                        }
                        break;

                    //UV, 니들, 파이프 홀더 고정 실린더 Down
                    case 70:
                        {
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, true);
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            Next(71);
                        }
                        break;

                    //UV, 니들 ,파이프 홀더 고정 실린더 Down 확인
                    case 71:
                        {
                            bool A = ml.Seq.SeqIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON);

                            bool B = ml.Seq.SeqIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_DOWN);
                            bool C = ml.Seq.SeqIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_DOWN);

                            if (!A && B && C == true)
                            {
                                Next(72);
                            }
                            else Next(70);
                        }
                        break;

                    //MPC Left, Right Cylinder BWD
                    case 72:
                        {
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, true);
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, true);
                            //cTimeOut.Restart();
                            Next(73);
                        }
                        break;

                    //MPC Left, Right Cylinder BWD 확인
                    case 73:
                        {
                            bool A = ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT);
                            bool B = ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT);

                            if (A == false && B == false)
                            {
                                Next(74);
                            }
                            else Next(72);

                            //이대로 썼을 때 IO가 일치했음에도 넘어가지 않는 버그가 있었음 한번 변수로 저장 후 사용하니까 해결됨

                            //if (ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) &&
                            //    ml.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false)
                            //{
                            //    cTimeOut.Stop();
                            //    Next(74);
                            //}
                            //else if (cTimeOut.ElapsedMilliseconds > 1000) Next(72);
                        }
                        break;

                    case 74:
                        {
                            double dMPC_Left_Y_InitPos = 0.0; double dMPC_Right_Y_InitPos = 0.0;

                            // MPC 왼쪽 레일 이니셜할 위치 확인
                            if (ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT).GetStatus(eStatus.NONE) == false)
                            {
                                dMPC_Left_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Rear);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetUnitNo(0).eStatus == eStatus.EMPTY ||
                                     ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetUnitNo(0).eStatus == eStatus.MOUNT ||
                                     ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetUnitNo(1).eStatus == eStatus.UV)
                            {
                                dMPC_Left_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetUnitNo(0).eStatus == eStatus.HOLDER)
                            {
                                dMPC_Left_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetAllStatus(eStatus.NONE) == true &&
                                     ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT).GetStatus(eStatus.NONE) == true)
                            {
                                dMPC_Left_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Rear);
                            }

                            // MPC 오른쪽 레일 이니셜할 위치 확인
                            if (ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT).GetStatus(eStatus.NONE) == false)
                            {
                                dMPC_Right_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_RIGHT_Y, (int)eAxisRightMPC_Y.Rear);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT).GetStatus(eStatus.NONE) == false)
                            {
                                dMPC_Right_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_RIGHT_Y, (int)eAxisRightMPC_Y.Front);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT).GetStatus(eStatus.NONE) == true &&
                                     ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT).GetStatus(eStatus.NONE) == true)
                            {
                                dMPC_Right_Y_InitPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_RIGHT_Y, (int)eAxisRightMPC_Y.Front);
                            }

                            ml.Axis[eMotor.MPC_LEFT_Y].MoveAbsolute(dMPC_Left_Y_InitPos, 30);
                            ml.Axis[eMotor.MPC_RIGHT_Y].MoveAbsolute(dMPC_Right_Y_InitPos, 30);
                            Next(75);
                        }
                        break;

                    case 75:
                        {
                            if (ml.Axis[eMotor.MPC_LEFT_Y].IsMoveDone() == true &&
                                ml.Axis[eMotor.MPC_RIGHT_Y].IsMoveDone() == true)
                            {
                                double dMPC_Y_Front = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
                                double dMPC_Y_Rear = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Rear);
                                double dMPC_Y_GetAct = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                                if (dMPC_Y_Front == dMPC_Y_GetAct ||
                                    dMPC_Y_Rear == dMPC_Y_GetAct)
                                {
                                    ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, false);
                                    ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);
                                    Next(80);
                                    break;
                                }

                                ml.Seq.SeqIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);
                                Next(80);
                            }
                        }
                        break;

                    case 80:    // 맵데이터에 따른 트랜스퍼 R축 해당 위치로 회전
                        {
                            // 파이프 R축 회전
                            if (ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).GetStatus(eStatus.EMPTY) == true ||
                                ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).GetStatus(eStatus.STANBY) == true)
                            {
                                ml.Axis[eMotor.PIPE_ROTATE].MoveAbsolute((int)eAxisPipeRotate.Origin);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true)
                            {
                                ml.Axis[eMotor.PIPE_ROTATE].MoveAbsolute((int)eAxisPipeRotate.Rotate90);
                            }

                            // 니들 R축 회전
                            if (ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.EMPTY) == true ||
                                ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.STANBY) == true)
                            {
                                ml.Axis[eMotor.NEEDLE_ROTATE].MoveAbsolute((int)eAxisNeedleRotate.Origin);
                            }
                            else if (ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true)
                            {
                                ml.Axis[eMotor.NEEDLE_ROTATE].MoveAbsolute((int)eAxisNeedleRotate.Rotate90);
                            }

                            Next(81);
                        }
                        break;

                    case 81:
                        {
                            if (ml.Axis[eMotor.PIPE_ROTATE].IsMoveDone() == true &&
                                ml.Axis[eMotor.NEEDLE_ROTATE].IsMoveDone() == true)
                            {
                                Next(82);
                            }
                        }
                        break;

                    case 82:
                        {
                            ml.Seq.SeqIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            Next(83);
                        }
                        break;

                    case 83:
                        {
                            if (ml.Seq.SeqIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_DOWN) == true &&
                                ml.Seq.SeqIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_DOWN) == true)
                            {
                                cTimeOut.Restart();
                                Next(90);
                            }
                            else
                            {
                                strMsgState = "Front MPC Holder Fix Cylinder Down Fail";
                            }
                        }
                        break;

                    case 90:
                        {
                            // 시뮬레이션 모드일 때, 실린더 Out Put 신호 초기화
                            SimCylinderClearOutput();
                            After_init();
                            Next(100);
                        }
                        break;

                    case 100:
                        {
                            strMsgState = "Initilize Finish!";
                            ml.cVar.bInitializeComplete = true;     // 장비 초기화 완료
                            ml.cVar.bInitializeMotor = true;        // 모터 초기화 완료
                            ml.Seq.SeqIO.SetMachineState(eMachineState.READY);
                            bThread = false;
                            Thread.Sleep(500);
                            if (InitializingEnded != null)
                                InitializingEnded();
                            Next(0);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 에러 발생 처리
        /// </summary>
        /// <param name="strErrorMsg"></param>
        private void Error(string strErrorMsg)
        {
            ml.cVar.bInitializeMotor = false; // 이니셜라이즈 실패
            bThread = false;
            if (InitializingEnded != null)
                InitializingEnded();
            ml.AddError(eErrorCode.MC_AIR_OFF);
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strErrorMsg);
        }

        /// <summary>
        /// 에러 발생 처리
        /// </summary>
        /// <param name="eAxis"></param>
        /// <param name="strErrorMsg"></param>
        /// <param name="strRtnMsg"></param>
        private void Error(eErrorCode errorCode, string strErrorMsg, string strRtnMsg = null, eMotor eAxis = eMotor.MOT_NONE)
        {
            ml.cVar.bInitializeMotor = false; // 이니셜라이즈 실패
            bThread = false;
            if (InitializingEnded != null)
                InitializingEnded();
            ml.AddError(errorCode);
            if (eAxis == eMotor.MOT_NONE)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format($"Home Error = {strErrorMsg}"));
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                           string.Format($"Home Error = [Axis {(int)eAxis}] {Enum.GetName(typeof(eMotor), eAxis)} {strErrorMsg}"));
            }

            if (strRtnMsg != null) NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtnMsg);
        }

        /// <summary>
        /// 초기화 전 설정 값
        /// </summary>
        private void Before_init()
        {
            // 장비 조명 끄고 켬
            ml.Seq.SeqIO.SetOutput((int)eIO_O.FLUORESCENT_LAMP_ONOFF, ml.cOptionData.bFluorescentUse);
            // init 표시
            ml.Seq.SeqIO.SetMachineState(eMachineState.INIT);
            // 제어기 리셋
            for (int i = 0; i < Enum.GetValues(typeof(eMotor)).Length - 1; i++)
            {
                ml.Axis[(eMotor)i].Reset();
            }
            Thread.Sleep(500);
            // Motor Power On
            for (int idx = 0; idx < ml.Axis.Count; idx++)
            {
                ml.Axis[(eMotor)idx].SetServoState(true);
            }
            Thread.Sleep(500);
        }

        /// <summary>
        /// 초기화 후 설정 값
        /// </summary>
        private void After_init()
        {
            // System Utility IO Check Start
            ml.cVar.bSystemUtilityIOCheck = true;
        }

        /// <summary>
        /// 시뮬레이션 모드일 때, 실린더 Out put 신호를 초기화 시켜준다.
        /// </summary>
        private void SimCylinderClearOutput()
        {
            if (Define.SIMULATION == true)
            {
                foreach (IOActuatorUI iOActuatorUI in ml.cDataEditUIManager.ListIOActuatorUI)
                {
                    ml.Seq.SeqIO.SetOutput(iOActuatorUI._iBWDOutput, true);
                }
            }
        }
    }
}