namespace MachineControlBase
{
    /// <summary>
    /// Vision TCP 통신
    /// </summary>
    public class TCPVisionClient
    {
        /// <summary>
        /// 클라이언트 통신
        /// </summary>
        public CTCPAsyncClient cTCPClient = null;

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
        /// 통신을 연결합니다.
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="strIP"></param>
        /// <param name="uiPort"></param>
        public void Connect(eLogType eLogType, string strIP, uint uiPort)
        {
            // TCP Client Start
            cTCPClient.SetLog(NLogger.GetLogClass(eLogType));
            if (cTCPClient.Connect(strIP, uiPort,
                                   bServerConnectStatus,
                                   OnVisionReceiveClient
                                   ) == false)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP}, {uiPort} Vision Not Server Connect");
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
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "[Client] The client has connected to the vision server.");
            }
            else
            {
                if (CMainLib.Ins.McState == eMachineState.RUN) CMainLib.Ins.AddError(eErrorCode.VISION_CLIENT_NOT_CONNECT);
                TCPConnectStatus?.Invoke(false);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "[Client] The client has been disconnected from the vision server.");
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

        private static object ReceiveLock = new object();

        /// <summary>
        /// 서버에서 클라이언트로 보내온 데이터
        /// </summary>
        /// <param name="strReceiveData"></param>
        private void OnVisionReceiveClient(string strReceiveData)
        {
            //try
            //{
            //    lock (ReceiveLock)
            //    {
            //        string[] receiveText_Split_Main = strReceiveData.Split('@');
            //        for (int i = 0; i < receiveText_Split_Main.Length - 1; i++)
            //        {
            //            string[] receiveText_Split = receiveText_Split_Main[i].Split(',');

            //            // 카메라 작업 번호
            //            uint uiCameraNo;
            //            if (int.TryParse(receiveText_Split[0], out uiCameraNo) == true)
            //            {
            //                if (uiCameraNo >= 7 || uiCameraNo < 0)
            //                {
            //                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"CAM : {uiCameraNo} CAM_NUM_FAIL");
            //                    return;
            //                }
            //            }
            //            else
            //            {
            //                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"CAM : {uiCameraNo} CAM_NUM_FAIL");
            //                return;
            //            }

            //            int iToolBlockNo;
            //            if (int.TryParse(receiveText_Split[1], out iToolBlockNo) == true)
            //            {
            //                if (uiCameraNo == 0 || uiCameraNo == 1 || uiCameraNo == 2)
            //                {
            //                    if (iToolBlockNo >= 3 || iToolBlockNo < 0)
            //                    {
            //                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"CAM : {uiCameraNo} FUNC_NUM_FAIL");
            //                        return;
            //                    }
            //                }
            //            }

            //            int iGoodNg;
            //            int iToolBlock = 1;
            //            for (int j = 0; j < 4; j++)
            //            {
            //                if (uiCameraNo == 0 || uiCameraNo == 1 || uiCameraNo == 2) iToolBlock = 2;
            //                if (int.TryParse(receiveText_Split[(j + 1) * iToolBlock], out iGoodNg) == true)
            //                {
            //                    if (iGoodNg >= 2 || iGoodNg < 0)
            //                    {
            //                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"CAM : {uiCameraNo}, {j} GOOD_NG_FAIL");
            //                        return;
            //                    }
            //                    CMainLib.Ins.cVar.iVisionGoodNgResult[uiCameraNo, j] = iGoodNg;
            //                    if (iGoodNg == 1)
            //                    {
            //                        if (uiCameraNo == 0 || uiCameraNo == 1 || uiCameraNo == 2)
            //                        {
            //                            double dDegreeResult;
            //                            if (double.TryParse(receiveText_Split[((j + 1) * 2) + 1], out dDegreeResult) == true)
            //                            {
            //                                CMainLib.Ins.cVar.dVisionDegreeResult[uiCameraNo, j] = dDegreeResult;
            //                            }
            //                            else
            //                            {
            //                                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"CAM : {uiCameraNo}, {j} DEGREE_RESULT_FAIL");
            //                                return;
            //                            }
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"CAM : {uiCameraNo}, {j} GOOD_NG_FAIL");
            //                    return;
            //                }
            //            }
            //            CMainLib.Ins.cVar.bVisioinReceiveDone[uiCameraNo] = true;
            //        }
            //    }
            //}
            //catch
            //{
            //    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, "Vision Client receive exception : " + strReceiveData);
            //}
        }
    }
}