namespace MachineControlBase
{
    /// <summary>
    /// 미쓰비시 스카라 통신 프로토콜
    /// </summary>
    public class MitsubishiScara
    {
        /// <summary>
        /// 클라이언트 통신
        /// </summary>
        public CTCPAsyncClient cTCPClient = null;

        /// <summary>
        /// 통신 응답 데이터
        /// </summary>
        private string[] strCommRtn = null;

        /// <summary>
        /// 전송한 문자열
        /// </summary>
        private string strSendData = string.Empty;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            cTCPClient = new CTCPAsyncClient();
        }

        public void Free()
        {
            // 통신 종료
            if (cTCPClient != null)
            {
                cTCPClient.Disconnect();
                cTCPClient.CloseTCPClient();
            }
        }

        /// <summary>
        /// 스카라 번호
        /// </summary>
        private int iNo = 0;

        /// <summary>
        /// 로그 타입 설정
        /// </summary>
        private eLogType eLogType;

        /// <summary>
        /// 서버 IP 주소
        /// </summary>
        private string strIP = null;

        /// <summary>
        /// 통신을 연결합니다.
        /// </summary>
        /// <param name="iNo"></param>
        /// <param name="eLogType"></param>
        /// <param name="strIP"></param>
        /// <param name="uiPort"></param>
        public void Connect(int iNo, eLogType eLogType, string strIP, uint uiPort)
        {
            // TCP Client Start
            this.iNo = iNo;
            this.eLogType = eLogType;
            this.strIP = strIP;
            cTCPClient.SetLog(NLogger.GetLogClass(eLogType));
            if (cTCPClient.Connect(strIP, uiPort,
                                   bServerConnectStatus,
                                   OnVisionReceiveClient
                                   ) == false)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} MitsubishiScara Not Server Connect");
            }
        }

        public event TCPConnectStatus_EventHandler TCPConnectStatus;

        /// <summary>
        /// Server와 연결 상태
        /// </summary>
        /// <param name="bStatus"></param>
        public void bServerConnectStatus(bool bStatus)
        {
            if (bStatus == true)
            {
                TCPConnectStatus?.Invoke(true);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has connected to the MitsubishiScara server.");
            }
            else
            {
                if (CMainLib.Ins.McState == eMachineState.RUN) CMainLib.Ins.AddError((eErrorCode)iNo);  // 에러 번호 정의 필요.
                TCPConnectStatus?.Invoke(false);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has been disconnected from the MitsubishiScara server.");
            }
        }

        /// <summary>
        /// 데이터 전달
        /// </summary>
        /// <param name="strSendData"></param>
        /// <returns></returns>
        public bool SendData(string strSendData)
        {
            return cTCPClient.SendData(strSendData);
        }

        /// <summary>
        /// Staos1 이송
        /// </summand Pry>
        /// <returns></returns>
        public void Go_Stand1()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_STAND_BY1";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// Stand Pos2 이송
        /// </summary>
        /// <returns></returns>
        public void Go_Stand2()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_STAND_BY2";
            cTCPClient.SendData(strSendData + "\r");
        }

        #region Megazine Rack Scara

        /// <summary>
        /// Magazine Rack Pickup 위치 이동
        /// </summary>
        /// <returns></returns>
        public void GO_MZ_RACK_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_MZ_RACK_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// Magazine Rack Pickup Down 위치 이동
        /// </summary>
        /// <returns></returns>
        public void GO_MZ_RACK_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_MZ_RACK_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 하부 2D  레이저 및 바코드 Read부 Rack 공급부 이동
        /// </summary>
        /// <returns></returns>
        public void GO_LASER_BARCORD_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_LASER_BARCORD_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 하부 2D  레이저 및 바코드 Read부 Rack Place
        /// </summary>
        /// <returns></returns>
        public void GO_LASER_BARCORD_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_LASER_BARCORD_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 하부 2D  레이저 및 바코드 Rack Align 후 재 Plckup(Offset 위치 이동 후)
        /// </summary>
        /// <returns></returns>
        public void GO_LASER_BARCORD_OFFSET_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_LASER_BARCORD_OFFSET_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 랙 방향 180도 회전 이동
        /// </summary>
        public void GO_LASER_BARCORD_RE_180_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_LASER_BARCORD_RE_180_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 사이드 레이저 마킹 위치로 이동
        /// </summary>
        public void GO_LASER_BARCORD_MARKING_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_LASER_BARCORD_MARKING_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 사이드 로고 마킹 위치로 이동
        /// </summary>
        public void GO_LASER_LOGO_MARKING_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_LASER_LOGO_MARKING_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 바코드 촬영 위치로 이동
        /// </summary>
        public void GO_BARCORD_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_BARCORD_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 빈 Rack 배출 위치로 이동
        /// </summary>
        /// <returns></returns>
        public void GO_OUT_RACK_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_OUT_RACK_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 빈 Rack 배출 위치 Pickup위치 Z축 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_OUT_RACK_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_OUT_RACK_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 바코드 에러 NG RacK 공급으로 이동
        /// </summary>
        /// <returns></returns>
        public void GO_NG_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_NG_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 바코드 에러 NG RacK 공급 Z축 포지션 1 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_NG_N1_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_NG_N1_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 바코드 에러 NG RacK 공급 Z축 포지션 2 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_NG_N2_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_NG_N2_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 바코드 에러 NG RacK 공급 Z축 포지션 3 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_NG_N3_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_NG_N3_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        #endregion Megazine Rack Scara

        #region Rack Scara

        /// <summary>
        /// #1 Rack  공급 & 회수 T0 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N1_T0_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N1_0_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #1 Rack  공급 & 회수 T180 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N1_T180_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N1_180_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #1 Rack  공급 & 회수 위치 Z축 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N1_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N1_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #2 Rack  공급 & 회수 위치 T0 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N2_T0_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N2_0_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #2 Rack  공급 & 회수 위치 180 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N2_T180_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N2_180_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #2 Rack  공급 & 회수 위치 Z축 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N2_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N2_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #3 Rack  공급 & 회수 위치 T0 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N3_T0_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N3_0_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #3 Rack  공급 & 회수 위치 T180 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N3_T180_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N3_180_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #3 Rack  공급 & 회수 위치 Z축 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N3_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N3_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #4 Rack  공급 & 회수 위치 T0 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N4_T0_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N4_0_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #4 Rack  공급 & 회수 위치 T180 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N4_T180_POS()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N4_180_POS";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// #4 Rack  공급 & 회수 위치 Z축 하강 이동
        /// </summary>
        /// <returns></returns>
        public void GO_N4_POS_DOWN()
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "GO_N4_POS_DOWN";
            cTCPClient.SendData(strSendData + "\r");
        }

        /// <summary>
        /// 내부 속도 변경(외부 속도(TB)는 TB에서 설정하거나 멜파로 속도 변경)
        /// </summary>
        /// <returns></returns>
        public void Set_Velocity(int iVel)
        {
            if (cTCPClient.bConncect == false) return;
            strCommRtn = null;
            strSendData = "SET_VEL," + iVel.ToString();
            cTCPClient.SendData(strSendData + "\r");
        }

        #endregion Rack Scara

        private static object ReceiveLock = new object();

        /// <summary>
        /// 응답와서 모션 완료 상태
        /// </summary>
        /// <returns></returns>
        public bool IsMoveDone()
        {
            if (strCommRtn != null)
            {
                if (strCommRtn[0] == strSendData &&
                   strCommRtn[1] == "OK") return true;
                else return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 서버에서 클라이언트로 보내온 데이터
        /// </summary>
        /// <param name="strReceiveData"></param>
        private void OnVisionReceiveClient(string strReceiveData)
        {
            try
            {
                lock (ReceiveLock)
                {
                    strReceiveData = strReceiveData.Substring(0, strReceiveData.Length - 1);
                    string[] receiveText_Split_Main = strReceiveData.Split(',');
                    strCommRtn = receiveText_Split_Main;
                }
            }
            catch
            {
                NLogger.AddLog(eLogType, NLogger.eLogLevel.FATAL, "Client receive exception : " + strReceiveData);
            }
        }
    }
}