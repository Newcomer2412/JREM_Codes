namespace MachineControlBase
{
    /// <summary>
    /// 충돌 조건 관리 클래스
    /// </summary>
    public static class CInterLock
    {
        /// <summary>
        /// Safe 함수 연결
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetAxisSafety(eMotor motor)
        {
            switch (motor)
            {
                case eMotor.HOLDER_LOADING_X: return Holder_Loading_SafeCheck(); break;
                case eMotor.HOLDER_TRAY_Y: return Holder_Loading_SafeCheck(); break;
                case eMotor.MPC_FRONT_X: return MPC_Front_X_SafeCheck(); break;
                case eMotor.MPC_LEFT_Y: return MPC_Left_Y_SafeCheck(); break;
                case eMotor.MPC_RIGHT_Y: return MPC_Right_Y_SafeCheck(); break;
                case eMotor.PIPE_PnP_X: return PipePnP_XY_SafeCheck(); break;
                case eMotor.PIPE_PnP_Y: return PipePnP_XY_SafeCheck(); break;
                case eMotor.NEEDLE_PnP_X: return NeedlePnP_XY_SafeCheck(); break;
                case eMotor.NEEDLE_PnP_Y: return NeedlePnP_XY_SafeCheck(); break;
                case eMotor.PIPE_MOUNT_X: return PipeMount_XY_SafeCheck(); break;
                case eMotor.PIPE_MOUNT_Y: return PipeMount_XY_SafeCheck(); break;
                case eMotor.NEEDLE_MOUNT_X: return NeedleMount_XY_SafeCheck(); break;
                case eMotor.NEEDLE_MOUNT_Y: return NeedleMount_XY_SafeCheck(); break;
                case eMotor.FLIP_T: return Flip_T_SafeCheck(); break;
                case eMotor.DISPENSER_X: return Dispenser_XY_SafeCheck(); break;
                case eMotor.DISPENSER_Y: return Dispenser_XY_SafeCheck(); break;
            }
            return true;
        }

        #region Axis Safety Check

        /// <summary>
        /// 홀더 로딩 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool Holder_Loading_SafeCheck()
        {
            bool bSafe1 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true;
            return bSafe1;
        }

        /// <summary>
        /// 전면 MPC 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool MPC_Front_X_SafeCheck()
        {
            bool bSafe1 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_UP_ON) == true &&
                          CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_UP_ON) == true;

            double dFlip_Z_CmdPos = CMainLib.Ins.Axis[eMotor.FLIP_Z].GetCmdPostion();
            double dFlip_Z_Safety = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.FLIP_Z, (int)eAxisFlip_Z.FlipUp);
            bool bSafe2 = dFlip_Z_CmdPos >= dFlip_Z_Safety;

            double GetActLeftRail_Y = CMainLib.Ins.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
            double LeftRail_Y_FrontPos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
            double GetActRightRail_Y = CMainLib.Ins.Axis[eMotor.MPC_RIGHT_Y].GetActPostion();
            double RightRail_Y_FrontPos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.MPC_RIGHT_Y, (int)eAxisRightMPC_Y.Front);

            bool bSafe3 = GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 && GetActRightRail_Y >= RightRail_Y_FrontPos - 0.05 &&
                          GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05 && GetActRightRail_Y <= RightRail_Y_FrontPos + 0.05;

            bool bSafe4 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == true &&
                          CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true;

            return bSafe1 && bSafe2 && bSafe3 && bSafe4;
        }

        /// <summary>
        /// MPC 왼쪽 Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool MPC_Left_Y_SafeCheck()
        {
            bool bSafe1 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false;
            bool bSafe2 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == true &&
                          CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == true;
            return bSafe1 && bSafe2;
        }

        /// <summary>
        /// MPC 오른쪽 Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool MPC_Right_Y_SafeCheck()
        {
            bool bSafe1 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false;
            bool bSafe2 = CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == true &&
                          CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == true;
            return bSafe1 && bSafe2;
        }

        /// <summary>
        /// 파이프 PnP X,Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool PipePnP_XY_SafeCheck()
        {
            double dGetActPipePnP_Z = CMainLib.Ins.Axis[eMotor.PIPE_PnP_Z].GetActPostion();
            double dPipePnP_Z_SafePos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_PnP_Z, (int)eAxisPipePnP_Z.Safe);
            return dGetActPipePnP_Z >= dPipePnP_Z_SafePos - 1;
        }

        /// <summary>
        /// 니들 PnP X,Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool NeedlePnP_XY_SafeCheck()
        {
            double dGetActNeedlePnP_Z = CMainLib.Ins.Axis[eMotor.NEEDLE_PnP_Z].GetActPostion();
            double dNeedlePnP_Z_SafePos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_PnP_Z, (int)eAxisNeedlePnP_Z.Safe);
            return dGetActNeedlePnP_Z >= dNeedlePnP_Z_SafePos - 1;
        }

        /// <summary>
        /// 파이프 마운트 X,Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool PipeMount_XY_SafeCheck()
        {
            double dGetActPipeMount_Z = CMainLib.Ins.Axis[eMotor.PIPE_MOUNT_Z].GetActPostion();
            double dPipeMount_Z_SafePos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_MOUNT_Z, (int)eAxisPipeMount_Z.Safe);
            return dGetActPipeMount_Z >= dPipeMount_Z_SafePos - 1;
        }

        /// <summary>
        /// 니들 마운트 X,Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool NeedleMount_XY_SafeCheck()
        {
            double dGetActNeedleMount_Z = CMainLib.Ins.Axis[eMotor.NEEDLE_MOUNT_Z].GetActPostion();
            double dNeedleMount_Z_SafePos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Z, (int)eAxisNeedleMount_Z.Safe);
            return dGetActNeedleMount_Z >= dNeedleMount_Z_SafePos - 1;
        }

        /// <summary>
        /// 플립 T축 안전확인
        /// </summary>
        /// <returns></returns>
        public static bool Flip_T_SafeCheck()
        {
            double dGetActFlip_Z = CMainLib.Ins.Axis[eMotor.FLIP_Z].GetActPostion();
            double dFlip_Z_SafePos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.FLIP_Z, (int)eAxisFlip_Z.FlipUp);
            return dGetActFlip_Z >= dFlip_Z_SafePos - 1;
        }

        /// <summary>
        /// 디스펜서 X,Y축 안전 확인
        /// </summary>
        /// <returns></returns>
        public static bool Dispenser_XY_SafeCheck()
        {
            double dGetActDispenser_Z = CMainLib.Ins.Axis[eMotor.DISPENSER_Z].GetActPostion();
            double dDispenser_Z_SafePos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.DISPENSER_Z, (int)eAxisDispenser_Z.Safe);
            return dGetActDispenser_Z >= dDispenser_Z_SafePos - 1;
        }

        #endregion Axis Safety Check
    }
}